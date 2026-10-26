using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_CLOSE_CHECKLISTS: copy of FINOVA GL_PERIOD_CLOSE_CHECKLIST (APEX), all columns kept.
/// </summary>
public class GlPeriodCloseChecklist : AuditableEntity
{
    public long CloseChecklistId { get; set; }

    public long LedgerPeriodId { get; set; }

    public long CloseTaskId { get; set; }

    /// <summary>One of: NOT_STARTED, IN_PROGRESS, COMPLETED, WAIVED, BLOCKED, FAILED.</summary>
    public string TaskStatus { get; set; } = "NOT_STARTED";

    public bool IsRequired { get; set; } = true;

    public string? ResponsibleRoleCode { get; set; }

    public string? AssignedToUser { get; set; }

    public DateTime? DueAt { get; set; }

    public string? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? WaivedBy { get; set; }

    public DateTime? WaivedAt { get; set; }

    public string? WaiverReason { get; set; }

    public string? ValidationMessage { get; set; }

    public string? Notes { get; set; }
}
