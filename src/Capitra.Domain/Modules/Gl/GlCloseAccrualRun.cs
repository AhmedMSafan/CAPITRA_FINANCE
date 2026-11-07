using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCRUAL_RUNS: copy of FINOVA GL_CLOSE_ACCRUAL_RUNS (APEX), all columns kept.
/// </summary>
public class GlCloseAccrualRun : AuditableEntity
{
    public long AccrualRunId { get; set; }

    public long CloseRunId { get; set; }

    public long? AccrualRuleId { get; set; }

    public long? JournalBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? ReversalJournalId { get; set; }

    /// <summary>One of: DRAFT, GENERATED, APPROVED, POSTED, REVERSED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public int GeneratedLineCount { get; set; }

    public DateOnly? ReversalDate { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
