using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_CONTROL_RULES: copy of FINOVA GL_PERIOD_CONTROL_RULES (APEX), all columns kept.
/// </summary>
public class GlPeriodControlRule : AuditableEntity
{
    public long PeriodRuleId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    /// <summary>One of: OPEN_VALIDATION, POSTING_VALIDATION, CLOSE_VALIDATION, PERMANENT_CLOSE_VALIDATION.</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string SeverityCode { get; set; } = "ERROR";

    public long? LedgerId { get; set; }

    public string? SourceCode { get; set; }

    public string? BusinessLineCode { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplaySeq { get; set; } = 10;

    public string? MessageEn { get; set; }

    public string? MessageAr { get; set; }
}
