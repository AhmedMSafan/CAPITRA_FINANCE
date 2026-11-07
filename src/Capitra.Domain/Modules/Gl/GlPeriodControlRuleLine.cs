using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_CONTROL_RULE_LINES: copy of FINOVA GL_PERIOD_CONTROL_RULE_LINES (APEX), all columns kept.
/// </summary>
public class GlPeriodControlRuleLine : AuditableEntity
{
    public long PeriodRuleLineId { get; set; }

    public long PeriodRuleId { get; set; }

    public int LineNo { get; set; }

    /// <summary>One of: SOURCE_STATUS, TASK_STATUS, UNPOSTED_COUNT, UNTRANSFERRED_COUNT, ERROR_COUNT, PERIOD_STATUS, CUSTOM.</summary>
    public string ConditionType { get; set; } = string.Empty;

    public string ConditionCode { get; set; } = string.Empty;

    /// <summary>One of: EQ, NE, GT, GE, LT, LE, IN, NOT_IN, IS_NULL, IS_NOT_NULL.</summary>
    public string OperatorCode { get; set; } = "EQ";

    public string? ConditionValue { get; set; }

    public bool IsActive { get; set; } = true;
}
