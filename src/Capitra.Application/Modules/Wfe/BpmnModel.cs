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
    string? FormCode = null)
{
    public bool IsHumanTask =>
        Type is "userTask" or "manualTask" || string.Equals(StepType, "HumanTask", StringComparison.OrdinalIgnoreCase);
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
    public static BpmnElement? Next(string xml, string elementId, string? outcome = null)
    {
        var doc = Load(xml);
        var flows = doc.Descendants(Bpmn + "sequenceFlow").Where(x => (string?)x.Attribute("sourceRef") == elementId).ToList();
        if (flows.Count == 0)
            return null;

        var selected = flows.Count == 1 || string.IsNullOrWhiteSpace(outcome)
            ? flows[0]
            : flows.FirstOrDefault(x => Matches(x, outcome)) ?? flows[0];

        var target = (string?)selected.Attribute("targetRef") ?? throw new InvalidOperationException("Sequence flow has no targetRef.");
        return Element(xml, target);
    }

    private static bool Matches(XElement flow, string outcome) =>
        string.Equals((string?)flow.Attribute("name"), outcome, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Attr(flow, "outcome"), outcome, StringComparison.OrdinalIgnoreCase);

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
        Attr(e, "formCode"));

    private static string? Attr(XElement e, string name) => (string?)e.Attribute(Finova + name) ?? (string?)e.Attribute(name);
}
