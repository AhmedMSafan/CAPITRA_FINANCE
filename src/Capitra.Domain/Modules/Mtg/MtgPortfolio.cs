using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PORTFOLIO: a mortgage portfolio being bought from a developer (APEX FNV_PURSHASED_PORTFOLIO,
/// App 105 pages 201-203). Face value and outstanding are totals of its accounts, kept by the service.
/// </summary>
public class MtgPortfolio : AuditableEntity
{
    /// <summary>Status every new portfolio starts in.</summary>
    public const string DraftStatus = "DRAFT";

    /// <summary>Set when the seller workbook has been imported (workflow step "Excel Import &amp; Validation", page 208).</summary>
    public const string DataLoadingCompletedStatus = "DATA_LOADING_COMPLETED";

    /// <summary>Set by the AML review step (page 209).</summary>
    public const string AmlAcceptedStatus = "AML_ACCEPTED";

    /// <summary>Set by the AML review step (page 209); the reason is kept in <see cref="RejectionReason"/>.</summary>
    public const string AmlRejectedStatus = "AML_REJECTED";

    /// <summary>Set by the portfolio inquiries step (page 210).</summary>
    public const string InquiryAcceptedStatus = "INQUIRY_ACCEPTED";

    /// <summary>Set by the portfolio inquiries step (page 210).</summary>
    public const string InquiryRejectedStatus = "INQUIRY_REJECTED";

    /// <summary>Set by the legal review step (page 211).</summary>
    public const string LegalReviewAcceptedStatus = "LEGAL_REVIEW_ACCEPTED";

    /// <summary>Set by the legal review step (page 211).</summary>
    public const string LegalReviewRejectedStatus = "LEGAL_REVIEW_REJECTED";

    /// <summary>Set by the valuation step (page 401).</summary>
    public const string ValuationCompletedStatus = "VALUATION_COMPLETED";

    /// <summary>Set by the coverage review step (page 212).</summary>
    public const string CoverageReviewAcceptedStatus = "COVERAGE_REVIEW_ACCEPTED";

    /// <summary>Set by the coverage review step (page 212); the reason is kept in <see cref="RejectionReason"/>.</summary>
    public const string CoverageReviewRejectedStatus = "COVERAGE_REVIEW_REJECTED";

    /// <summary>Set by the cheque coverage step (page 213).</summary>
    public const string ChequeCoverageAcceptedStatus = "CHEQUE_COVERAGE_ACCEPTED";

    /// <summary>Set by the cheque coverage step (page 213); the reason is kept in <see cref="RejectionReason"/>.</summary>
    public const string ChequeCoverageRejectedStatus = "CHEQUE_COVERAGE_REJECTED";

    public long PortfolioId { get; set; }

    public string PortfolioNo { get; set; } = string.Empty;

    public long DeveloperId { get; set; }

    public long ProjectId { get; set; }

    public DateOnly RequestDate { get; set; }

    /// <summary>MTG_LKP_PORTFOLIO_STATUS code.</summary>
    public string PortfolioStatusCode { get; set; } = DraftStatus;

    /// <summary>ADM_LKP_CURRENCY code.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    public int? TermMonths { get; set; }

    public string? Comments { get; set; }

    public decimal FaceValueAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public decimal ProposedPurchaseAmount { get; set; }

    public decimal ExpectedReturnAmount { get; set; }

    public decimal AdminExpenseAmount { get; set; }

    public decimal? CoveragePct { get; set; }

    public decimal? ReservedAmountPct { get; set; }

    /// <summary>Present value calculation rate.</summary>
    public decimal? PvRate { get; set; }

    public decimal? LateFeeRate { get; set; }

    public decimal? MarginPct { get; set; }

    public decimal? AdminExpensePct { get; set; }

    public decimal? LegalFeePerUnitAmount { get; set; }

    public decimal? ValuationFeePerUnitAmount { get; set; }

    /// <summary>Why the last review rejected the portfolio; cleared when a review accepts it.</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Accounts kept in the deal (deal decision SELECTED), counted when AML is accepted or data is loaded.</summary>
    public int? SelectedAccountCount { get; set; }

    public decimal? SelectedFaceValueAmount { get; set; }

    public decimal? SelectedOutstandingAmount { get; set; }

    public decimal? SelectedPaidAmount { get; set; }
}
