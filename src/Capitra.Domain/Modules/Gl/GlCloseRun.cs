using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_RUNS: copy of FINOVA GL_CLOSE_RUNS (APEX), all columns kept.
/// </summary>
public class GlCloseRun : AuditableEntity
{
    public long CloseRunId { get; set; }

    public string CloseRunNo { get; set; } = string.Empty;

    public long? CloseProfileId { get; set; }

    /// <summary>One of: MONTH_END, YEAR_END, QUARTER_END, ADHOC.</summary>
    public string CloseType { get; set; } = string.Empty;

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public long? FiscalYearId { get; set; }

    public string? ClosePeriodName { get; set; }

    public int? CloseYearNumber { get; set; }

    /// <summary>One of: DRAFT, IN_PROGRESS, VALIDATION_FAILED, READY_FOR_APPROVAL, APPROVED, CLOSED, LOCKED, REOPENED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    /// <summary>One of: NORMAL, REOPEN, SIMULATION, CORRECTION.</summary>
    public string RunMode { get; set; } = "NORMAL";

    public string? StartedBy { get; set; }

    public DateTime? StartedAt { get; set; }

    public string? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public string? ReopenReason { get; set; }

    public string? CloseSummary { get; set; }

    public decimal? TotalTasks { get; set; }

    public int? CompletedTasks { get; set; }

    public int? BlockedTasks { get; set; }

    public int? WarningCount { get; set; }

    public int? ErrorCount { get; set; }

    public int? BlockerCount { get; set; }
}
