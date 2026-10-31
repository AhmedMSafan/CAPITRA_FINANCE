using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_PORTFOLIO_STATUSES: copy of FINOVA REF_PORTFOLIO_STATUS (APEX), all columns kept.
/// </summary>
public class MtgLkpPortfolioStatus : AuditableEntity
{
    public string StatusCode { get; set; } = string.Empty;

    public string StatusNameAr { get; set; } = string.Empty;

    public string StatusNameEn { get; set; } = string.Empty;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
