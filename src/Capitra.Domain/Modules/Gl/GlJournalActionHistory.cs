using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_ACTION_HIST: copy of FINOVA GL_JOURNAL_ACTION_HISTORY (APEX), all columns kept.
/// </summary>
public class GlJournalActionHistory : AuditableEntity
{
    public long ActionHistoryId { get; set; }

    public long? BatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    public string ActionCode { get; set; } = string.Empty;

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? ActionReason { get; set; }

    public string? ActionNotes { get; set; }

    public string ActionBy { get; set; } = string.Empty;

    public DateTime ActionAt { get; set; }
}
