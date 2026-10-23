using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_VALUATION: one valuation of the portfolio's project, or of one of its units, by a registered valuer
/// (APEX FNV_VALUATION, App 105 page 401).
/// </summary>
public class MtgValuation : AuditableEntity
{
    public long ValuationId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>The portfolio's project at the time of the valuation.</summary>
    public long ProjectId { get; set; }

    /// <summary>The unit valued; null when the whole project was valued.</summary>
    public long? ProjectUnitId { get; set; }

    /// <summary>MTG_LKP_VALUATION_TYPE code.</summary>
    public string ValuationTypeCode { get; set; } = string.Empty;

    public string ValuerName { get; set; } = string.Empty;

    public string? ValuerRegistrationNo { get; set; }

    public DateOnly ValuationDate { get; set; }

    public decimal MarketValueAmount { get; set; }

    public decimal? RecommendedValueAmount { get; set; }

    /// <summary>How far construction has progressed, 0 to 100.</summary>
    public decimal? ProjectCompletionPct { get; set; }

    /// <summary>State of roads, utilities and shared facilities, as the valuer describes it.</summary>
    public string? FacilitiesStatus { get; set; }

    /// <summary>Name of who approved the valuation, as typed.</summary>
    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }
}
