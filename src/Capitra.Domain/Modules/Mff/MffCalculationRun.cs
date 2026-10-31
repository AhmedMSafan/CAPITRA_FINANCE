using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CALCULATION_RUNS: copy of FINOVA MFF_CALCULATION_RUNS (APEX), all columns kept.
/// </summary>
public class MffCalculationRun : AuditableEntity
{
    public long CalcRunId { get; set; }

    public long? ApplicationId { get; set; }

    public long ProgramVersionId { get; set; }

    public long? FinancierId { get; set; }

    public string RunType { get; set; } = "LIVE";

    public decimal? InputIncome { get; set; }

    public decimal? InputPropertyValue { get; set; }

    public decimal? InputDownpayment { get; set; }

    public decimal? MortgageRatePct { get; set; }

    public decimal? TermYears { get; set; }

    public decimal? PaymentGrowthYears { get; set; }

    public decimal? WageIncreasePct { get; set; }

    public decimal? MaxPaymentIncomePct { get; set; }

    public decimal? MinDownpaymentPct { get; set; }

    public decimal? MaxDownpaymentPct { get; set; }

    public decimal? MaxSubsidyAmount { get; set; }

    public decimal? DiscountFactorPct { get; set; }

    public decimal? InputMonthlySubsidy { get; set; }

    public int? InputSubsidyMonths { get; set; }

    public decimal? OutBasicMortgage { get; set; }

    public decimal? OutMaxMortgage { get; set; }

    public decimal? OutPvBuydownSubsidy { get; set; }

    public decimal? OutDownpaymentAssist { get; set; }

    public decimal? OutTotalSubsidy { get; set; }

    public decimal? OutMortgageLoan { get; set; }

    public decimal? OutInitialPayment { get; set; }

    public decimal? OutLtvPct { get; set; }

    public decimal? OutBorrowerDownpayment { get; set; }

    public decimal? OutMaxPropertyValue { get; set; }

    public decimal? OutPropertyValue { get; set; }

    public decimal? OutMinPropertyValue { get; set; }

    public bool IsNegativeAmortization { get; set; }

    public string? WarningMessageAr { get; set; }

    public string CalcStatus { get; set; } = "COMPLETE";

    public string? SourceReference { get; set; }
}
