using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_BALANCE_IMPACTS: copy of FINOVA GL_POSTING_BALANCE_IMPACTS (APEX), all columns kept.
/// </summary>
public class GlPostingBalanceImpact : AuditableEntity
{
    public long BalanceImpactId { get; set; }

    public long PostingRunId { get; set; }

    public long? PostingRunLineId { get; set; }

    public long? QueueId { get; set; }

    public long? JournalBatchId { get; set; }

    public long JournalHeaderId { get; set; }

    public long JournalLineId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public DateOnly AccountingDate { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    public decimal AccountedDr { get; set; }

    public decimal AccountedCr { get; set; }

    public decimal? BalanceBeforeDr { get; set; }

    public decimal? BalanceBeforeCr { get; set; }

    public decimal? BalanceAfterDr { get; set; }

    public decimal? BalanceAfterCr { get; set; }

    /// <summary>One of: APPLIED, REVERSED, ROLLBACK_APPLIED, REBUILD_APPLIED.</summary>
    public string ImpactStatus { get; set; } = "APPLIED";

    public long? ReversalImpactId { get; set; }
}
