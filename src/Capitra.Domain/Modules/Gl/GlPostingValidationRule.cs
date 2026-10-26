using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_VALIDATION_RULES: copy of FINOVA GL_POSTING_VALIDATION_RULES (APEX), all columns kept.
/// </summary>
public class GlPostingValidationRule : AuditableEntity
{
    public long PostingValidationRuleId { get; set; }

    public long ValidationSetId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    /// <summary>One of: PRE_QUEUE, PRE_LOCK, PRE_POST, BALANCE_UPDATE, POST_BALANCE, SOURCE_NOTIFY.</summary>
    public string ValidationStage { get; set; } = "PRE_POST";

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string SeverityCode { get; set; } = "ERROR";

    public int RuleSeq { get; set; } = 10;

    public bool IsStopOnFailure { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? ErrorMessage { get; set; }
}
