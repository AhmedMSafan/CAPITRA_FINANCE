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

            // Keys and indexes of child tables use the approved short form, so names stay within PostgreSQL's 63 characters.
            var owner = KeyPrefix(table);

            var foreignKeys = entity.GetForeignKeys().ToList();
            foreach (var foreignKey in foreignKeys)
            {
                var principal = foreignKey.PrincipalEntityType.GetTableName();
                var samePrincipal = foreignKeys.Count(x => x.PrincipalEntityType == foreignKey.PrincipalEntityType) > 1;
                var name = $"FK_{owner}_{principal}";

                if (samePrincipal)
                    name += "_" + string.Join("_", foreignKey.Properties.Select(p => p.GetColumnName()));

                foreignKey.SetConstraintName(Fit(name));
            }

            var indexNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var index in entity.GetIndexes())
            {
                // Trailing columns are left out of the name when it would pass PostgreSQL's 63 characters.
                var columns = index.Properties.Select(p => p.GetColumnName()).ToList();
                string name;
                while ((name = $"{(index.IsUnique ? "UQ" : "IX")}_{owner}_{string.Join("_", columns)}").Length > 63 && columns.Count > 1)
                    columns.RemoveAt(columns.Count - 1);

                // Two indexes cut down to the same leading columns get a short hash of the full column list.
                if (!indexNames.Add(name))
                {
                    var full = string.Join("_", index.Properties.Select(p => p.GetColumnName()));
                    name = Fit($"{name}_{Hash(full)}", force: true);
                    indexNames.Add(name);
                }

                index.SetDatabaseName(Fit(name));
            }
        }
    }

    /// <summary>Names over PostgreSQL's 63 characters are cut to 54 and end in a short hash, so they stay unique.</summary>
    private static string Fit(string name, bool force = false)
    {
        if (name.Length <= 63 && !force)
            return name;

        var hash = Hash(name);
        return name.Length <= 63 ? name : $"{name[..54]}_{hash}";
    }

    private static string Hash(string value) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))[..8];

    /// <summary>MTG_PURCHASED_PORTFOLIO_ACCOUNTS → MTG_PP_ACCOUNTS (approved abbreviation PP, keys and indexes only).</summary>
    private static string KeyPrefix(string table) =>
        table.StartsWith("MTG_PURCHASED_PORTFOLIO_", StringComparison.Ordinal)
            ? "MTG_PP_" + table["MTG_PURCHASED_PORTFOLIO_".Length..]
            : table;

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
