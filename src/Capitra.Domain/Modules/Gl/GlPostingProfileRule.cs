using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_PROFILE_RULES: copy of FINOVA GL_POSTING_PROFILE_RULES (APEX), all columns kept.
/// </summary>
public class GlPostingProfileRule : AuditableEntity
{
    public long ProfileRuleId { get; set; }

    public long PostingProfileId { get; set; }

    public int RuleSeq { get; set; }

    /// <summary>One of: ELIGIBILITY, PERIOD, BALANCE, SOURCE, APPROVAL, CONTROL.</summary>
    public string RuleScope { get; set; } = "ELIGIBILITY";

    public string ConditionAttribute { get; set; } = string.Empty;

    /// <summary>One of: =, <>, >, >=, <, <=, IN, NOT_IN, BETWEEN, IS_NULL, IS_NOT_NULL.</summary>
    public string ConditionOperator { get; set; } = string.Empty;

    public string? ConditionValue1 { get; set; }

    public string? ConditionValue2 { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsWarning { get; set; }

    public bool IsActive { get; set; } = true;
}
