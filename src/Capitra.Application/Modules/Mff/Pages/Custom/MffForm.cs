using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Pages;
using Capitra.Application.Modules.Mtg.Pages.Custom;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>
/// The shape of the After Submit processes of the app 110 pages: one transaction, apex_application.g_print_success_message
/// on success, and on any error a rollback with the error text shown on the page.
/// </summary>
internal static class MffForm
{
    public static async Task<object> RunAsync(ICapitraDbContext db, Func<Task<(string Message, Dictionary<string, string?>? Items)>> body, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var (message, items) = await body();
            await tx.CommitAsync(ct);
            return items is null ? new { ok = true, message } : new { ok = true, message, items };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = (e.GetBaseException() is { } b && b != e ? b : e).Message };
        }
    }

    public static decimal? Number(ApexRequest request, string item) => MtgAjax.Number(request.Item(item));

    public static long? Long(ApexRequest request, string item) => MtgAjax.Long(request.Item(item));

    public static int? Int(ApexRequest request, string item) => Number(request, item) is { } d ? (int)d : null;

    public static string? Text(ApexRequest request, string item) => request.Item(item);

    /// <summary>A date item, sent as yyyy-mm-dd.</summary>
    public static DateOnly? Date(ApexRequest request, string item)
    {
        var v = MtgAjax.Trim(request.Item(item));
        if (v is null)
            return null;
        return DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
            : throw new ApexProcessException("ORA-01861: literal does not match format string");
    }

    /// <summary>sysdate as a date (Cairo time).</summary>
    public static DateOnly Today(IClock clock) => DateOnly.FromDateTime(MtgAjax.CairoNow(clock));

    /// <summary>A NOT NULL column: ORA-01400 as Oracle raised it on insert.</summary>
    public static T Req<T>(T? value, string table, string column) where T : struct =>
        value ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"" + table + "\".\"" + column + "\")");

    public static string Req(string? value, string table, string column) =>
        value ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"" + table + "\".\"" + column + "\")");

    /// <summary>A "Load ..." button that runs the same block as the page's Before Header load: the items it sets, none when no row.</summary>
    public static async Task<Dictionary<string, string?>?> LoadAsync(IApexSqlReader reader, int page, ApexRequest request, CancellationToken ct)
    {
        var load = MffApexSql.Loads.First(x => x.Page == page);
        var rows = await reader.QueryAsync(load.Sql, request.Items ?? [], ct);
        return rows.Count == 0 ? null : rows[0].ToDictionary(c => c.Key, c => ApexSqlLovValues.Text(c.Value));
    }

    public static string Id(long id) => id.ToString(CultureInfo.InvariantCulture);
}
