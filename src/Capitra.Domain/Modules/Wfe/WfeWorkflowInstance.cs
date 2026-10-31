using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_WORKFLOW_INSTANCES: one run of a workflow definition for one business record.</summary>
public class WfeWorkflowInstance : AuditableEntity
{
    public long WorkflowInstanceId { get; set; }

    public long WorkflowDefinitionId { get; set; }

    /// <summary>One of: RUNNING, WAITING, COMPLETED, FAULTED.</summary>
    public string InstanceStatus { get; set; } = "RUNNING";

    public string? BusinessReference { get; set; }

    public string? CurrentElementId { get; set; }

    public DateTime StartedAt { get; set; }

    public string StartedBy { get; set; } = "SYSTEM";

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public WfeWorkflowDefinition? Definition { get; set; }

    public ICollection<WfeStepInstance> Steps { get; set; } = [];

    public ICollection<WfeInstanceVariable> Variables { get; set; } = [];
}
