using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCRUAL_RULE_LINES: copy of FINOVA GL_CLOSE_ACCRUAL_RULE_LINES (APEX), all columns kept.
/// </summary>
public class GlCloseAccrualRuleLine : AuditableEntity
{
    public long AccrualRuleLineId { get; set; }

    public long AccrualRuleId { get; set; }

    public int LineNo { get; set; }

    /// <summary>One of: DR, CR.</summary>
    public string DrCrFlag { get; set; } = string.Empty;

    /// <summary>One of: FIXED, LOOKUP, SEGMENT_DERIVATION, SOURCE_ACCOUNT.</summary>
    public string AccountDerivationMethod { get; set; } = "FIXED";

    public long? AccountCombinationId { get; set; }

    public string? AccountLookupCode { get; set; }

    public string? SegmentDerivationJson { get; set; }

    /// <summary>One of: FIXED, SOURCE_AMOUNT, PERCENTAGE_OF_SOURCE, FORMULA, BALANCE_BASED.</summary>
    public string AmountMethod { get; set; } = "SOURCE_AMOUNT";

    public string? AmountSourceCode { get; set; }

    public decimal? FixedAmount { get; set; }

    public decimal? PercentageValue { get; set; }

    public string? FormulaExpression { get; set; }

    /// <summary>One of: LEDGER, SOURCE, FIXED.</summary>
    public string CurrencyDerivation { get; set; } = "LEDGER";

    /// <summary>One of: STANDARD, ROUND_UP, ROUND_DOWN, NO_ROUNDING.</summary>
    public string RoundingRule { get; set; } = "STANDARD";

    public string? LineDescriptionTemplate { get; set; }

    public bool IsActive { get; set; } = true;
}
