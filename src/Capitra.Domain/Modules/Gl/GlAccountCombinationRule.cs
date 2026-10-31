using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_COMBINATION_RULES: copy of FINOVA GL_ACCOUNT_COMBINATION_RULES (APEX), all columns kept.
/// </summary>
public class GlAccountCombinationRule : AuditableEntity
{
    public long RuleId { get; set; }

    public long CoaId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    /// <summary>One of: ALLOW, DENY, REQUIRE, WARNING.</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>One of: ACCOUNT_COMBINATION, ACCOUNT, BUSINESS_LINE_ACCOUNT, BRANCH_ACCOUNT, COST_CENTER_ACCOUNT, INTERCOMPANY, SEGMENT_VALUE.</summary>
    public string RuleScope { get; set; } = string.Empty;

    /// <summary>One of: ERROR, WARNING.</summary>
    public string ValidationLevel { get; set; } = "ERROR";

    public int PriorityNo { get; set; } = 100;

    public string? ErrorMessageEn { get; set; }

    public string? ErrorMessageAr { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }
}
