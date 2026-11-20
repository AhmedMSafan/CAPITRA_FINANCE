using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_FX_REVALUATION_RUNS: copy of FINOVA GL_FX_REVALUATION_RUNS (APEX), all columns kept.
/// </summary>
public class GlFxRevaluationRun : AuditableEntity
{
    public long RevaluationRunId { get; set; }

    public long RevaluationRuleId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string RunCode { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, GENERATED, APPROVED, POSTED, REVERSED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    public DateOnly RevaluationDate { get; set; }

    public long RateTypeId { get; set; }

    public int TotalLineCount { get; set; }

    public decimal TotalGainAmount { get; set; }

    public decimal TotalLossAmount { get; set; }

    public long? JournalBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? ReversalJournalHeaderId { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? ReversedBy { get; set; }

    public DateTime? ReversedAt { get; set; }

    public string? Notes { get; set; }
}
