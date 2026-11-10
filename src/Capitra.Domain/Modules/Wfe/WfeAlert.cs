using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_ALERTS: an in-app message to one user about a task's time limit (SLA): due soon, overdue, or escalated to them.
/// Shown on the bell in the page header until the user opens or dismisses it.
/// </summary>
public class WfeAlert : AuditableEntity
{
    public long AlertId { get; set; }

    /// <summary>The user the alert is for (user name).</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>One of: REMINDER, OVERDUE, ESCALATED.</summary>
    public string AlertType { get; set; } = string.Empty;

    public long? InboxItemId { get; set; }

    public long? WorkflowInstanceId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime AlertAt { get; set; }

    public DateTime? ReadAt { get; set; }
}
