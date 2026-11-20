using Capitra.Application.Abstractions;
using Xunit;

namespace Capitra.Tests.Apex;

/// <summary>Report queries are paged by PostgreSQL: the wrapping SQL, and the in-memory fallback it replaces.</summary>
public sealed class ApexSqlPagingTests
{
    private const string Sql = "select ID, NAME from T where X = @P1_X order by NAME;  ";

    [Fact]
    public void The_query_is_wrapped_without_its_semicolon()
    {
        Assert.Equal("SELECT count(*) FROM (select ID, NAME from T where X = @P1_X order by NAME) apex_q", ApexSqlPaging.Count(Sql));
        Assert.Equal("SELECT * FROM (select ID, NAME from T where X = @P1_X order by NAME) apex_q LIMIT 0", ApexSqlPaging.Columns(Sql));
    }

    [Fact]
    public void A_page_sorts_only_by_a_column_of_the_query()
    {
        string[] cols = ["id", "name"];
        Assert.EndsWith("apex_q ORDER BY apex_q.\"name\" ASC NULLS LAST LIMIT 15 OFFSET 30", ApexSqlPaging.Page(Sql, cols, "name", false, 30, 15));
        Assert.EndsWith("apex_q ORDER BY apex_q.\"id\" DESC NULLS FIRST LIMIT 15 OFFSET 0", ApexSqlPaging.Page(Sql, cols, "id", true, 0, 15));
        // anything else keeps the query's own order: a column name is never put in the SQL unless the query has it
        Assert.EndsWith("apex_q LIMIT 15 OFFSET 0", ApexSqlPaging.Page(Sql, cols, "name\"; drop table T; --", false, 0, 15));
        Assert.EndsWith("apex_q LIMIT 1 OFFSET 0", ApexSqlPaging.Page(Sql, cols, null, false, -5, 0));
    }

    [Fact]
    public void Identifiers_are_quoted()
    {
        Assert.Equal("\"a\"\"b\"", ApexSqlPaging.Quote("a\"b"));
    }

    private sealed class InMemory(params Dictionary<string, object?>[] rows) : IApexSqlReader
    {
        public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
            string sql, IReadOnlyDictionary<string, string?> items, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(rows);
    }

    [Fact]
    public async Task The_fallback_pages_in_memory_with_nulls_last()
    {
        IApexSqlReader reader = new InMemory(
            new() { ["n"] = "b" }, new() { ["n"] = null }, new() { ["n"] = "A" }, new() { ["n"] = "c" });
        var page = await reader.QueryPageAsync("q", new Dictionary<string, string?>(), "n", false, 1, 2, CancellationToken.None);
        Assert.Equal(4, page.Total);
        Assert.Equal(["b", "c"], page.Rows.Select(r => r["n"]));
        var desc = await reader.QueryPageAsync("q", new Dictionary<string, string?>(), "n", true, 0, 1, CancellationToken.None);
        Assert.Null(desc.Rows[0]["n"]);
    }
}
