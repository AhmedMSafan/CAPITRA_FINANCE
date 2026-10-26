using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// The shape of the on-demand processes of the workflow step pages: apex_json {success, message}, and on any error
/// "rollback; success false, message sqlerrm".
/// </summary>
internal static class MtgAjax
{
    public static async Task<object> RunAsync(ICapitraDbContext db, Func<Task<string?>> body, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var message = await body();
            await tx.CommitAsync(ct);
            return message is null ? new { success = true } : new { success = true, message };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { success = false, message = e.Message };
        }
    }

    /// <summary>The same, for a process that writes its own apex_json object on success.</summary>
    public static async Task<object> RunJsonAsync(ICapitraDbContext db, Func<Task<object>> body, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var result = await body();
            await tx.CommitAsync(ct);
            return result;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { success = false, message = e.Message };
        }
    }

    private static readonly TimeZoneInfo CairoZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>sysdate / systimestamp: the database's clock is Cairo time.</summary>
    public static DateTime CairoNow(IClock clock) => TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, CairoZone);

    /// <summary>A stored UTC time in Cairo time.</summary>
    public static DateTime? Cairo(DateTime? utc) =>
        utc is null ? null : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), CairoZone);

    /// <summary>Oracle months_between(d1, d2): whole months when the days match or both are month ends, else the rest over 31 days.</summary>
    public static decimal MonthsBetween(DateOnly d1, DateOnly d2)
    {
        var months = (d1.Year - d2.Year) * 12 + d1.Month - d2.Month;
        var end1 = d1.Day == DateTime.DaysInMonth(d1.Year, d1.Month);
        var end2 = d2.Day == DateTime.DaysInMonth(d2.Year, d2.Month);
        return d1.Day == d2.Day || (end1 && end2) ? months : months + (d1.Day - d2.Day) / 31m;
    }

    /// <summary>to_number(p): null when empty, ORA-01722 when it is not a number.</summary>
    public static decimal? Number(string? p) =>
        string.IsNullOrWhiteSpace(p) ? null
        : decimal.TryParse(p.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d
        : throw new ApexProcessException("ORA-01722: invalid number");

    public static long? Long(string? p) => Number(p) is { } d ? (long)d : null;

    /// <summary>upper(trim(p)), null when empty.</summary>
    public static string? Upper(string? p) => string.IsNullOrWhiteSpace(p) ? null : p.Trim().ToUpperInvariant();

    /// <summary>nullif(trim(p),'').</summary>
    public static string? Trim(string? p) => string.IsNullOrWhiteSpace(p) ? null : p.Trim();
}
