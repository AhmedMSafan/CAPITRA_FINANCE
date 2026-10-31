using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_PRODUCTS: the product catalog (FINOVA_FINANCEING ADM_PRODUCTS).</summary>
public class AdmProduct : AuditableEntity
{
    /// <summary>Code of the Mortgage Portfolio Purchasing product, the only one that uses the MPP rates.</summary>
    public const string MortgagePortfolioPurchasingCode = "MTG_PORTFOLIO_PURCHASING";

    public long ProductId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductNameEn { get; set; } = string.Empty;

    public string? ProductNameAr { get; set; }

    public string BusinessLineCode { get; set; } = string.Empty;

    public string ProductTypeCode { get; set; } = string.Empty;

    public string? FactoringModeCode { get; set; }

    /// <summary>With or without recourse; blank when it does not apply.</summary>
    public bool? IsRecourse { get; set; }

    public string? DefaultCurrencyCode { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Mortgage Portfolio Purchasing: PV rate, in percent.</summary>
    public decimal MppPvRate { get; set; }

    /// <summary>Mortgage Portfolio Purchasing: late fee rate, in percent.</summary>
    public decimal MppLateFeeRate { get; set; }

    /// <summary>Mortgage Portfolio Purchasing: reserved amount, in percent.</summary>
    public decimal MppReservedAmountPct { get; set; }

    /// <summary>Mortgage Portfolio Purchasing: investor payment, in percent.</summary>
    public decimal MppInvestorPaymentPct { get; set; }
}
