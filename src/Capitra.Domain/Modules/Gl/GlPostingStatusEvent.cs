using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_STATUS_EVENTS: copy of FINOVA GL_POSTING_STATUS_EVENTS (APEX), all columns kept.
/// </summary>
public class GlPostingStatusEvent : AuditableEntity
{
    public long PostingStatusEventId { get; set; }

    public long? PostingRunId { get; set; }

    public long? QueueId { get; set; }

    public long? JournalBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public string? SourceModule { get; set; }

    public string? SourceDocumentId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string EventCode { get; set; } = string.Empty;

    public string? EventMessage { get; set; }

    public string EventBy { get; set; } = string.Empty;

    public DateTime EventAt { get; set; }
}
