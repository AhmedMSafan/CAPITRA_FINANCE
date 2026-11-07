using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_YEAR_END_CLOSE_RUNS: copy of FINOVA GL_YEAR_END_CLOSE_RUNS (APEX), all columns kept.
/// </summary>
public class GlYearEndCloseRun : AuditableEntity
{
    public long YearEndRunId { get; set; }

    public long CloseRunId { get; set; }

    public long LedgerId { get; set; }

    public long FiscalYearId { get; set; }

    public long? TargetFiscalYearId { get; set; }

    public long? RetainedEarningsAccountCombId { get; set; }

    public long? OpeningBalanceBatchId { get; set; }

    public long? ClosingJournalBatchId { get; set; }

    /// <summary>One of: DRAFT, GENERATED, APPROVED, POSTED, OPENING_GENERATED, LOCKED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    public decimal RevenueTotal { get; set; }

    public decimal ExpenseTotal { get; set; }

    public int NetProfitLoss { get; set; }

    public decimal BalanceSheetTotalDr { get; set; }

    public decimal BalanceSheetTotalCr { get; set; }

    public decimal OpeningBalanceTotalDr { get; set; }

    public decimal OpeningBalanceTotalCr { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
