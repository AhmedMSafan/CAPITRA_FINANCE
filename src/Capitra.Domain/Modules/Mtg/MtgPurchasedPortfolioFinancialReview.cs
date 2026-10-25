using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_FINANCIAL_REVIEWS: how the purchase price of a portfolio was worked out, one per portfolio, saved with
/// every input so the figures can be explained later (APEX FNV_PORTFOLIO_FINANCIAL_REVIEW, App 105 page 214).
/// </summary>
public class MtgPurchasedPortfolioFinancialReview : AuditableEntity
{
    public long FinancialReviewId { get; set; }

    public long PortfolioId { get; set; }

    /// <summary>ANNUAL, MONTHLY or SIMPLE discounting of each cheque.</summary>
    public string PvMethodCode { get; set; } = string.Empty;

    /// <summary>The portfolio's present value rate when this was saved, % per year.</summary>
    public decimal PvRate { get; set; }

    /// <summary>The day cheques were discounted to.</summary>
    public DateOnly PvBaseDate { get; set; }

    public int ChequeCount { get; set; }

    public decimal ChequesFaceAmount { get; set; }

    public decimal ChequesPvAmount { get; set; }

    /// <summary>SELECTED_OUTSTANDING or CHEQUES_PV.</summary>
    public string MarginBaseCode { get; set; } = string.Empty;

    public decimal MarginPct { get; set; }

    public decimal MarginBaseAmount { get; set; }

    public decimal MarginAmount { get; set; }

    /// <summary>SELECTED_OUTSTANDING, SELECTED_FACE, CHEQUES_PV or PROPOSED_PURCHASE.</summary>
    public string AdminBaseCode { get; set; } = string.Empty;

    public decimal AdminExpensePct { get; set; }

    public decimal AdminBaseAmount { get; set; }

    public decimal AdminCalcAmount { get; set; }

    /// <summary>The calculated amount, or the amount the reviewer typed instead.</summary>
    public decimal AdminFinalAmount { get; set; }

    public bool IsAdminIncluded { get; set; }

    /// <summary>Units of the contracts kept in the deal.</summary>
    public int UnitCount { get; set; }

    public decimal LegalFeePerUnitAmount { get; set; }

    public decimal LegalCalcAmount { get; set; }

    public decimal LegalFinalAmount { get; set; }

    public bool IsLegalIncluded { get; set; }

    public decimal ValuationFeePerUnitAmount { get; set; }

    public decimal ValuationCalcAmount { get; set; }

    public decimal ValuationFinalAmount { get; set; }

    public bool IsValuationIncluded { get; set; }

    public decimal TotalChargesAmount { get; set; }

    /// <summary>Cheques' present value less margin and the included charges.</summary>
    public decimal ProposedPurchaseAmount { get; set; }

    /// <summary>What the cheques pay less what is paid for the portfolio.</summary>
    public decimal ExpectedReturnAmount { get; set; }
}
