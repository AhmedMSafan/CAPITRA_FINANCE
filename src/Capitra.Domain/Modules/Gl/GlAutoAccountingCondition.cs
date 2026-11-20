using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_ACCOUNTING_CONDITIONS: copy of FINOVA GL_AUTO_ACCOUNTING_CONDITIONS (APEX), all columns kept.
/// </summary>
public class GlAutoAccountingCondition : AuditableEntity
{
    public long ConditionId { get; set; }

    /// <summary>One of: RULE_SET, RULE_LINE, SEGMENT_LINE, LOOKUP_LINE.</summary>
    public string OwnerType { get; set; } = string.Empty;

    public long OwnerId { get; set; }

    public int ConditionGroupNo { get; set; } = 1;

    public int SequenceNo { get; set; } = 1;

    public long? LeftSourceValueId { get; set; }

    public string? LeftValueCode { get; set; }

    public string? LeftExpression { get; set; }

    /// <summary>One of: EQ, NE, GT, GE, LT, LE, BETWEEN, IN, NOT_IN, LIKE, NOT_LIKE, IS_NULL, IS_NOT_NULL.</summary>
    public string OperatorCode { get; set; } = string.Empty;

    /// <summary>One of: LITERAL, SOURCE_VALUE, EXPRESSION, NONE.</summary>
    public string CompareType { get; set; } = "LITERAL";

    public long? CompareSourceValueId { get; set; }

    public string? CompareValueCode { get; set; }

    public string? CompareTextValue { get; set; }

    public decimal? CompareNumberValue { get; set; }

    public DateOnly? CompareDateValueDate { get; set; }

    public decimal? CompareFromNumber { get; set; }

    public decimal? CompareToNumber { get; set; }

    /// <summary>One of: AND, OR.</summary>
    public string JoinOperator { get; set; } = "AND";

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
