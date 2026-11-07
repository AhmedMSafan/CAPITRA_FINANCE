using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_INBOX (agreed exception to plural table names): the user-facing work item of a task, shown in My Inbox.</summary>
public class WfeInboxItem : AuditableEntity
{
    public long InboxItemId { get; set; }

    public long TaskId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public long StepInstanceId { get; set; }

    /// <summary>One of: OPEN, CLAIMED, IN_PROGRESS, COMPLETED.</summary>
    public string InboxStatus { get; set; } = "OPEN";

    public string? AssignmentType { get; set; }

    public string? AssignedTo { get; set; }

    public string? AssignedRole { get; set; }

    public string? ClaimedBy { get; set; }

    public string? Subject { get; set; }

    public string? Route { get; set; }

    public string? RouteParameters { get; set; }

    public string? AllowedActions { get; set; }

    public string? Outcome { get; set; }

    public DateTime? ClaimedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public WfeTask? Task { get; set; }

    public WfeWorkflowInstance? Instance { get; set; }

    public WfeStepInstance? StepInstance { get; set; }
}
