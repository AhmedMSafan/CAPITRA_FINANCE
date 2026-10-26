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
