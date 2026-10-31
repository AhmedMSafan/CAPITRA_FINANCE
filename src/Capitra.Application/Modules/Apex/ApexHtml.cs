using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Capitra.Application.Modules.Apex;

/// <summary>
/// What the PL/SQL dynamic content regions of a copied page use to print their HTML: apex_escape, to_char with
/// FM format masks, and the small functions the FINOVA pages declare in each region (t, e, money, initials, show, url).
/// </summary>
public static partial class ApexHtml
{
    /// <summary>apex_escape.html</summary>
    public static string E(string? value) => value is null ? string.Empty : WebUtility.HtmlEncode(value).Replace("&#39;", "&#x27;", StringComparison.Ordinal);

    /// <summary>apex_escape.html_attribute</summary>
    public static string Attr(string? value) => E(value);

    /// <summary>t(p_en, p_ar): the English text, with both languages for the page's language switch.</summary>
    public static string T(string en, string ar) => $"<span data-finova-en=\"{Attr(en)}\" data-finova-ar=\"{Attr(ar)}\">{E(en)}</span>";

    /// <summary>money(p_n): 1.2B, 3.4M, 5.6K or the number, &amp;ndash; when null.</summary>
    public static string Money(decimal? n)
    {
        if (n is null)
            return "&ndash;";
        var a = Math.Abs(n.Value);
        if (a >= 1_000_000_000)
            return ToChar(n / 1_000_000_000, "FM999G990D0") + "B";
        if (a >= 1_000_000)
            return ToChar(n / 1_000_000, "FM999G990D0") + "M";
        if (a >= 1_000)
            return ToChar(n / 1_000, "FM999G990D0") + "K";
        return ToChar(n, "FM999G990");
    }

    /// <summary>show(p): a count with group separators, &amp;ndash; when null.</summary>
    public static string Show(decimal? n) => n is null ? "&ndash;" : ToChar(n, "FM999G999G990");

    /// <summary>initials(p): the first letters of the first and last word, or the first two letters.</summary>
    public static string Initials(string? p)
    {
        var l = NotAlpha().Replace(p ?? "?", " ").Trim();
        var i = l.IndexOf(' ', StringComparison.Ordinal);
        if (i > 0)
            return (l[..1] + l[(l.LastIndexOf(' ') + 1)..].PadRight(1)[..1]).ToUpperInvariant();
        return (l.Length > 2 ? l[..2] : l).ToUpperInvariant();
    }

    /// <summary>url(p_page, p_items, p_values, p_clear): apex_page.get_url of a page of App 105, escaped for an attribute.</summary>
    public static string Url(int page, string? items = null, string? values = null, string? request = null) =>
        Attr(ApexPages.Url(105, page, items, values, request));

    /// <summary>
    /// to_char(number, format) for the FM masks the pages use: 9 and 0 digits, G group separator, D decimal point,
    /// FM no padding. NLS is the default English one: G is ',' and D is '.'; rounding is half away from zero.
    /// </summary>
    public static string ToChar(decimal? n, string mask)
    {
        if (n is null)
            return string.Empty;
        var fm = mask.StartsWith("FM", StringComparison.OrdinalIgnoreCase);
        var m = fm ? mask[2..] : mask;
        var d = m.IndexOfAny(['D', '.']);
        var decimals = d < 0 ? 0 : m.Length - d - 1;
        var intPart = d < 0 ? m : m[..d];
        var group = intPart.Contains('G', StringComparison.Ordinal) || intPart.Contains(',', StringComparison.Ordinal);
        var zero = intPart.IndexOf('0', StringComparison.Ordinal);
        var minInt = zero < 0 ? 0 : intPart[zero..].Count(c => c is '9' or '0');     // a 0 forces the digits from there on
        var digits = intPart.Count(c => c is '9' or '0');
        var v = Math.Round(n.Value, decimals, MidpointRounding.AwayFromZero);
        if (Math.Abs(Math.Truncate(v)).ToString(CultureInfo.InvariantCulture).Length > digits && Math.Truncate(v) != 0)
            return new string('#', mask.Length - (fm ? 2 : 0) + 1);

        // decimals: FM drops trailing 9-position zeros, keeps 0-position ones
        var fixedText = Math.Abs(v).ToString("F" + decimals, CultureInfo.InvariantCulture);
        var parts = fixedText.Split('.');
        var ip = parts[0];
        if (ip == "0" && minInt == 0)
            ip = string.Empty;
        ip = ip.PadLeft(minInt, '0');
        if (group && ip.Length > 3)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < ip.Length; i++)
            {
                if (i > 0 && (ip.Length - i) % 3 == 0)
                    sb.Append(',');
                sb.Append(ip[i]);
            }

            ip = sb.ToString();
        }

        var dp = string.Empty;
        if (decimals > 0)
        {
            var frac = parts[1];
            var decMask = m[(d + 1)..];
            if (fm)
            {
                var keep = decMask.LastIndexOf('0') + 1;
                frac = frac.TrimEnd('0');
                if (frac.Length < keep)
                    frac = frac.PadRight(keep, '0');
            }

            dp = "." + frac;
        }

        var text = ip + dp;
        if (text.Length == 0 || text == ".")
            text = "0";
        if (v < 0)
            text = "-" + text;
        return fm ? text : text.PadLeft(mask.Length + 1);
    }

    [GeneratedRegex(@"[^\p{L} ]")]
    private static partial Regex NotAlpha();
}
