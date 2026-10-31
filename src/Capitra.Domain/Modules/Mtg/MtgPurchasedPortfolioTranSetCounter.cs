using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS: copy of FINOVA FNV_PP_TRAN_SET_COUNTER (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioTranSetCounter : AuditableEntity
{
    public string PortfolioNo { get; set; } = string.Empty;

    public int YearNo { get; set; }

    public int LastNo { get; set; }
}
