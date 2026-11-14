using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_FX_REVALUATION_RULES: copy of FINOVA GL_FX_REVALUATION_RULES (APEX), all columns kept.
/// </summary>
public class GlFxRevaluationRule : AuditableEntity
{
    public long RevaluationRuleId { get; set; }

    public long LedgerId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    public long RateTypeId { get; set; }

    /// <summary>One of: PERIOD_END_BALANCE, TRANSACTION_OPEN_BALANCE.</summary>
    public string RevaluationMethod { get; set; } = "PERIOD_END_BALANCE";

    /// <summary>One of: ALL_FOREIGN_BALANCES, BANK_ONLY, CUSTOMER_ONLY, SUPPLIER_ONLY, ACCOUNT_RANGE, MANUAL_SELECTION.</summary>
    public string RevaluationScope { get; set; } = "ALL_FOREIGN_BALANCES";

    /// <summary>One of: ACTUAL, BUDGET, STAT.</summary>
    public string SourceBalanceType { get; set; } = "ACTUAL";

    public long? GainAccountCombinationId { get; set; }

    public long? LossAccountCombinationId { get; set; }

    public bool IsReversalRequired { get; set; } = true;

    public bool IsAutoPost { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? Notes { get; set; }
}
