using System.Xml.Linq;

namespace Capitra.Application.Modules.Wfe;

/// <summary>One BPMN element with the step settings the designer stores as finova:* attributes.</summary>
public sealed record BpmnElement(
    string Id,
    string Type,
    string? Name,
    string? StepCode,
    string? StepType,
    string? AssignmentType,
    string? Assignee,
    string? Route,
    string? RouteParameters,
    string? Actions,
    string? RuleCode = null,
    string? FormCode = null,
    BpmnSla? Sla = null)
{
    public bool IsHumanTask =>
        Type is "userTask" or "manualTask" || string.Equals(StepType, "HumanTask", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// A step's time limit (SLA), as the designer stores it: finova:slaDuration and finova:slaUnit give the due time; the assignee is
/// reminded finova:slaRemindHours before it; finova:slaEscalateHours after it the task is escalated (finova:slaEscalation REASSIGN
/// moves it to finova:slaEscalateTo, a User or Role per finova:slaEscalateType; NOTIFY only tells them).
/// </summary>
public sealed record BpmnSla(
    decimal Duration, string Unit, decimal? RemindHours, decimal? EscalateHours, string Escalation, string? EscalateType, string? EscalateTo)
{
    public const string Hours = "HOURS";
    public const string Days = "DAYS";
    public const string WorkingDays = "WORKING_DAYS";
    public const string None = "NONE";
    public const string Reassign = "REASSIGN";
    public const string Notify = "NOTIFY";

    public bool Escalates => Escalation is Reassign or Notify && !string.IsNullOrWhiteSpace(EscalateTo);
}

/// <summary>
/// The workflow's type and document. System-built workflows are started by the system (factoring, portfolios); user-defined ones
/// by a user from Start a request, on the one document linked to the whole workflow. Type is null on workflows drawn before types.
/// </summary>
public sealed record BpmnProcessInfo(string? Type, string? DocumentCode)
{
    public const string System = "SYSTEM";
    public const string User = "USER";

    public bool IsUserDefined => string.Equals(Type, User, StringComparison.OrdinalIgnoreCase);

    public bool IsSystemBuilt => string.Equals(Type, System, StringComparison.OrdinalIgnoreCase);
}

/// <summary>A path (sequence flow) between two elements, with its action name and its conditions on data fields.</summary>
public sealed record BpmnFlow(
    string Id, string? Name, string? Outcome, string SourceId, string SourceName, string TargetId, string TargetName, string? RawConditions, IReadOnlyList<BpmnRule> Rules);

/// <summary>Reads a BPMN 2.0 diagram: its start event and the element a sequence flow leads to.</summary>
public static class BpmnModel
{
    private static readonly XNamespace Bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
    private static readonly XNamespace Finova = "http://finova/workflow";

    /// <summary>The process's type and document (finova:workflowType, finova:documentCode); empty when the XML cannot be read.</summary>
    public static BpmnProcessInfo Process(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return new BpmnProcessInfo(null, null);
        try
        {
            var p = XDocument.Parse(xml).Descendants(Bpmn + "process").FirstOrDefault();
            if (p is null)
                return new BpmnProcessInfo(null, null);
            var type = Attr(p, "workflowType")?.Trim().ToUpperInvariant();
            var doc = Attr(p, "documentCode")?.Trim().ToUpperInvariant();
            return new BpmnProcessInfo(
                type is BpmnProcessInfo.System or BpmnProcessInfo.User ? type : null, string.IsNullOrEmpty(doc) ? null : doc);
        }
        catch (System.Xml.XmlException)
        {
            return new BpmnProcessInfo(null, null);
        }
    }

    public static BpmnElement StartEvent(string xml)
    {
        var start = Load(xml).Descendants(Bpmn + "startEvent").FirstOrDefault()
            ?? throw new InvalidOperationException("The BPMN definition has no start event.");
        return Map(start);
    }

    public static BpmnElement Element(string xml, string elementId)
    {
        var element = Load(xml).Descendants().FirstOrDefault(x => (string?)x.Attribute("id") == elementId)
            ?? throw new InvalidOperationException($"BPMN element '{elementId}' was not found.");
        return Map(element);
    }

    /// <summary>Every element of the diagram that carries an id (events, tasks, gateways, flows).</summary>
    public static IReadOnlyList<BpmnElement> Elements(string xml) =>
        Load(xml).Descendants().Where(x => x.Attribute("id") is not null && x.Name.Namespace == Bpmn).Select(Map).ToList();

    /// <summary>The first element with this step code, or null.</summary>
    public static BpmnElement? StepByCode(string xml, string stepCode) =>
        Load(xml).Descendants().Select(Map).FirstOrDefault(x => string.Equals(x.StepCode, stepCode, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The element after <paramref name="elementId"/>, or null at the end. With several outgoing flows, the flow whose
    /// name (or finova:outcome) matches the outcome is taken, so APPROVE and REJECT route without extra settings.
    /// </summary>
    public static BpmnElement? Next(string xml, string elementId, string? outcome = null) =>
        Next(xml, elementId, outcome, null).Element;

    /// <summary>
    /// The element after <paramref name="elementId"/> and the path taken. The paths that answer the outcome (by name or
    /// finova:outcome) are tried first, or all paths when none does. Among them, the first path whose conditions on data fields
    /// all hold for <paramref name="values"/> wins; otherwise the first path without conditions. Diagrams without conditions
    /// route exactly as before.
    /// </summary>
    public static (BpmnElement? Element, BpmnFlow? Flow) Next(
        string xml, string elementId, string? outcome, IReadOnlyDictionary<string, string?>? values)
    {
        var flows = Flows(Load(xml)).Where(x => x.SourceId == elementId).ToList();
        if (flows.Count == 0)
            return (null, null);

        var answering = string.IsNullOrWhiteSpace(outcome) ? [] : flows.Where(x => string.Equals(x.Name, outcome, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Outcome, outcome, StringComparison.OrdinalIgnoreCase)).ToList();
        var candidates = answering.Count > 0 ? answering : flows;
        var data = values ?? new Dictionary<string, string?>();
        var selected = candidates.FirstOrDefault(x => x.Rules.Count > 0 && WfeRules.Holds(x.Rules, data))
            ?? candidates.FirstOrDefault(x => x.Rules.Count == 0)
            ?? throw new Capitra.Application.Common.Exceptions.BusinessRuleException(
                $"No path out of '{flows[0].SourceName}' fits this request's data: "
                + string.Join("; ", candidates.Select(x => $"to {x.TargetName} needs {string.Join(" and ", x.Rules.Select(r => r.Text))}"))
                + ". Add a path without conditions in the designer.");

        return (Element(xml, selected.TargetId), selected);
    }

    /// <summary>Every path of the diagram, with its conditions.</summary>
    public static IReadOnlyList<BpmnFlow> Flows(string xml) => Flows(Load(xml));

    private static List<BpmnFlow> Flows(XDocument doc)
    {
        var names = doc.Descendants().Where(x => x.Attribute("id") is not null)
            .GroupBy(x => (string)x.Attribute("id")!)
            .ToDictionary(g => g.Key, g => (string?)g.First().Attribute("name") is { Length: > 0 } n ? n : g.Key);
        return doc.Descendants(Bpmn + "sequenceFlow").Select(f =>
        {
            var source = (string?)f.Attribute("sourceRef") ?? string.Empty;
            var target = (string?)f.Attribute("targetRef") ?? throw new InvalidOperationException("Sequence flow has no targetRef.");
            var raw = Attr(f, "conditions");
            var outcome = Attr(f, "outcome") is { Length: > 0 } o ? o.Trim() : ((string?)f.Attribute("name"))?.Trim();
            return new BpmnFlow(
                (string?)f.Attribute("id") ?? string.Empty, ((string?)f.Attribute("name"))?.Trim(), string.IsNullOrEmpty(outcome) ? null : outcome,
                source, names.GetValueOrDefault(source, source), target, names.GetValueOrDefault(target, target), raw, WfeRules.Parse(raw));
        }).ToList();
    }

    private static XDocument Load(string xml) =>
        string.IsNullOrWhiteSpace(xml) ? throw new InvalidOperationException("BPMN XML is empty.") : XDocument.Parse(xml);

    private static BpmnElement Map(XElement e) => new(
        (string?)e.Attribute("id") ?? string.Empty,
        e.Name.LocalName,
        (string?)e.Attribute("name"),
        Attr(e, "stepCode"),
        Attr(e, "stepType"),
        Attr(e, "assignmentType"),
        Attr(e, "assignee"),
        Attr(e, "route"),
        Attr(e, "routeParameters"),
        Attr(e, "actions"),
        Attr(e, "ruleCode"),
        Attr(e, "formCode"),
        Sla(e));

    /// <summary>The step's time limit, or null when it has none (or the duration is not a positive number).</summary>
    private static BpmnSla? Sla(XElement e)
    {
        if (!decimal.TryParse(Attr(e, "slaDuration"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var duration)
            || duration <= 0)
            return null;
        var unit = Attr(e, "slaUnit")?.Trim().ToUpperInvariant();
        var escalation = Attr(e, "slaEscalation")?.Trim().ToUpperInvariant();
        return new BpmnSla(
            duration,
            unit is BpmnSla.Days or BpmnSla.WorkingDays ? unit : BpmnSla.Hours,
            Hours(Attr(e, "slaRemindHours")),
            Hours(Attr(e, "slaEscalateHours")),
            escalation is BpmnSla.Reassign or BpmnSla.Notify ? escalation : BpmnSla.None,
            string.Equals(Attr(e, "slaEscalateType"), "User", StringComparison.OrdinalIgnoreCase) ? "User" : "Role",
            string.IsNullOrWhiteSpace(Attr(e, "slaEscalateTo")) ? null : Attr(e, "slaEscalateTo")!.Trim());
    }

    private static decimal? Hours(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var h) && h >= 0 ? h : null;

    /// <summary>The raw SLA attributes of every human step, for checking a design before release: (step name, attribute, value).</summary>
    public static IEnumerable<(string Step, string Name, string Value)> SlaAttributes(string xml)
    {
        string[] names = ["slaDuration", "slaUnit", "slaRemindHours", "slaEscalateHours", "slaEscalation", "slaEscalateType", "slaEscalateTo"];
        foreach (var e in Load(xml).Descendants().Where(x => x.Name.Namespace == Bpmn))
            foreach (var n in names)
                if (Attr(e, n) is { } v && v.Trim().Length > 0)
                    yield return ((string?)e.Attribute("name") ?? (string?)e.Attribute("id") ?? "?", n, v.Trim());
    }

    private static string? Attr(XElement e, string name) => (string?)e.Attribute(Finova + name) ?? (string?)e.Attribute(name);
}
