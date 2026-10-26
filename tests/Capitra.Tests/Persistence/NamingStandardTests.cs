using System.Text.RegularExpressions;
using Capitra.Domain.Common;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Capitra.Tests.Persistence;

/// <summary>
/// Enforces the Capitra naming standard v1.0 on the database model. A new table or column that breaks
/// the standard fails the build here, so the standard cannot drift.
/// </summary>
public sealed partial class NamingStandardTests
{
    private static readonly string[] ModulePrefixes =
        ["ADM_", "MTG_", "FCT_", "FIN_", "GL_", "WFE_", "CHW_", "COL_", "CRM_", "TRS_", "TLS_", "SEC_", "RPT_", "SYS_"];

    private static readonly string[] AuditColumns = ["CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION"];

    private static readonly string[] BareColumnNames = ["ID", "NAME", "TYPE", "STATUS", "DATE", "VALUE", "CODE_ID", "DESC"];

    /// <summary>History tables end in HIST or HISTORY and are not pluralised; every other table name ends in a plural word.</summary>
    private static readonly string[] CollectiveLastWords = ["HIST", "HISTORY"];

    /// <summary>Singular words that end in S, so a table ending in one of them is not plural.</summary>
    private static readonly string[] SingularWordsEndingInS = ["STATUS", "ADDRESS", "CLASS", "BASIS"];

    private static readonly IModel Model = new CapitraDbContextFactory().CreateDbContext([]).GetService<IDesignTimeModel>().Model;

    private static IEnumerable<IEntityType> Tables => Model.GetEntityTypes().Where(e => !e.IsOwned());

    public static TheoryData<string> TableNames => [.. Tables.Select(e => e.GetTableName()!)];

    [GeneratedRegex("^[A-Z][A-Z0-9]*(_[A-Z0-9]+)*$")]
    private static partial Regex UpperSnake();

    [Fact]
    public void Model_has_tables() => Assert.NotEmpty(Tables);

