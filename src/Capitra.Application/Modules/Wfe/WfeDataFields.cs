using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using Capitra.Application.Common.Exceptions;

namespace Capitra.Application.Modules.Wfe;

/// <summary>
/// A data field a workflow declares in the designer (K2's data fields): the values each instance carries, such as the amount,
/// the client or a decision. Stored on the BPMN process as finova:variables, so older workflows with name, type and required
/// keep working.
/// </summary>
/// <param name="Type">String (text), Number (whole number), Decimal (amount), Boolean (yes/no), Date, Choice or User.</param>
public sealed record WfeDataField(
    string Name, string? Label, string Type, bool Required, string? DefaultValue, IReadOnlyList<string> Choices, string? Description)
{
    public string Caption => string.IsNullOrWhiteSpace(Label) ? Name : Label!;
}

public sealed record WfeDataValueDto(string Name, string Label, string Type, bool Required, string? Value, string? Description);

/// <summary>The data of one instance: its declared fields in the designer's order, then any other values modules stored.</summary>
public sealed record WfeInstanceDataDto(IReadOnlyList<WfeDataValueDto> Fields, IReadOnlyList<WfeDataValueDto> Other);

public static class WfeDataFields
{
    public static readonly string[] Types = ["String", "Number", "Decimal", "Boolean", "Date", "Choice", "User"];

