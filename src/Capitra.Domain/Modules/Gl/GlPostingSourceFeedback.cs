using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_SOURCE_FEEDBACKS: copy of FINOVA GL_POSTING_SOURCE_FEEDBACK (APEX), all columns kept.
/// </summary>
public class GlPostingSourceFeedback : AuditableEntity
{
    public long SourceFeedbackId { get; set; }

    public long? PostingRunId { get; set; }

    public long? QueueId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? AccountingEventId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string? SourceDocumentType { get; set; }

    public string SourceDocumentId { get; set; } = string.Empty;

    public string? SourceDocumentNo { get; set; }

    public string? SourceTransactionId { get; set; }

    /// <summary>One of: NOT_GENERATED, DRAFT_ACCOUNTING, READY_FOR_REVIEW, APPROVED_FOR_POSTING, QUEUED, POSTING, POSTED, REVERSED, ERROR, CANCELLED.</summary>
    public string PostingStatus { get; set; } = string.Empty;

    public string? GlJournalNo { get; set; }

    public DateTime? PostedAt { get; set; }

    public bool IsReversal { get; set; }

    /// <summary>One of: PENDING, SENT, ERROR, NOT_REQUIRED.</summary>
    public string FeedbackStatus { get; set; } = "PENDING";

    public int FeedbackAttempts { get; set; }

    public DateTime? LastAttemptAt { get; set; }

    public string? LastErrorMessage { get; set; }

    public string? PayloadJson { get; set; }
}
