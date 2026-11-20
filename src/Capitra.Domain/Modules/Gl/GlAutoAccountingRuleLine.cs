using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_ACCOUNTING_RULE_LINES: copy of FINOVA GL_AUTO_ACCOUNTING_RULE_LINES (APEX), all columns kept.
/// </summary>
public class GlAutoAccountingRuleLine : AuditableEntity
{
    public long RuleLineId { get; set; }

    public long RuleSetId { get; set; }

    public int LineNo { get; set; }

    public string? LineCode { get; set; }

    public string? LineNameEn { get; set; }

    /// <summary>One of: DR, CR.</summary>
    public string DrCrFlag { get; set; } = string.Empty;

    public string? LineRoleCode { get; set; }

    /// <summary>One of: FIXED_COMBINATION, FIXED_ACCOUNT_WITH_SEGMENTS, SOURCE_COMBINATION, SOURCE_ACCOUNT, LOOKUP, EXPRESSION, SUSPENSE_ON_ERROR.</summary>
    public string AccountDerivationMethod { get; set; } = "FIXED_COMBINATION";

    public long? FixedAccountCombinationId { get; set; }

    public long? FixedAccountId { get; set; }

    public long? AccountSourceValueId { get; set; }

    public long? AccountLookupId { get; set; }

    /// <summary>One of: FIXED_AMOUNT, SOURCE_AMOUNT, PERCENTAGE_OF_SOURCE, FORMULA, BALANCE_LOOKUP, ZERO.</summary>
    public string AmountCalculationMethod { get; set; } = "SOURCE_AMOUNT";

    public long? AmountSourceValueId { get; set; }

    public string? AmountValueCode { get; set; }

    public decimal? FixedAmount { get; set; }

    public string? FixedAmountCurrency { get; set; }

    public long? PercentageBaseValueId { get; set; }

    public string? PercentageBaseCode { get; set; }

    public decimal? PercentageRate { get; set; }

    public string? FormulaExpression { get; set; }

    public decimal? MinimumAmount { get; set; }

    public decimal? MaximumAmount { get; set; }

    /// <summary>One of: ABSOLUTE, SIGNED, NEGATE, ZERO_IF_NEGATIVE, ZERO_IF_POSITIVE.</summary>
    public string SignHandling { get; set; } = "ABSOLUTE";

    /// <summary>One of: STANDARD, UP, DOWN, TRUNCATE, BANKERS.</summary>
    public string RoundingMethod { get; set; } = "STANDARD";

    public int RoundingPrecision { get; set; } = 2;

    /// <summary>One of: EVENT_CURRENCY, SOURCE_VALUE, FIXED_CURRENCY, LEDGER_CURRENCY.</summary>
    public string CurrencyDerivationMethod { get; set; } = "EVENT_CURRENCY";

    public long? CurrencySourceValueId { get; set; }

    public string? FixedCurrencyCode { get; set; }

    public string? LineDescriptionTemplate { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsZeroAmountAllowed { get; set; }

    public bool IsActive { get; set; } = true;
}
