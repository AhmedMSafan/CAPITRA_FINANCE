using System.Globalization;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;

namespace Capitra.Application.Modules.Chq.Pages.Custom;

/// <summary>
/// The shape of the on-demand processes of App 122: an apex_json object (a null value is not written, as
/// apex_json.write leaves it out), dates as to_char masks, and errors as {ok: false, message: sqlerrm without its
/// ORA- prefix}. The FINOVA database's clock was UTC (its movement refs, built from sysdate, match the stored
/// times), so a stored time shows as it is and sysdate is the UTC time.
/// </summary>
internal static partial class ChqAjax
{
    public const int App = 122;

    /// <summary>An apex_json object: Add skips null values; a number is written as Oracle shows it (no trailing zeros).</summary>
    public sealed class Json : Dictionary<string, object?>
    {
        public new void Add(string key, object? value)
        {
            if (value is not null)
                this[key] = value is decimal d ? Normal(d) : value;
        }
    }

    /// <summary>25000.00 as 25000: the scale of a numeric column is not part of an Oracle NUMBER.</summary>
    public static decimal Normal(decimal d) => d / 1.000000000000000000000000000000000m;

    /// <summary>A Y/N column as the PL/SQL wrote it (the Capitra column is a boolean).</summary>
    public static string? Yn(object? value) => value switch
    {
        null => null,
        bool b => b ? "Y" : "N",
        _ => value.ToString(),
    };

    /// <summary>A stored time as the Oracle DATE it was.</summary>
    public static DateTime? Local(object? value) => value switch
    {
        DateTime t => t.Kind == DateTimeKind.Local ? t.ToUniversalTime() : t,
        DateTimeOffset o => o.UtcDateTime,
        DateOnly d => d.ToDateTime(TimeOnly.MinValue),
        _ => null,
    };

    /// <summary>to_char(d, 'DD Mon YYYY').</summary>
    public static string? Dmy(object? value) => Local(value)?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>to_char(d, 'DD Mon YYYY HH24:MI').</summary>
    public static string? DmyHm(object? value) => Local(value)?.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

    /// <summary>to_char(d, 'YYYY-MM-DD').</summary>
    public static string? Iso(object? value) => Local(value)?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>sysdate.</summary>
    public static DateTime Now(IClock clock) => DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);

    /// <summary>A date (an Oracle DATE without time) as it is stored: midnight of that day.</summary>
    public static DateTime Day(DateOnly date) => DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

    /// <summary>trunc(sysdate), stored.</summary>
    public static DateTime Today(IClock clock) => Day(DateOnly.FromDateTime(Now(clock)));

    /// <summary>to_date(x, 'YYYY-MM-DD'), stored; null when empty.</summary>
    public static DateTime? Date(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            throw new ApexProcessException("ORA-01861: literal does not match format string");
        return Day(d);
    }

    /// <summary>to_number(x); null when empty.</summary>
    public static decimal? Number(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        return decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var n)
            ? n
            : throw new ApexProcessException("ORA-01722: invalid number");
    }

    public static long? Long(string? text) => Number(text) is { } n ? (long)n : null;

    /// <summary>substr(x, 1, n).</summary>
    public static string? Substr(string? text, int length) => string.IsNullOrEmpty(text) ? null : text.Length <= length ? text : text[..length];

    /// <summary>The bind of a query's :ID.</summary>
    public static Dictionary<string, string?> Id(object? id) =>
        new(StringComparer.OrdinalIgnoreCase) { ["ID"] = Convert.ToString(id, CultureInfo.InvariantCulture) };

    public static readonly IReadOnlyDictionary<string, string?> NoItems = new Dictionary<string, string?>();

    /// <summary>apex_page.get_url of a page of App 122 with one item.</summary>
    public static string Url(int page, string? item = null, object? value = null) =>
        item is null ? ApexPages.Url(App, page) : ApexPages.Url(App, page, item, Convert.ToString(value, CultureInfo.InvariantCulture), request: null);

    /// <summary>regexp_replace(sqlerrm, '^ORA-[0-9]+: ', '').</summary>
    public static string Message(Exception e) => OraPrefix().Replace(e.GetBaseException().Message, string.Empty, 1);

    /// <summary>A process that changes data: one transaction; on any error rollback and {ok: false, message}.</summary>
    public static async Task<object> RunAsync(ICapitraDbContext db, Func<Task<object>> body, CancellationToken ct)
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
            return new Json { { "ok", false }, { "message", Message(e) } };
        }
    }

    [GeneratedRegex("^ORA-[0-9]+: ")]
    private static partial Regex OraPrefix();
}
