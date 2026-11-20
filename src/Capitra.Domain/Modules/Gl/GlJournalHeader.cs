using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_HEADERS: copy of FINOVA GL_JOURNAL_HEADERS (APEX), all columns kept.
/// </summary>
public class GlJournalHeader : AuditableEntity
{
    public long JournalHeaderId { get; set; }

    public long BatchId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string JournalNo { get; set; } = string.Empty;

    public string JournalName { get; set; } = string.Empty;

    public long SourceId { get; set; }

    public long CategoryId { get; set; }

    public DateOnly JournalDate { get; set; }

    public DateOnly AccountingDate { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public long? RateTypeId { get; set; }

    public DateOnly? ConversionDate { get; set; }

    public decimal ConversionRate { get; set; } = 1m;

    /// <summary>One of: DRAFT, VALIDATED, SUBMITTED, APPROVED, POSTED, REVERSED, CANCELLED, ERROR.</summary>
    public string JournalStatus { get; set; } = "DRAFT";

    /// <summary>One of: NOT_SUBMITTED, SUBMITTED, IN_REVIEW, APPROVED, REJECTED, RETURNED, NOT_REQUIRED.</summary>
    public string ApprovalStatus { get; set; } = "NOT_SUBMITTED";

    /// <summary>One of: NOT_POSTED, READY_TO_POST, POSTING, POSTED, POSTING_ERROR, REVERSED.</summary>
    public string PostingStatus { get; set; } = "NOT_POSTED";

    public bool IsBalanced { get; set; }

    public bool IsSystemGenerated { get; set; }

    public bool IsReversal { get; set; }

    public string? ReversalMethod { get; set; }

    public long? ReversalPeriodId { get; set; }

    public DateOnly? ReversalDate { get; set; }

    public long? ReversedHeaderId { get; set; }

    public long? OriginalHeaderId { get; set; }

    public long? RecurringTemplateId { get; set; }

    public string? SourceModule { get; set; }

    public string? SourceDocumentType { get; set; }

    public string? SourceDocumentId { get; set; }

    public string? SourceDocumentNo { get; set; }

    public DateOnly? SourceTransactionDate { get; set; }

    public string? Description { get; set; }

    public string? ReferenceNo { get; set; }

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
