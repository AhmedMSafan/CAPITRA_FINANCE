using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_PRODUCT_EVENT_RULE_CHARGES: a charge of a product event rule and how it is priced (FINOVA_FINANCEING ADM_PRODUCT_EVENT_RULE_COMPONENTS).</summary>
public class AdmProductEventRuleCharge : AuditableEntity
{
    public long RuleChargeId { get; set; }

    public long ProductEventRuleId { get; set; }

    public long ChargeId { get; set; }

    public int SequenceNo { get; set; } = 10;

    /// <summary>One of: FIXED, PERCENTAGE, PER_UNIT, TIERED, INDEX_PLUS_SPREAD, DAILY_INTEREST.</summary>
    public string CalculationMethodCode { get; set; } = string.Empty;

    public decimal? FixedAmount { get; set; }

    /// <summary>Rate in percent.</summary>
    public decimal? ChargeRate { get; set; }

    public decimal? PerUnitAmount { get; set; }

    /// <summary>What a PERCENTAGE rate is applied to; one of <see cref="AmountBases"/>.</summary>
    public string? AmountBaseCode { get; set; }

    /// <summary>How the charge is recognised as income; one of <see cref="IncomeRecognitions"/>.</summary>
    public string IncomeRecognitionCode { get; set; } = "PRORATE";

    /// <summary>Spread over the index, in percent.</summary>
    public decimal SpreadRate { get; set; }

    /// <summary>360, 365 or 366.</summary>
    public int? DayCountBasis { get; set; } = 365;

    public decimal? MinimumAmount { get; set; }

    public decimal? MaximumAmount { get; set; }

    public int RoundingScale { get; set; } = 2;

    /// <summary>One of: DUE_SEPARATELY, DEDUCT_FROM_DISBURSEMENT, FINANCE_WITH_PRINCIPAL, PAID_UPFRONT.</summary>
    public string ChargeTimingCode { get; set; } = "DUE_SEPARATELY";

    public int DueDays { get; set; }

    /// <summary>Tax rate in percent.</summary>
    public decimal TaxRate { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public static readonly string[] AmountBases = ["FACE_VALUE", "PRESENT_VALUE", "PV_LESS_MARGIN"];

    /// <summary>PRORATE: deferred and released with the effective interest; IMMEDIATE: income on the event date.</summary>
    public static readonly string[] IncomeRecognitions = ["PRORATE", "IMMEDIATE"];
}
