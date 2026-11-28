using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_APPROVAL_HIST: copy of FINOVA GL_JOURNAL_APPROVAL_HISTORY (APEX), all columns kept.
/// </summary>
public class GlJournalApprovalHistory : AuditableEntity
{
    public long ApprovalHistoryId { get; set; }

    public long? BatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public int ApprovalStepNo { get; set; }

    public string? ApproverRoleCode { get; set; }

    public string? ApproverUser { get; set; }

    /// <summary>One of: SUBMIT, APPROVE, REJECT, RETURN, DELEGATE, WITHDRAW, AUTO_APPROVE.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? ActionNotes { get; set; }

    public string? DelegatedFromUser { get; set; }

    public DateTime ActionAt { get; set; }
}
