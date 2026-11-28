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

    /// <summary>
    /// One page of the query, sorted and counted in the database (see <see cref="ApexSqlPaging"/>): only the page's rows are read.
    /// The default reads every row and pages in memory, for readers that cannot do better.
    /// </summary>
    async Task<ApexSqlPage> QueryPageAsync(
        string sql, IReadOnlyDictionary<string, string?> items, string? sortColumn, bool descending, int offset, int limit,
        CancellationToken cancellationToken)
    {
        var all = await QueryAsync(sql, items, cancellationToken);
        IEnumerable<IReadOnlyDictionary<string, object?>> rows = all;
        if (sortColumn is not null && all.Count > 0 && all[0].ContainsKey(sortColumn))
            rows = descending ? rows.OrderByDescending(r => r[sortColumn], ApexSqlPaging.Comparer) : rows.OrderBy(r => r[sortColumn], ApexSqlPaging.Comparer);
        return new ApexSqlPage(all.Count, [.. rows.Skip(offset).Take(limit)]);
    }
}

/// <summary>A page of a report query and the number of rows of the whole query.</summary>
public sealed record ApexSqlPage(long Total, IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows);

/// <summary>
/// The SQL that pages a report query in PostgreSQL: the query is wrapped as a subquery, sorted by one of its own columns (nulls
/// last ascending, first descending, as the in-memory sort did), then LIMIT / OFFSET; the count is a second, separate query.
/// </summary>
public static class ApexSqlPaging
{
    /// <summary>The query without trailing semicolons and blanks, so it can be wrapped.</summary>
    public static string Inner(string sql) => sql.TrimEnd().TrimEnd(';').TrimEnd();

    /// <summary>A column name as a quoted PostgreSQL identifier.</summary>
    public static string Quote(string column) => "\"" + column.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    /// <summary>The query with no rows: run it to learn the column names.</summary>
    public static string Columns(string sql) => $"SELECT * FROM ({Inner(sql)}) apex_q LIMIT 0";

    public static string Count(string sql) => $"SELECT count(*) FROM ({Inner(sql)}) apex_q";

    /// <summary>The page; <paramref name="sortColumn"/> must be one of <paramref name="columns"/> or it is ignored (the query's own order).</summary>
    public static string Page(string sql, IReadOnlyCollection<string> columns, string? sortColumn, bool descending, int offset, int limit)
    {
        var order = sortColumn is not null && columns.Contains(sortColumn)
            ? $" ORDER BY apex_q.{Quote(sortColumn)} {(descending ? "DESC NULLS FIRST" : "ASC NULLS LAST")}"
            : string.Empty;
        return $"SELECT * FROM ({Inner(sql)}) apex_q{order} LIMIT {Math.Max(1, limit)} OFFSET {Math.Max(0, offset)}";
    }

    /// <summary>Nulls last, then numbers, dates and text in their natural order (text without case).</summary>
    public static readonly IComparer<object?> Comparer = new ValueComparer();

    private sealed class ValueComparer : IComparer<object?>
    {
        public int Compare(object? x, object? y) => (x, y) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            (string a, string b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase),
            (IComparable a, _) when x.GetType() == y.GetType() => a.CompareTo(y),
            _ => string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase),
        };
    }
}
