using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_STEP_INSTANCES: each BPMN element an instance passed through.</summary>
public class WfeStepInstance : AuditableEntity
{
    public long StepInstanceId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public string ElementId { get; set; } = string.Empty;

    public string? StepCode { get; set; }

    public string? StepName { get; set; }

    public string ElementType { get; set; } = string.Empty;

    /// <summary>One of: ENTERED, WAITING, COMPLETED.</summary>
    public string StepStatus { get; set; } = "ENTERED";

    public DateTime EnteredAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? CompletedBy { get; set; }

    public string? Outcome { get; set; }

    public WfeWorkflowInstance? Instance { get; set; }

    /// <summary>The step's tasks: one, or one per approver on a parallel approval.</summary>
    public ICollection<WfeTask> Tasks { get; set; } = [];
}
