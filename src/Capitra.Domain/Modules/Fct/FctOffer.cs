using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_OFFERS: copy of FINOVA FCT_OFFERS (APEX), all columns kept.
/// </summary>
public class FctOffer : AuditableEntity
{
    public long OfferId { get; set; }

    public string OfferRef { get; set; } = string.Empty;

    public long ClientId { get; set; }

    public string FactoringType { get; set; } = string.Empty;

    public string RecourseType { get; set; } = string.Empty;

    public string? ProductCode { get; set; }

    public long CurrencyId { get; set; }

    public decimal RequestedLimit { get; set; }

    public decimal AdvanceRatePct { get; set; }

    public decimal? MaxAdvanceRatePct { get; set; }

    public decimal? MinAdvanceRatePct { get; set; }

    public decimal? ReserveRatePct { get; set; }

    public long? RateTypeId { get; set; }

    public decimal? BaseRatePct { get; set; }

    public decimal? SpreadPct { get; set; }

    public decimal? EffectiveRatePct { get; set; }

    public decimal? MarginPct { get; set; }

    public decimal? PenaltyRatePct { get; set; }

    public string? DayCountConvention { get; set; }

    public string? InterestCalcMethod { get; set; }

    public string? InterestFreq { get; set; }

    public DateTime? OfferAt { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateOnly? ClientResponseDate { get; set; }

    public int? AvgTenorDays { get; set; }

    public int? MaxInvoiceTenorDays { get; set; }

    public int? MinInvoiceTenorDays { get; set; }

    public decimal? MaxDebtorConcentrationPct { get; set; }

    public decimal? MaxSingleInvoicePct { get; set; }

    public decimal? MaxSingleInvoiceAmt { get; set; }

    public decimal? MinInvoiceAmount { get; set; }

    public decimal? MaxDilutionPct { get; set; }

    public decimal? DilutionReservePct { get; set; }

    public string? ApprovedBy { get; set; }

    public string? RelationshipManager { get; set; }

    public long? RmEmployeeId { get; set; }

    public string? Notes { get; set; }

    public string? RiskBand { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public bool IsActive { get; set; } = true;

    public string? StatusCode { get; set; }

    public decimal? MaxLimitTenorMth { get; set; }

    public string? AgreementNo { get; set; }
}
