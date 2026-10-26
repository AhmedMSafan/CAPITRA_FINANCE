using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_RULE_APPROVAL_HIST: copy of FINOVA GL_AUTO_RULE_APPROVAL_HISTORY (APEX), all columns kept.
/// </summary>
public class GlAutoRuleApprovalHistory : AuditableEntity
{
    public long ApprovalHistoryId { get; set; }

    public long RuleSetId { get; set; }

    /// <summary>One of: CREATE, SUBMIT, APPROVE, REJECT, RETURN, ACTIVATE, DEACTIVATE, RETIRE, REVISE.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string ActionBy { get; set; } = string.Empty;

    public DateTime ActionAt { get; set; }

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public string? Comments { get; set; }
}
