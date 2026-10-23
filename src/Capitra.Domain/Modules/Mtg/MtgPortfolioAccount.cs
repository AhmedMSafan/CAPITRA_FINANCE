using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PORTFOLIO_ACCOUNT: one investor's contract for one unit inside a portfolio
/// (APEX FNV_PURSHASED_PORTFOLIO_ACCOUNT, App 105 page 205).
/// </summary>
public class MtgPortfolioAccount : AuditableEntity
{
    /// <summary>The account stays in the deal.</summary>
    public const string SelectedDecision = "SELECTED";

    /// <summary>The account is left out of the deal.</summary>
    public const string ExcludedDecision = "EXCLUDED";

    public long PortfolioAccountId { get; set; }

    public long PortfolioId { get; set; }

    public long InvestorId { get; set; }

    public long ProjectUnitId { get; set; }

    /// <summary>The seller's contract number, set by the Excel import (page 208). Unique within a portfolio.</summary>
    public string? SellerContractNo { get; set; }

    public DateOnly? BookingDate { get; set; }

    public DateOnly? ContractDate { get; set; }

    public decimal ContractAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public decimal? InstallmentAmount { get; set; }

    /// <summary>Free text, as in APEX.</summary>
    public string? InstallmentFrequency { get; set; }

    public DateOnly? LastPaymentDate { get; set; }

    public int RegularPaymentMonths { get; set; }

    public decimal ArrearsAmount { get; set; }

    /// <summary>Free text, as in APEX.</summary>
    public string? AccountStatus { get; set; }

    /// <summary>SELECTED or EXCLUDED; set by the Excel import and changed in the AML review (page 209).</summary>
    public string? DealDecisionCode { get; set; }
}
