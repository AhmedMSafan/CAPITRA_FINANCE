using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_WORKFLOW_DEFINITIONS: a workflow drawn in the BPMN designer. Only a released, active definition can be started.</summary>
public class WfeWorkflowDefinition : AuditableEntity
{
    public long WorkflowDefinitionId { get; set; }

    public string WorkflowCode { get; set; } = string.Empty;

    public string WorkflowName { get; set; } = string.Empty;

    public string VersionNo { get; set; } = "1";

    /// <summary>One of: draft, released.</summary>
    public string DefinitionStatus { get; set; } = "draft";

    /// <summary>The BPMN 2.0 XML, with the step settings as finova:* attributes.</summary>
    public string BpmnXml { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
