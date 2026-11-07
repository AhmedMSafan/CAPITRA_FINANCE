using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_TASKS: the human task of a user-task step.</summary>
public class WfeTask : AuditableEntity
{
    public long TaskId { get; set; }

    public long StepInstanceId { get; set; }

    /// <summary>One of: OPEN, CLAIMED, IN_PROGRESS, COMPLETED.</summary>
    public string TaskStatus { get; set; } = "OPEN";

    /// <summary>One of: User, Role.</summary>
    public string? AssignmentType { get; set; }

    public string? Assignee { get; set; }

    public string? ClaimedBy { get; set; }

    public DateTime? ClaimedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>The page that does the work, e.g. /mortgage/aml-review.</summary>
    public string? Route { get; set; }

    /// <summary>JSON list of {name, value} route parameters, resolved from the instance variables.</summary>
    public string? RouteParameters { get; set; }

    /// <summary>Comma-separated actions the task may be completed with, e.g. APPROVE,REJECT.</summary>
    public string? AllowedActions { get; set; }

    public WfeStepInstance? StepInstance { get; set; }

    public WfeInboxItem? InboxItem { get; set; }
}
