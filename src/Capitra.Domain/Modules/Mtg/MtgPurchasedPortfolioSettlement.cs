using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_SETTLEMENTS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_SETTLEMENT (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioSettlement : AuditableEntity
{
    public long SettlementId { get; set; }

    public long PortfolioId { get; set; }

    public DateOnly? ContractExecutionDate { get; set; }

    public DateOnly? CutOffDate { get; set; }

    public DateOnly? AcquisitionDate { get; set; }

    public DateOnly? SettlementDate { get; set; }

    public DateOnly? AccountingDate { get; set; }

    public decimal ApprovedPurchaseValue { get; set; }

    public decimal? PaymentAmount { get; set; }

    public decimal AcquisitionCostAmount { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string? PaymentMethodCode { get; set; }

    public string? BankGlAccountCode { get; set; }

    public string? BankName { get; set; }

    public string? BankAccountNo { get; set; }

    public string? TransferReference { get; set; }

    public string? ChequeNo { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public string? SellerPaymentReference { get; set; }

    /// <summary>One of: DRAFT, SETTLED, CANCELLED.</summary>
    public string SettlementStatusCode { get; set; } = "DRAFT";

    /// <summary>One of: UNPOSTED, READY, POSTED, FAILED.</summary>
    public string AccountingStatusCode { get; set; } = "UNPOSTED";

    public string? GlBatchReference { get; set; }

    public string? GlJournalReference { get; set; }

    public string? Notes { get; set; }

    public string? SettledBy { get; set; }

    public DateTime? SettledOnAt { get; set; }
}
