using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS: copy of FINOVA FNV_PORTFOLIO_FINANCIAL_REVIEW (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioFinancialReview : AuditableEntity
{
    public long ReviewId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>One of: ANNUAL, MONTHLY, SIMPLE.</summary>
    public string PvMethod { get; set; } = "ANNUAL";

    public decimal? PvRate { get; set; }

    public DateOnly? PvBaseDate { get; set; }

    public int? ChequeCount { get; set; }

    public decimal? ChequesFaceAmount { get; set; }

    public decimal? ChequesPvAmount { get; set; }

    /// <summary>One of: SELECTED_OUTSTANDING, CHEQUES_PV.</summary>
    public string MarginBase { get; set; } = "SELECTED_OUTSTANDING";

    public decimal? MarginPercentage { get; set; }

    public decimal? MarginBaseAmount { get; set; }

    public decimal? MarginAmount { get; set; }

    /// <summary>One of: SELECTED_OUTSTANDING, SELECTED_FACE, CHEQUES_PV, PROPOSED_PURCHASE.</summary>
    public string AdminBase { get; set; } = "SELECTED_OUTSTANDING";

    public decimal? AdminExpensePercent { get; set; }

    public decimal? AdminBaseAmount { get; set; }

    public decimal? AdminCalcAmount { get; set; }

    public decimal? AdminFinalAmount { get; set; }

    public bool IsAdminIncluded { get; set; } = true;

    public int? UnitsCount { get; set; }

    public decimal? LegalFeePerUnit { get; set; }

    public decimal? LegalCalcAmount { get; set; }

    public decimal? LegalFinalAmount { get; set; }

    public bool IsLegalIncluded { get; set; } = true;

    public decimal? ValuationFeePerUnit { get; set; }

    public decimal? ValuationCalcAmount { get; set; }

    public decimal? ValuationFinalAmount { get; set; }

    public bool IsValuationIncluded { get; set; } = true;

    public decimal? TotalChargesAmount { get; set; }

    public decimal? ProposedPurchaseValue { get; set; }

    public decimal? ExpectedReturnAmount { get; set; }
}
