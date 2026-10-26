using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_BATCHES: copy of FINOVA GL_JOURNAL_BATCHES (APEX), all columns kept.
/// </summary>
public class GlJournalBatch : AuditableEntity
{
    public long BatchId { get; set; }

    public long LedgerId { get; set; }

    public long? LedgerPeriodId { get; set; }

    public string BatchNo { get; set; } = string.Empty;

    public string BatchName { get; set; } = string.Empty;

    public long SourceId { get; set; }

    public long? CategoryId { get; set; }

    public DateOnly BatchDate { get; set; }

    /// <summary>One of: DRAFT, VALIDATED, SUBMITTED, APPROVED, POSTING, POSTED, REVERSED, CANCELLED, ERROR.</summary>
    public string BatchStatus { get; set; } = "DRAFT";

    /// <summary>One of: NOT_SUBMITTED, SUBMITTED, IN_REVIEW, APPROVED, REJECTED, RETURNED, NOT_REQUIRED.</summary>
    public string ApprovalStatus { get; set; } = "NOT_SUBMITTED";

    /// <summary>One of: NOT_POSTED, READY_TO_POST, POSTING, POSTED, PARTIALLY_POSTED, POSTING_ERROR, REVERSED.</summary>
    public string PostingStatus { get; set; } = "NOT_POSTED";

    public decimal ControlTotalDr { get; set; }

    public decimal ControlTotalCr { get; set; }

    public decimal ActualTotalDr { get; set; }

    public decimal ActualTotalCr { get; set; }

    public int JournalCount { get; set; }

    public string? SourceBatchRef { get; set; }

    public string? Description { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }
}
