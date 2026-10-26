using System.Globalization;
using System.Text.Json;
using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Apex;

/// <summary>
/// An interactive report of a copied page whose source is a SQL query (not one table): the query, translated to
/// PostgreSQL by tools/oracle-port/apex_sql.py, runs with the page items it binds.
/// </summary>
public sealed record ApexSqlReport(int App, int Page, string Key, string Permission, string Sql);

/// <summary>A select list of a copied page whose values come from a SQL list of values (first column shown, second returned).</summary>
public sealed record ApexSqlLov(int App, int Page, string Item, string Permission, string Sql);

/// <summary>
/// The page-specific processes APEX ran around a wizard form: "Set Defaults" before a new row is shown, and the
/// processes after Process form (store an upload, recalculate the portfolio). One per page, found by app and page.
/// </summary>
public interface IApexFormHooks
{
    int App { get; }

    int Page { get; }

    /// <summary>Initialize form for a new row: fill defaults into the values (by column), knowing the page items.</summary>
    Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct) => Task.CompletedTask;

    /// <summary>Before the row is saved (CREATE or SAVE): change or check the values (by column); throw ApexProcessException to stop.</summary>
    Task BeforeDmlAsync(string request, IDictionary<string, string?> values, CancellationToken ct) => Task.CompletedTask;

    /// <summary>After Process form, in the same unit of work: <paramref name="id"/> is the row's key.</summary>
    Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct) => Task.CompletedTask;
}

/// <summary>Page items sent with a request as a JSON object {ITEM: value}.</summary>
public static class ApexItems
{
    public static Dictionary<string, string?> Read(JsonElement? items)
    {
        var res = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (items is { ValueKind: JsonValueKind.Object } o)
        {
            foreach (var p in o.EnumerateObject())
                res[p.Name.ToUpperInvariant()] = p.Value.ValueKind == JsonValueKind.Null ? null : p.Value.ToString();
        }

        return res;
    }

    public static Dictionary<string, string?> Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        using var doc = JsonDocument.Parse(json);
        return Read(doc.RootElement);
    }
}

/// <summary>IR_&lt;key&gt; of a SQL report: x01 {search, sort, dir, first, rows, items}.</summary>
internal sealed class ApexSqlReportProcess(ApexSqlReport report, IApexSqlReader reader, ICurrentUser user) : IApexProcess
{
    public int App => report.App;

    public int Page => report.Page;

    public string Name => "IR_" + report.Key;

    public string Permission => report.Permission;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
        var o = doc.RootElement;
        var items = ApexItems.Read(o.TryGetProperty("items", out var i) ? i : null);
        items["APP_USER"] = user.UserName;
        var rows = await reader.QueryAsync(report.Sql, items, ct);

        // the query's own order is the report's default order
        var index = 0;
        var list = rows.Select(r => new ApexReport.Row(index++, new Dictionary<string, object?>(r))).ToList();
        return ApexReport.Page(list, o);
    }
}

/// <summary>LOVS: the values of every select list of one page, as {ITEM: [[display, return], ...]}.</summary>
internal sealed class ApexSqlLovValues(IApexSqlReader reader)
{
    public async Task<List<object?[]>> ValuesAsync(ApexSqlLov lov, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        var rows = await reader.QueryAsync(lov.Sql, items, ct);
        return rows.Select(r => r.Values.Take(2).ToArray())
            .Where(v => v.Length == 2 && v[1] is not null)
            .Select(v => new object?[] { Text(v[0]), Text(v[1]) })
            .ToList();
    }

    private static string? Text(object? value) => value switch
    {
        null => null,
        bool b => b ? "Y" : "N",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}
