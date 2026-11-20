using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_QUEUES: copy of FINOVA GL_POSTING_QUEUE (APEX), all columns kept.
/// </summary>
public class GlPostingQueue : AuditableEntity
{
    public long QueueId { get; set; }

    public string QueueNo { get; set; } = string.Empty;

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public long? PostingProfileId { get; set; }

    public long? BatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? AccountingEventId { get; set; }

    public string? SourceModule { get; set; }

    public string? SourceDocumentType { get; set; }

    public string? SourceDocumentId { get; set; }

    public string? SourceDocumentNo { get; set; }

    public string? SourceTransactionId { get; set; }

    /// <summary>One of: QUEUED, READY, HOLD, LOCKED, POSTING, POSTED, ERROR, CANCELLED, SKIPPED.</summary>
    public string RequestStatus { get; set; } = "QUEUED";

    public int RequestPriority { get; set; } = 100;

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public DateTime? ReadyAt { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? HoldReason { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public DateTime? NextRetryAt { get; set; }

    public long? PostingRunId { get; set; }
}
