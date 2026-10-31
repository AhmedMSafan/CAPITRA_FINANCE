using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_RUN_TASKS: copy of FINOVA GL_CLOSE_RUN_TASKS (APEX), all columns kept.
/// </summary>
public class GlCloseRunTask : AuditableEntity
{
    public long CloseRunTaskId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseStepId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string TaskNameEn { get; set; } = string.Empty;

    public string? TaskNameAr { get; set; }

    public string? TaskGroupCode { get; set; }

    /// <summary>One of: VALIDATION, TASK, GENERATION, APPROVAL, POSTING, RECONCILIATION, REPORT, LOCK, YEAR_END_TRANSFER, OPENING_BALANCE.</summary>
    public string TaskType { get; set; } = string.Empty;

    public string? SourceModule { get; set; }

    public long? DependencyTaskId { get; set; }

    public string? OwnerRoleCode { get; set; }

    public string? OwnerUserName { get; set; }

    /// <summary>One of: NOT_STARTED, IN_PROGRESS, COMPLETED, APPROVED, FAILED, BLOCKED, WAIVED, CANCELLED.</summary>
    public string TaskStatus { get; set; } = "NOT_STARTED";

    public bool IsMandatory { get; set; } = true;

    public bool IsBlocking { get; set; } = true;

    public DateTime? DueAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public string? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsWaiver { get; set; }

    public string? WaiverReason { get; set; }

    public string? CompletionNotes { get; set; }

    public int DisplaySeq { get; set; } = 10;
}
