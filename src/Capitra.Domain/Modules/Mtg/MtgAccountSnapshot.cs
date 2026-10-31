using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_ACCOUNT_SNAPSHOTS: copy of FINOVA FNV_ACCOUNT_SNAPSHOT (APEX), all columns kept.
/// </summary>
public class MtgAccountSnapshot : AuditableEntity
{
    public long SnapshotId { get; set; }

    public DateOnly AsOfDate { get; set; }

    public long PortfolioAccountId { get; set; }

    public decimal OutstandingValue { get; set; }

    public decimal ArrearsAmount { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public string? PaymentStatus { get; set; }
}
