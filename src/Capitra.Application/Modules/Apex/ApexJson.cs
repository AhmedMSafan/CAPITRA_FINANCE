using System.Globalization;
using System.Text.Json;

namespace Capitra.Application.Modules.Apex;

/// <summary>Reads a JSON object the way apex_json.get_varchar2 / get_number did: any scalar as text, missing as null.</summary>
public sealed class ApexJson
{
    private readonly Dictionary<string, string?> values = new(StringComparer.Ordinal);

    public ApexJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        using var doc = JsonDocument.Parse(json);
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            values[p.Name] = p.Value.ValueKind switch
            {
                JsonValueKind.String => p.Value.GetString(),
                JsonValueKind.Number => p.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null,
            };
        }
    }

    /// <summary>get_varchar2 as written (no trim).</summary>
    public string? Raw(string key) => values.GetValueOrDefault(key) is { Length: > 0 } v ? v : null;

    /// <summary>trim(get_varchar2(key)); an empty string is NULL, as in Oracle.</summary>
    public string? Str(string key) => Text(Raw(key));

    public int? Number(string key) => Str(key) is { } v ? (int)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    public long? Key(string key) => Str(key) is { } v ? (long)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    /// <summary>to_date(value, 'YYYY-MM-DD').</summary>
    public DateOnly? Date(string key) =>
        Str(key) is { } v ? DateOnly.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture) : null;

    /// <summary>Oracle treats '' as NULL; trims like TRIM().</summary>
    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string Yn(bool value) => value ? "Y" : "N";

    public static string? Ymd(DateOnly? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
