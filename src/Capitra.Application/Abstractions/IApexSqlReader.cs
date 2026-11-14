namespace Capitra.Application.Abstractions;

/// <summary>
/// Runs the read-only report and list-of-values queries of copied APEX pages (translated to PostgreSQL by
/// tools/oracle-port/apex_sql.py). Parameters are the page items the query names as @ITEM; an empty item is null,
/// as in APEX. Values come back as .NET values: dates as DateOnly, timestamps as UTC DateTime, booleans, decimals.
/// </summary>
public interface IApexSqlReader
{
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
        string sql, IReadOnlyDictionary<string, string?> items, CancellationToken cancellationToken);
}