    private static readonly XNamespace Bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
    private static readonly XNamespace Finova = "http://finova/workflow";
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions JsonOut = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed class Raw
    {
        public string? Name { get; set; }
        public string? Label { get; set; }
        public string? Type { get; set; }
        public bool Required { get; set; }
        public string? DefaultValue { get; set; }
        public List<string>? Choices { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>The fields a diagram declares; none when it declares none or the list cannot be read.</summary>
    public static IReadOnlyList<WfeDataField> Of(string xml)
    {
        string? json;
        try
        {
            var process = XDocument.Parse(xml).Descendants(Bpmn + "process").FirstOrDefault();
            json = (string?)process?.Attribute(Finova + "variables") ?? (string?)process?.Attribute("variables");
        }
        catch (System.Xml.XmlException)
        {
            return [];
        }

        return Parse(json);
    }

    /// <summary>Fields stored as JSON (finova:variables or a document's FIELDS_JSON); none when the list cannot be read.</summary>
    public static IReadOnlyList<WfeDataField> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        List<Raw>? rows;
        try
        {
            rows = JsonSerializer.Deserialize<List<Raw>>(json, Json);
        }
        catch (JsonException)
        {
            return [];
        }

        return (rows ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new WfeDataField(
                x.Name!.Trim(), x.Label?.Trim(), Types.FirstOrDefault(t => string.Equals(t, x.Type?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "String",
                x.Required, string.IsNullOrWhiteSpace(x.DefaultValue) ? null : x.DefaultValue.Trim(),
                (x.Choices ?? []).Select(c => c.Trim()).Where(c => c.Length > 0).ToList(), x.Description?.Trim()))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
            .ToList();
    }

    public static string ToJson(IEnumerable<WfeDataField> fields) =>
        JsonSerializer.Serialize(fields.Select(f => new Raw
        {
            Name = f.Name, Label = f.Label, Type = f.Type, Required = f.Required, DefaultValue = f.DefaultValue,
            Choices = f.Choices.Count == 0 ? null : f.Choices.ToList(), Description = f.Description,
        }), JsonOut);

    /// <summary>
    /// A document's fields as typed in the Document Designer, checked: each has a name of letters, digits or _ (once), a known type,
    /// a Choice field its choices, and a default that fits its type. Problems are added to <paramref name="errors"/>.
    /// </summary>
    public static List<WfeDataField> Clean(IEnumerable<WfeDataField?>? given, List<string> errors)
    {
        var list = new List<WfeDataField>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (f, i) in (given ?? []).Select((f, i) => (f, i + 1)))
        {
            var name = f?.Name?.Trim() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9_]{0,49}$"))
            {
                errors.Add($"Field {i}: give it a name of letters, digits or _ (starting with a letter).");
                continue;
            }

            if (!seen.Add(name))
            {
                errors.Add($"Field \"{name}\" is listed twice.");
                continue;
            }

            var type = Types.FirstOrDefault(t => string.Equals(t, f!.Type?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "String";
            var choices = (f!.Choices ?? []).Select(c => c?.Trim() ?? string.Empty).Where(c => c.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (type != "Choice")
                choices = [];
            var field = new WfeDataField(name, string.IsNullOrWhiteSpace(f.Label) ? null : f.Label.Trim(), type, f.Required,
                string.IsNullOrWhiteSpace(f.DefaultValue) ? null : f.DefaultValue.Trim(), choices,
                string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim());
            if (type == "Choice" && choices.Count == 0)
                errors.Add($"\"{field.Caption}\" is a Choice field: list its choices.");
            if (field.DefaultValue is not null)
            {
                var (value, error) = Normalize(field, field.DefaultValue);
                if (error is not null)
                    errors.Add($"Default of {error}");
                else
                    field = field with { DefaultValue = value };
            }

            list.Add(field);
        }

        return list;
    }

    /// <summary>
    /// The values to store when an instance starts: the given values with each declared field's default filled in, every declared
    /// value checked against its type and written in one form (1234.50, 2026-11-08, true), and every required field present.
    /// Values the workflow does not declare are kept as given.
    /// </summary>
    public static Dictionary<string, string?> ForStart(IReadOnlyList<WfeDataField> fields, IDictionary<string, string?>? given)
    {
        var values = new Dictionary<string, string?>(given ?? new Dictionary<string, string?>(), StringComparer.OrdinalIgnoreCase);
        foreach (var f in fields)
            if (f.DefaultValue is not null && (!values.TryGetValue(f.Name, out var v) || string.IsNullOrWhiteSpace(v)))
                values[f.Name] = f.DefaultValue;

        var errors = new Dictionary<string, string[]>();
        Check(fields, values, errors);
        foreach (var f in fields.Where(f => f.Required))
            if (!values.TryGetValue(f.Name, out var v) || string.IsNullOrWhiteSpace(v))
                errors[f.Name] = [$"The workflow needs \"{f.Caption}\" to start."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        return values;
    }

    /// <summary>The values set when a task completes: declared ones checked against their type and written in one form.</summary>
    public static Dictionary<string, string?> ForUpdate(IReadOnlyList<WfeDataField> fields, IDictionary<string, string?> given)
    {
        var values = new Dictionary<string, string?>(given, StringComparer.OrdinalIgnoreCase);
        var errors = new Dictionary<string, string[]>();
        Check(fields, values, errors);
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        return values;
    }

    /// <summary>The declared type of a value, for WFE_INSTANCE_VARIABLES.VARIABLE_TYPE; null when the workflow does not declare it.</summary>
    public static string? TypeOf(IReadOnlyList<WfeDataField> fields, string name) =>
        fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))?.Type;

    /// <summary>The instance's data as the task page shows it.</summary>
    public static WfeInstanceDataDto View(IReadOnlyList<WfeDataField> fields, IReadOnlyDictionary<string, string?> values)
    {
        var declared = fields.Select(f => new WfeDataValueDto(f.Name, f.Caption, f.Type, f.Required,
            values.TryGetValue(f.Name, out var v) ? v : null, f.Description)).ToList();
        var other = values.Where(kv => !fields.Any(f => string.Equals(f.Name, kv.Key, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new WfeDataValueDto(kv.Key, kv.Key, "String", false, kv.Value, null)).ToList();
        return new WfeInstanceDataDto(declared, other);
    }

    private static void Check(IReadOnlyList<WfeDataField> fields, Dictionary<string, string?> values, Dictionary<string, string[]> errors)
    {
        foreach (var f in fields)
        {
            var key = values.Keys.FirstOrDefault(k => string.Equals(k, f.Name, StringComparison.OrdinalIgnoreCase));
            if (key is null || string.IsNullOrWhiteSpace(values[key]))
                continue;
            var (value, error) = Normalize(f, values[key]!.Trim());
            if (error is not null)
                errors[f.Name] = [error];
            else
                values[key] = value;
        }
    }

    /// <summary>A value in its stored form, or the reason it does not fit the field's type.</summary>
    public static (string? Value, string? Error) Normalize(WfeDataField f, string raw)
    {
        var inv = CultureInfo.InvariantCulture;
        switch (f.Type)
        {
            case "Number":
                return long.TryParse(raw.Replace(",", string.Empty, StringComparison.Ordinal), NumberStyles.Integer, inv, out var n)
                    ? (n.ToString(inv), null) : (null, $"\"{f.Caption}\" must be a whole number, not \"{raw}\".");
            case "Decimal":
                return decimal.TryParse(raw.Replace(",", string.Empty, StringComparison.Ordinal), NumberStyles.Number, inv, out var d)
                    ? (d.ToString(inv), null) : (null, $"\"{f.Caption}\" must be an amount, not \"{raw}\".");
            case "Boolean":
                return raw.ToUpperInvariant() switch
                {
                    "TRUE" or "Y" or "YES" or "1" => ("true", null),
                    "FALSE" or "N" or "NO" or "0" => ("false", null),
                    _ => (null, $"\"{f.Caption}\" must be Yes or No, not \"{raw}\"."),
                };
            case "Date":
                return DateTime.TryParse(raw, inv, DateTimeStyles.None, out var dt)
                    ? (dt.ToString("yyyy-MM-dd", inv), null) : (null, $"\"{f.Caption}\" must be a date, not \"{raw}\".");
            case "Choice":
                var c = f.Choices.FirstOrDefault(x => string.Equals(x, raw, StringComparison.OrdinalIgnoreCase));
                return f.Choices.Count == 0 || c is not null
                    ? (c ?? raw, null) : (null, $"\"{f.Caption}\" must be one of {string.Join(", ", f.Choices)}, not \"{raw}\".");
            default:
                return (raw, null);
        }
    }
}
