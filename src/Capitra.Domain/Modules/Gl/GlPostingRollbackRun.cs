using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_ROLLBACK_RUNS: copy of FINOVA GL_POSTING_ROLLBACK_RUNS (APEX), all columns kept.
/// </summary>
public class GlPostingRollbackRun : AuditableEntity
{
    public long RollbackRunId { get; set; }

    public string RollbackRunNo { get; set; } = string.Empty;

    public long PostingRunId { get; set; }

    public string RollbackReasonCode { get; set; } = string.Empty;

    public string RollbackReason { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, APPROVED, IN_PROGRESS, COMPLETED, ERROR, CANCELLED.</summary>
    public string RollbackStatus { get; set; } = "DRAFT";

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