    [Fact]
    public void Table_names_follow_the_standard()
    {
        var errors = new List<string>();
        foreach (var table in Tables.Select(e => e.GetTableName()!))
        {
            if (!UpperSnake().IsMatch(table))
                errors.Add($"{table}: must be UPPER_SNAKE_CASE");
            if (table.Length > 50)
                errors.Add($"{table}: longer than 50 characters");
            if (!ModulePrefixes.Any(table.StartsWith))
                errors.Add($"{table}: must start with a module prefix ({string.Join(", ", ModulePrefixes)})");

            var lastWord = table[(table.LastIndexOf('_') + 1)..];
            if (!CollectiveLastWords.Contains(lastWord) && (!lastWord.EndsWith('S') || SingularWordsEndingInS.Contains(lastWord)))
                errors.Add($"{table}: table names are plural");
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Column_names_and_types_follow_the_standard()
    {
        var errors = new List<string>();
        foreach (var entity in Tables)
        {
            var table = entity.GetTableName()!;
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());

            foreach (var property in entity.GetProperties())
            {
                var column = property.GetColumnName(store);
                if (column is null)
                    continue;

                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                var where = $"{table}.{column}";

                if (!UpperSnake().IsMatch(column))
                    errors.Add($"{where}: must be UPPER_SNAKE_CASE");
                if (column.Length > 63)
                    errors.Add($"{where}: longer than 63 characters");
                if (BareColumnNames.Contains(column))
                    errors.Add($"{where}: bare generic name, qualify it (e.g. PORTFOLIO_STATUS_CODE)");

                if (type == typeof(bool) && !column.StartsWith("IS_", StringComparison.Ordinal))
                    errors.Add($"{where}: boolean columns start with IS_");
                if (column.StartsWith("IS_", StringComparison.Ordinal) && type != typeof(bool))
                    errors.Add($"{where}: IS_ columns are boolean");

                if (column.EndsWith("_AT", StringComparison.Ordinal) && type != typeof(DateTime) && type != typeof(DateTimeOffset))
                    errors.Add($"{where}: _AT columns are timestamps");
                if (type == typeof(DateTime) && !column.EndsWith("_AT", StringComparison.Ordinal))
                    errors.Add($"{where}: timestamp columns end with _AT");
                if (column.EndsWith("_DATE", StringComparison.Ordinal) && type != typeof(DateOnly))
                    errors.Add($"{where}: _DATE columns are DateOnly");

                if (column.EndsWith("_AMOUNT", StringComparison.Ordinal) && (type != typeof(decimal) || property.GetPrecision() < 18 || property.GetScale() < 2))
                    errors.Add($"{where}: _AMOUNT columns are numeric(18,2) or wider (copied APEX columns)");
                if ((column.EndsWith("_PCT", StringComparison.Ordinal) || column.EndsWith("_RATE", StringComparison.Ordinal))
                    && (type != typeof(decimal) || property.GetPrecision() < 9 || property.GetScale() < 6))
                    errors.Add($"{where}: _PCT and _RATE columns are numeric(9,6) or wider (copied APEX columns)");
                if (type == typeof(decimal) && property.GetPrecision() is null)
                    errors.Add($"{where}: decimal columns need a suffix (_AMOUNT, _PCT, _RATE) or an explicit precision");
                if (column.EndsWith("_COUNT", StringComparison.Ordinal) && type != typeof(int) && type != typeof(long))
                    errors.Add($"{where}: _COUNT columns are integers");
            }
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Every_table_has_the_audit_columns()
    {
        var errors = new List<string>();
        foreach (var entity in Tables)
        {
            var table = entity.GetTableName()!;
            if (!typeof(IAuditable).IsAssignableFrom(entity.ClrType))
            {
                errors.Add($"{table}: entity {entity.ClrType.Name} must implement IAuditable");
                continue;
            }

            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());
            var columns = entity.GetProperties().Select(p => p.GetColumnName(store)).ToHashSet();
            errors.AddRange(AuditColumns.Where(c => !columns.Contains(c)).Select(c => $"{table}: missing {c}"));

            if (entity.FindProperty(nameof(IAuditable.RowVersion))?.IsConcurrencyToken != true)
                errors.Add($"{table}: ROW_VERSION must be a concurrency token");
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Constraint_names_follow_the_standard()
    {
        var errors = new List<string>();
        foreach (var entity in Tables)
        {
            var table = entity.GetTableName()!;
            // Approved abbreviation PP (purchased portfolio) for keys and indexes of the portfolio's child tables.
            var owner = table.StartsWith("MTG_PURCHASED_PORTFOLIO_", StringComparison.Ordinal)
                ? "MTG_PP_" + table["MTG_PURCHASED_PORTFOLIO_".Length..]
                : table;

            if (entity.FindPrimaryKey()?.GetName() is { } pk && pk != $"PK_{table}")
                errors.Add($"{table}: primary key is {pk}, expected PK_{table}");

            foreach (var fk in entity.GetForeignKeys())
                if (fk.GetConstraintName() is { } name && (!name.StartsWith($"FK_{owner}_", StringComparison.Ordinal) || name.Length > 63))
                    errors.Add($"{table}: foreign key {name} must be FK_{owner}_<PARENT> and at most 63 characters");

            foreach (var index in entity.GetIndexes())
            {
                var expected = (index.IsUnique ? "UQ_" : "IX_") + owner + "_";
                var name = index.GetDatabaseName();
                if (name is null || !name.StartsWith(expected, StringComparison.Ordinal) || name.Length > 63)
                    errors.Add($"{table}: index {name} must be {expected}<COLUMNS> and at most 63 characters");
            }
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Lookup_tables_use_the_lookup_shape()
    {
        var lookups = Tables.Where(e => typeof(LookupEntity).IsAssignableFrom(e.ClrType)).ToList();
        Assert.NotEmpty(lookups);

        foreach (var entity in lookups)
        {
            var table = entity.GetTableName()!;
            Assert.Contains("_LKP_", table, StringComparison.Ordinal);
            Assert.Equal(["CODE"], entity.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));
            Assert.InRange(entity.FindProperty(nameof(LookupEntity.Code))!.GetMaxLength() ?? int.MaxValue, 1, LookupEntity.CodeMaxLength);
        }
    }
}
