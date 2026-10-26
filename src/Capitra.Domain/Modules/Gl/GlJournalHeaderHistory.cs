using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_JOURNAL_HEADERS_HIST: every action on a journal. The audit columns record who did it and when.</summary>
public class GlJournalHeaderHistory : AuditableEntity
{
    public long JournalHistoryId { get; set; }

    public long JournalHeaderId { get; set; }

    /// <summary>CREATE, UPDATE, SUBMIT, APPROVE, REJECT, POST, POST_FAILED, CANCEL or REVERSE.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string? ActionReason { get; set; }
}
