using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_SNAPSHOTS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_SNAPSHOT (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioSnapshot : AuditableEntity
{
    public long SnapshotId { get; set; }

    public DateOnly AsOfDate { get; set; }

    public long PortfolioId { get; set; }

    public decimal OutstandingValue { get; set; }

    public decimal PurchaseValue { get; set; }

    public decimal? ChequeCoverage { get; set; }

    public decimal? DocumentCompletionPct { get; set; }

    public int OpenExceptionCount { get; set; }

    public string? CurrentStatus { get; set; }
}
