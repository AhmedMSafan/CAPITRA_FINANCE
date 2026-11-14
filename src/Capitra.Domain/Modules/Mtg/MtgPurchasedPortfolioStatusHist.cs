using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_STATUS_HIST: copy of FINOVA FNV_PURSHASED_PORTFOLIO_STATUS_HIST (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioStatusHist : AuditableEntity
{
    public long StatusHistId { get; set; }

    public long PortfolioId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public DateTime ChangeAt { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public string? Comments { get; set; }
}
