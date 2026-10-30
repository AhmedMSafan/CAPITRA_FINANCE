using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_DISBURSEMENT_CHARGES: copy of FINOVA FNV_DISBURSEMENT_CHARGES (APEX). The charges come from Capitra's product event rules
/// (Ahmed, 30 Oct): PRODUCT_EVENT_RULE_ID is ADM_PRODUCT_EVENT_RULES and RULE_CHARGE_ID (FINOVA's EVENT_ID and COMPONENT_ID) is the
/// rule's charge component in ADM_PRODUCT_EVENT_RULE_CHARGES; RULE_CODE and RULE_NAME are the charge's code and name.
/// </summary>
public class FctDisbursementCharge : AuditableEntity
{
    public long DisbursementChargeId { get; set; }

    public long DisbursementId { get; set; }

    public long ProductEventRuleId { get; set; }

    public long ProductId { get; set; }

    public long RuleChargeId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public string CalculationMethod { get; set; } = string.Empty;

    public string? BaseCode { get; set; }

    public decimal? BaseAmount { get; set; }

    /// <summary>FINOVA UNIT_COUNT: the invoice or cheque count or tenor days of a PER_UNIT charge, else the base amount.</summary>
    public decimal? Units { get; set; }

    public decimal? FixedAmount { get; set; }

    public decimal? RatePercent { get; set; }

    public decimal? PerUnitAmount { get; set; }

    public decimal? SpreadPercent { get; set; }

    public int? DayCountBasis { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal TaxRatePercent { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public long CurrencyId { get; set; }

    public string ChargeTimingCode { get; set; } = string.Empty;

    /// <summary>The rule charge's PRORATE or IMMEDIATE, for the GL entries.</summary>
    public string IncomeRecognitionCode { get; set; } = "PRORATE";

    public DateOnly? DueDate { get; set; }

    /// <summary>One of: CALCULATED, DUE, PARTIALLY_COLLECTED, COLLECTED, WAIVED, REVERSED, CANCELLED.</summary>
    public string StatusCode { get; set; } = "CALCULATED";

    public decimal CollectedAmount { get; set; }

    public decimal WaivedAmount { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime CalculationAt { get; set; }
}
