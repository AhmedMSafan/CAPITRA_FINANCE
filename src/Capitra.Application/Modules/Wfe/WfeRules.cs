using System.Globalization;
using System.Text.Json;

namespace Capitra.Application.Modules.Wfe;

/// <summary>
/// One condition on a path (sequence flow), as the designer stores it in finova:conditions: a data field, an operator and a value,
/// such as AMOUNT &gt; 1000000. A path with conditions is taken only when all of them hold; a path without conditions is the
/// "otherwise" path.
/// </summary>
public sealed record BpmnRule(string Field, string Op, string? Value)
{
    public string Text => Op is WfeRules.Empty or WfeRules.NotEmpty ? $"{Field} {Op}" : $"{Field} {Op} {Value}";
}

/// <summary>Rules on data fields (K2's line rules): the next step is chosen by the values of the instance's data fields.</summary>
public static class WfeRules
{
    public const string Equal = "=";
    public const string NotEqual = "!=";
    public const string Greater = ">";
    public const string GreaterOrEqual = ">=";
    public const string Less = "<";
    public const string LessOrEqual = "<=";
    public const string Contains = "contains";
    public const string Empty = "empty";
    public const string NotEmpty = "notEmpty";

    public static readonly string[] Operators = [Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual, Contains, Empty, NotEmpty];

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private sealed class Raw
    {
        public string? Field { get; set; }
        public string? Op { get; set; }
        public JsonElement Value { get; set; }
    }

    /// <summary>The conditions in a finova:conditions value; empty when there are none or the JSON cannot be read.</summary>
    public static IReadOnlyList<BpmnRule> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        try
        {
            return (JsonSerializer.Deserialize<List<Raw>>(json, Json) ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.Field))
                .Select(x => new BpmnRule(
                    x.Field!.Trim(),
                    Normalize(x.Op),
                    x.Value.ValueKind switch
                    {
                        JsonValueKind.String => x.Value.GetString(),
                        JsonValueKind.Undefined or JsonValueKind.Null => null,
                        _ => x.Value.GetRawText(),
                    }))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string Normalize(string? op)
    {
        var o = (op ?? Equal).Trim();
        return o switch
        {
            "==" => Equal,
            "<>" => NotEqual,
            _ => Operators.FirstOrDefault(x => string.Equals(x, o, StringComparison.OrdinalIgnoreCase)) ?? o,
        };
    }

    /// <summary>True when every condition holds for the instance's values (field names are not case-sensitive).</summary>
    public static bool Holds(IReadOnlyList<BpmnRule> rules, IReadOnlyDictionary<string, string?> values) =>
        rules.All(r => Holds(r, values.TryGetValue(r.Field, out var v) ? v : null));

    public static bool Holds(BpmnRule rule, string? actual)
    {
        var a = actual?.Trim() ?? string.Empty;
        var b = rule.Value?.Trim() ?? string.Empty;
        return rule.Op switch
        {
            Empty => a.Length == 0,
            NotEmpty => a.Length > 0,
            Contains => a.Contains(b, StringComparison.OrdinalIgnoreCase),
            Equal => Compare(a, b) == 0,
            NotEqual => Compare(a, b) != 0,
            // an empty value never passes a greater or less test
            Greater => a.Length > 0 && Compare(a, b) > 0,
            GreaterOrEqual => a.Length > 0 && Compare(a, b) >= 0,
            Less => a.Length > 0 && Compare(a, b) < 0,
            LessOrEqual => a.Length > 0 && Compare(a, b) <= 0,
            _ => false,
        };
    }

