using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_OPENING_BALANCE_RUNS: copy of FINOVA GL_OPENING_BALANCE_RUNS (APEX), all columns kept.
/// </summary>
public class GlOpeningBalanceRun : AuditableEntity
{
    public long OpeningBalanceRunId { get; set; }

    public long YearEndRunId { get; set; }

    public long LedgerId { get; set; }

    public long TargetFiscalYearId { get; set; }

    public long? TargetLedgerPeriodId { get; set; }

    public long? JournalBatchId { get; set; }

    /// <summary>One of: DRAFT, GENERATED, APPROVED, POSTED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    public decimal TotalDr { get; set; }

    public decimal TotalCr { get; set; }

    public int GeneratedLineCount { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
