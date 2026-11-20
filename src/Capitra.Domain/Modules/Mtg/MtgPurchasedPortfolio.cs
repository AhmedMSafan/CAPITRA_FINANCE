using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIOS: copy of FINOVA FNV_PURSHASED_PORTFOLIO (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolio : AuditableEntity
{
    public long PortfolioId { get; set; }

    public string PortfolioNo { get; set; } = string.Empty;

    public long DeveloperId { get; set; }

    public long ProjectId { get; set; }

    public DateOnly RequestDate { get; set; }

    public string PortfolioStatusCode { get; set; } = "DRAFT";

    public decimal FaceValue { get; set; }

    public decimal OutstandingValue { get; set; }

    public decimal ProposedPurchaseValue { get; set; }

    public decimal ExpectedReturnAmount { get; set; }

    public int? TermMonths { get; set; }

    public decimal AdminExpenseAmount { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string? CurrentApprovalStep { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? BoardDecisionNo { get; set; }

    public DateOnly? BoardDecisionDate { get; set; }

    public string? BoardDecision { get; set; }

    public string? Comments { get; set; }

    public bool IsActive { get; set; } = true;

    public long? WorkflowInstanceId { get; set; }

    public long? WorkflowTaskId { get; set; }

    public string? RejectionReason { get; set; }

    public int SelectedAccountCount { get; set; }

    public decimal SelectedFaceValue { get; set; }

    public decimal SelectedOutstandingValue { get; set; }

    public decimal SelectedPaidValue { get; set; }

    public decimal LateFeeRate { get; set; }

    public decimal PvRate { get; set; }

    public decimal ReservedAmountPercent { get; set; }

    public decimal? CoveragePct { get; set; }

    public decimal? MarginPercentage { get; set; }

    public decimal? AdminExpensePercent { get; set; }

    public decimal? LegalFeePerUnit { get; set; }

    public decimal? ValuationFeePerUnit { get; set; }
}