    /// <summary>Numbers compare as numbers (1,000,000 or 1000000), dates as dates, yes/no as yes/no, the rest as text.</summary>
    private static int Compare(string a, string b)
    {
        if (Number(a) is { } x && Number(b) is { } y)
            return x.CompareTo(y);
        if (Date(a) is { } d1 && Date(b) is { } d2)
            return d1.CompareTo(d2);
        if (Bool(a) is { } p && Bool(b) is { } q)
            return p.CompareTo(q);
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static decimal? Number(string s) =>
        decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static DateTime? Date(string s) =>
        DateTime.TryParseExact(s, ["yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ", "o", "dd-MMM-yyyy"],
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d) ? d.Date : null;

    private static bool? Bool(string s) => s.ToLowerInvariant() switch
    {
        "true" or "yes" => true,
        "false" or "no" => false,
        _ => null,
    };

    /// <summary>
    /// The problems with the diagram's path conditions, for checking a design before release: unknown fields, unknown operators,
    /// a missing value, a greater/less test on a field that is not a number or date, and a decision with no path for "otherwise".
    /// </summary>
    public static List<string> Check(string xml, IReadOnlyList<WfeDataField> fields)
    {
        var errors = new List<string>();
        // paths are grouped by where they start and the action they answer (APPROVE, REJECT, or none)
        var groups = new Dictionary<(string, string), (int Conditional, int Plain, string Name)>();
        foreach (var flow in BpmnModel.Flows(xml))
        {
            var name = flow.SourceName;
            var key = (flow.SourceId, (flow.Outcome ?? string.Empty).ToUpperInvariant());
            var label = flow.Outcome is { Length: > 0 } o ? $"{name} ({o})" : name;
            var counts = groups.TryGetValue(key, out var found) ? found : (Conditional: 0, Plain: 0, Name: label);
            groups[key] = flow.Rules.Count > 0 ? (counts.Conditional + 1, counts.Plain, label) : (counts.Conditional, counts.Plain + 1, label);
            if (flow.RawConditions is not null && flow.Rules.Count == 0 && flow.RawConditions.Trim() is not ("" or "[]"))
                errors.Add($"Path from {name} to {flow.TargetName}: its conditions cannot be read.");
            foreach (var rule in flow.Rules)
            {
                var where = $"Path from {name} to {flow.TargetName}";
                var field = fields.FirstOrDefault(f => string.Equals(f.Name, rule.Field, StringComparison.OrdinalIgnoreCase));
                if (field is null)
                    errors.Add($"{where}: there is no data field '{rule.Field}'.");
                if (!Operators.Contains(rule.Op))
                    errors.Add($"{where}: '{rule.Op}' is not a known test.");
                else if (rule.Op is not (Empty or NotEmpty) && string.IsNullOrWhiteSpace(rule.Value))
                    errors.Add($"{where}: give a value to compare {rule.Field} with.");
                else if (rule.Op is Greater or GreaterOrEqual or Less or LessOrEqual)
                {
                    if (field is not null && field.Type is not ("Number" or "Decimal" or "Date"))
                        errors.Add($"{where}: {rule.Field} is not a number or date, so it cannot be compared with {rule.Op}.");
                    else if (field?.Type is "Number" or "Decimal" && Number(rule.Value!.Trim()) is null)
                        errors.Add($"{where}: '{rule.Value}' is not a number.");
                    else if (field?.Type == "Date" && Date(rule.Value!.Trim()) is null)
                        errors.Add($"{where}: '{rule.Value}' is not a date (use yyyy-mm-dd).");
                }
            }
        }

        foreach (var (_, c) in groups)
            if (c.Conditional > 0 && c.Plain == 0)
                errors.Add($"{c.Name}: every path out of it has conditions, so add one path without conditions for all other cases.");
        return errors;
    }
}

/// <summary>Release checks for parallel approvals.</summary>
public static class WfeParallel
{
    public static List<string> Check(string xml)
    {
        var errors = new List<string>();
        foreach (var e in BpmnModel.Elements(xml).Where(x => x.IsHumanTask && x.Parallel is not null))
        {
            var name = e.Name ?? e.Id;
            var p = e.Parallel!;
            if (p.Mode == BpmnParallel.Number && p.Count < 1)
                errors.Add($"{name}: give how many approvals are needed (1 or more).");
            if (string.Equals(e.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(e.Assignee))
                    errors.Add($"{name}: choose the role whose members approve.");
                continue;
            }

            var users = (e.Assignee ?? string.Empty).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count();
            if (users < 2)
                errors.Add($"{name}: a parallel approval needs at least two users, or a role.");
            else if (p.Mode == BpmnParallel.Number && p.Count > users)
                errors.Add($"{name}: it needs {p.Count} approvals but has only {users} approvers.");
        }

        return errors;
    }
}
