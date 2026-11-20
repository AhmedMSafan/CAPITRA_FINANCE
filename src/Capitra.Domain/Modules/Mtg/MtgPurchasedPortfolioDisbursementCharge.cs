using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_CHARGES: each charge of a disbursement, how it was calculated (from its product
/// event rule component, or the financial review when the Disbursement step raises no business event) and its final amount.
/// </summary>
public class MtgPurchasedPortfolioDisbursementCharge : AuditableEntity
{
    public long DisbursementChargeId { get; set; }

    public long DisbursementId { get; set; }

    public long PortfolioId { get; set; }

    public int SequenceNo { get; set; }

    /// <summary>ADM_CHARGES.CHARGE_CODE.</summary>
    public string ChargeCode { get; set; } = string.Empty;

    public string ChargeNameEn { get; set; } = string.Empty;

    public string? ChargeNameAr { get; set; }

    /// <summary>The product event rule component the amount was calculated from; null when it came from the financial review.</summary>
    public long? RuleChargeId { get; set; }

    /// <summary>The rule component's method (FIXED, PERCENTAGE, PER_UNIT), or FINANCIAL_REVIEW.</summary>
    public string CalculationMethodCode { get; set; } = string.Empty;

    public string? AmountBaseCode { get; set; }

    public decimal? BaseAmount { get; set; }

    /// <summary>Rate in percent.</summary>
    public decimal? ChargeRate { get; set; }

    public decimal? PerUnitAmount { get; set; }

    public int? UnitCount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public decimal FinalAmount { get; set; }

    /// <summary>PRORATE or IMMEDIATE.</summary>
    public string IncomeRecognitionCode { get; set; } = "PRORATE";
}
