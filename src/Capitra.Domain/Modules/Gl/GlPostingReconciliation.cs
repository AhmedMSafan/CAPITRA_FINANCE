using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_RECONCILIATIONS: copy of FINOVA GL_POSTING_RECONCILIATIONS (APEX), all columns kept.
/// </summary>
public class GlPostingReconciliation : AuditableEntity
{
    public long PostingReconId { get; set; }

    public string ReconNo { get; set; } = string.Empty;

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public long? PostingRunId { get; set; }

    /// <summary>One of: POSTING_RUN, PERIOD, LEDGER, SOURCE, ACCOUNT.</summary>
    public string ReconScope { get; set; } = "POSTING_RUN";

    public decimal JournalTotalDr { get; set; }

    public decimal JournalTotalCr { get; set; }

    public decimal ImpactTotalDr { get; set; }

    public decimal ImpactTotalCr { get; set; }

    public decimal BalanceTotalDr { get; set; }

    public decimal BalanceTotalCr { get; set; }

    public decimal DifferenceDr { get; set; }

    public decimal DifferenceCr { get; set; }

    /// <summary>One of: DRAFT, MATCHED, DIFFERENCE, APPROVED, ERROR, CANCELLED.</summary>
    public string ReconStatus { get; set; } = "DRAFT";

    public string? ReconciledBy { get; set; }

    public DateTime? ReconciledAt { get; set; }
}
