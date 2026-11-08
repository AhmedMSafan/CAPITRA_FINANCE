using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_DEFINITION_VERSIONS: the BPMN of a workflow as it was released. A new version is kept each time the steps or routing
/// of a released workflow change; an instance runs on the version it started with, so changing a workflow never moves the
/// instances already running.
/// </summary>
public class WfeDefinitionVersion : AuditableEntity
{
    public long DefinitionVersionId { get; set; }

    public long WorkflowDefinitionId { get; set; }

    /// <summary>1, 2, 3 ... per workflow.</summary>
    public int VersionNo { get; set; }

    public string BpmnXml { get; set; } = string.Empty;

    public DateTime ReleasedAt { get; set; }

    public string ReleasedBy { get; set; } = "SYSTEM";

    public WfeWorkflowDefinition? Definition { get; set; }
}
