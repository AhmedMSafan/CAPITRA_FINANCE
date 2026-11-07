using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_COMBINATION_RULE_LINES: copy of FINOVA GL_ACCOUNT_COMBINATION_RULE_LINES (APEX), all columns kept.
/// </summary>
public class GlAccountCombinationRuleLine : AuditableEntity
{
    public long RuleLineId { get; set; }

    public long RuleId { get; set; }

    public int LineNo { get; set; }

    public string? SegmentCode { get; set; }

    public string? SegmentType { get; set; }

    public string? AttributeName { get; set; }

    /// <summary>One of: EQ, NE, IN, NOT_IN, LIKE, NOT_LIKE, BETWEEN, IS_NULL, IS_NOT_NULL.</summary>
    public string OperatorCode { get; set; } = string.Empty;

    public string? ValueFrom { get; set; }

    public string? ValueTo { get; set; }

    public string? ValueList { get; set; }

    /// <summary>One of: INCLUDE, EXCLUDE.</summary>
    public string IncludeExcludeFlag { get; set; } = "INCLUDE";
}
