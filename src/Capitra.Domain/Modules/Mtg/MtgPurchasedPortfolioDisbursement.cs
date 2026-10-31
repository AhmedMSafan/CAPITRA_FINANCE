using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS: copy of FINOVA FNV_PP_DISBURSEMENT (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioDisbursement : AuditableEntity
{
    public long DisbursementId { get; set; }

    public long PortfolioId { get; set; }

    public string TranSetId { get; set; } = string.Empty;

    public DateOnly DisbursementDate { get; set; }

    public long? BankId { get; set; }

    public long? BankAccountId { get; set; }

    public string? BankGlAccount { get; set; }

    public string? PaymentRef { get; set; }

    public string? CurrencyCode { get; set; }

    public string? PvMethod { get; set; }

    public decimal? PvRate { get; set; }

    public DateOnly? PvBaseDate { get; set; }

    public int? ChequeCount { get; set; }

    public int? ContractCount { get; set; }

    public decimal? FaceAmount { get; set; }

    public decimal? PvAmount { get; set; }

    public decimal? FundingRecoveryAmount { get; set; }

    public decimal? MarginAmount { get; set; }

    public decimal? AdminFrAmount { get; set; }

    public decimal? AdminFinalAmount { get; set; }

    public decimal? LegalFrAmount { get; set; }

    public decimal? LegalFinalAmount { get; set; }

    public decimal? ValuationFrAmount { get; set; }

    public decimal? ValuationFinalAmount { get; set; }

    public decimal? FeesAmount { get; set; }

    public decimal? BoardPriceAmount { get; set; }

    public decimal? PayableAmount { get; set; }

    public int? ClientsCreated { get; set; }

    public int? ClientsMatched { get; set; }

    public string? GlBatchNo { get; set; }

    public string? GlStatus { get; set; }

    public string? GlResponse { get; set; }

    public long? WorkflowTaskId { get; set; }

    public string? Comments { get; set; }

    public string StatusCode { get; set; } = "COMPLETED";

    public string? MarginTreatment { get; set; }

    public string? AdminTreatment { get; set; }

    public string? LegalTreatment { get; set; }

    public string? ValuationTreatment { get; set; }

    public decimal? MarginDeferredAmount { get; set; }

    public decimal? MarginImmediateAmount { get; set; }

    public decimal? FeesDeferredAmount { get; set; }

    public decimal? FeesImmediateAmount { get; set; }
}
