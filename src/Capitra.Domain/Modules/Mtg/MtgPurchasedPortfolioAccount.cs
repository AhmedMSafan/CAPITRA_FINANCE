using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_ACCOUNTS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_ACCOUNT (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioAccount : AuditableEntity
{
    public long PortfolioAccountId { get; set; }

    public long PortfolioId { get; set; }

    public long InvestorId { get; set; }

    public long UnitId { get; set; }

    public DateOnly? BookingDate { get; set; }

    public DateOnly? ContractDate { get; set; }

    public decimal ContractValue { get; set; }

    public decimal PaidValue { get; set; }

    public decimal OutstandingValue { get; set; }

    public decimal? InstallmentAmount { get; set; }

    public string? InstallmentFrequency { get; set; }

    public DateOnly? LastPaymentDate { get; set; }

    public int RegularPaymentMonths { get; set; }

    public decimal ArrearsAmount { get; set; }

    public string? AccountStatus { get; set; } = "ACTIVE";

    public string? SellerContractNo { get; set; }

    /// <summary>One of: SELECTED, EXCLUDED.</summary>
    public string? DealDecisionCode { get; set; } = "SELECTED";
}
