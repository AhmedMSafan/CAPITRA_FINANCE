using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_VALUATIONS: copy of FINOVA FNV_VALUATION (APEX), all columns kept.
/// </summary>
public class MtgValuation : AuditableEntity
{
    public long ValuationId { get; set; }

    public long PortfolioId { get; set; }

    public long ProjectId { get; set; }

    public long? UnitId { get; set; }

    public string ValuationTypeCode { get; set; } = string.Empty;

    public string? ValuerName { get; set; }

    public string? ValuerRegistrationNo { get; set; }

    public DateOnly? ValuationDate { get; set; }

    public decimal? MarketValue { get; set; }

    public decimal? RecommendedValue { get; set; }

    public decimal? ProjectCompletionPct { get; set; }

    public string? FacilitiesStatus { get; set; }

    public long? ReportDocumentId { get; set; }

    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }
}
