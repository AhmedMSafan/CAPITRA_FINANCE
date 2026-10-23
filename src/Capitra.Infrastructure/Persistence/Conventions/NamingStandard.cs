using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Capitra.Infrastructure.Persistence.Conventions;

/// <summary>
/// Applies the Capitra naming standard v1.0 to the whole model after every entity is configured:
/// UPPER_SNAKE column names, PK_/FK_/IX_/UQ_ constraint names, money and rate precision,
/// and the RowVersion concurrency check on every audited table.
/// Table names are never derived: each entity configuration must call ToTable explicitly.
/// Owned types are skipped because they are stored in the owner's table or in a JSON column.
/// </summary>
public static class NamingStandard
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Owned types live inside their owner's table (or a JSON column); the owner carries the naming.
            if (entity.IsOwned())
                continue;

            if (entity.FindAnnotation(RelationalAnnotationNames.TableName) is null)
                throw new InvalidOperationException(
                    $"Entity '{entity.ClrType.Name}' has no explicit table name. Call ToTable(\"PREFIX_ENTITY\") in its configuration.");

            var table = entity.GetTableName()!;

            foreach (var property in entity.GetProperties())
            {
                if (property.FindAnnotation(RelationalAnnotationNames.ColumnName) is null)
                    property.SetColumnName(DbNames.ToUpperSnake(property.Name));

                ApplyNumericPrecision(property);
            }

            if (typeof(IAuditable).IsAssignableFrom(entity.ClrType))
                entity.FindProperty(nameof(IAuditable.RowVersion))!.IsConcurrencyToken = true;

            entity.FindPrimaryKey()?.SetName($"PK_{table}");

            var foreignKeys = entity.GetForeignKeys().ToList();
            foreach (var foreignKey in foreignKeys)
            {
                var principal = foreignKey.PrincipalEntityType.GetTableName();
                var samePrincipal = foreignKeys.Count(x => x.PrincipalEntityType == foreignKey.PrincipalEntityType) > 1;
                var name = $"FK_{table}_{principal}";

                if (samePrincipal)
                    name += "_" + string.Join("_", foreignKey.Properties.Select(p => p.GetColumnName()));

                foreignKey.SetConstraintName(name);
            }

            foreach (var index in entity.GetIndexes())
            {
                var columns = string.Join("_", index.Properties.Select(p => p.GetColumnName()));
                index.SetDatabaseName($"{(index.IsUnique ? "UQ" : "IX")}_{table}_{columns}");
            }
        }
    }

    private static void ApplyNumericPrecision(IMutableProperty property)
    {
        var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
        if (type != typeof(decimal) || property.GetPrecision() is not null)
            return;

        if (property.Name.EndsWith("Amount", StringComparison.Ordinal))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
        else if (property.Name.EndsWith("Pct", StringComparison.Ordinal) || property.Name.EndsWith("Rate", StringComparison.Ordinal))
        {
            property.SetPrecision(9);
            property.SetScale(6);
        }
    }
}
