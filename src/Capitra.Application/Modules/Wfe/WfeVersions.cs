using System.Globalization;
using System.Xml.Linq;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

public sealed record WfeVersionXmlDto(string BpmnXml);

public sealed record WfeVersionDto(long DefinitionVersionId, int VersionNo, DateTime ReleasedAt, string ReleasedBy, int Running, int Total, bool IsCurrent);

/// <summary>
/// Keeps a version of a workflow each time a released workflow's steps or routing change (like K2: each deploy is a new
/// version). New instances start on the latest version; running ones stay on the version they started with.
/// </summary>
public static class WfeVersions
{
    private static readonly XNamespace Bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";

    /// <summary>
    /// The latest version of a released definition, made first when there is none or when the definition's steps or routing
    /// differ from it. A change of layout only (shapes moved, the canvas position) updates the latest version in place.
    /// </summary>
    public static async Task<WfeDefinitionVersion> CurrentAsync(ICapitraDbContext db, WfeWorkflowDefinition definition, string user, DateTime now,
        CancellationToken ct)
    {
        var latest = definition.WorkflowDefinitionId == 0 ? null
            : await db.Set<WfeDefinitionVersion>().Where(x => x.WorkflowDefinitionId == definition.WorkflowDefinitionId)
                .OrderByDescending(x => x.VersionNo).FirstOrDefaultAsync(ct);
        if (latest is not null && SameProcess(latest.BpmnXml, definition.BpmnXml))
        {
            if (latest.BpmnXml != definition.BpmnXml)
                latest.BpmnXml = definition.BpmnXml;
            definition.VersionNo = latest.VersionNo.ToString(CultureInfo.InvariantCulture);
            return latest;
        }

        // the first version keeps the number the workflow already showed
        var no = latest?.VersionNo + 1
            ?? (int.TryParse(definition.VersionNo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n > 0 ? n : 1);
        var version = new WfeDefinitionVersion { Definition = definition, VersionNo = no, BpmnXml = definition.BpmnXml, ReleasedAt = now, ReleasedBy = user };
        db.Add(version);
        definition.VersionNo = no.ToString(CultureInfo.InvariantCulture);
        return version;
    }

    /// <summary>The BPMN an instance runs on: its version's, or the definition's for instances started before versions were kept.</summary>
    public static string XmlOf(WfeWorkflowInstance instance) =>
        instance.Version?.BpmnXml ?? instance.Definition?.BpmnXml ?? throw new InvalidOperationException("The workflow instance has no BPMN.");

    /// <summary>The BPMN the instance runs on, or null when there is no such instance.</summary>
    public static Task<string?> XmlOfInstanceAsync(ICapitraDbContext db, long workflowInstanceId, CancellationToken ct) =>
        (from i in db.Set<WfeWorkflowInstance>().AsNoTracking()
         join d in db.Set<WfeWorkflowDefinition>() on i.WorkflowDefinitionId equals d.WorkflowDefinitionId
         join v in db.Set<WfeDefinitionVersion>() on i.DefinitionVersionId equals v.DefinitionVersionId into vs
         from v in vs.DefaultIfEmpty()
         where i.WorkflowInstanceId == workflowInstanceId
         select v != null ? v.BpmnXml : d.BpmnXml).FirstOrDefaultAsync(ct);

    public static async Task<IReadOnlyList<WfeVersionDto>> ListAsync(ICapitraDbContext db, long workflowDefinitionId, CancellationToken ct)
    {
        string[] closed = ["COMPLETED", "REJECTED", "CANCELLED"];
        var versions = await db.Set<WfeDefinitionVersion>().AsNoTracking().Where(x => x.WorkflowDefinitionId == workflowDefinitionId)
            .OrderByDescending(x => x.VersionNo).ToListAsync(ct);
        var counts = await db.Set<WfeWorkflowInstance>().AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == workflowDefinitionId && x.DefinitionVersionId != null)
            .GroupBy(x => x.DefinitionVersionId)
            .Select(g => new { Id = g.Key!.Value, Total = g.Count(), Running = g.Count(x => !closed.Contains(x.InstanceStatus.ToUpper())) })
            .ToDictionaryAsync(x => x.Id, ct);
        return versions.Select((x, i) => new WfeVersionDto(x.DefinitionVersionId, x.VersionNo, x.ReleasedAt, x.ReleasedBy,
            counts.TryGetValue(x.DefinitionVersionId, out var c) ? c.Running : 0, c?.Total ?? 0, i == 0)).ToList();
    }

    /// <summary>True when two diagrams have the same steps, settings and flows (the layout and canvas position are left out).</summary>
    public static bool SameProcess(string a, string b) => string.Equals(ProcessOf(a), ProcessOf(b), StringComparison.Ordinal);

    private static string ProcessOf(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return string.Join("\n", doc.Descendants(Bpmn + "process").Select(x => x.ToString(SaveOptions.DisableFormatting)));
        }
        catch (System.Xml.XmlException)
        {
            return xml;
        }
    }
}
