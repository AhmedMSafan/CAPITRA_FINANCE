using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_HEADERS: a journal. It moves DRAFT → SUBMITTED → APPROVED (by another user) → POSTED;
/// REJECTED goes back to editing, CANCELLED ends it before posting, and a posted journal is corrected by a reversal
/// (APEX App 120 pages 50 and 51, GL_POSTING_API_PKG).
/// </summary>
public class GlJournalHeader : AuditableEntity
{
    public long JournalHeaderId { get; set; }

    public long BatchId { get; set; }

    public long LedgerId { get; set; }

    /// <summary>The ledger period of the accounting date.</summary>
    public long LedgerPeriodId { get; set; }

    public string JournalNo { get; set; } = string.Empty;

    public string JournalName { get; set; } = string.Empty;

    public long SourceId { get; set; }

    public long CategoryId { get; set; }

    public DateOnly JournalDate { get; set; }

    public DateOnly AccountingDate { get; set; }

    /// <summary>The ledger currency.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>One of <see cref="GlCodes.JournalStatuses"/>.</summary>
    public string JournalStatus { get; set; } = GlCodes.JournalDraft;

    public decimal TotalDr { get; set; }

    public decimal TotalCr { get; set; }

    /// <summary>The journal reverses <see cref="ReversedHeaderId"/>; its lines cannot be changed.</summary>
    public bool IsReversal { get; set; }

    public long? ReversedHeaderId { get; set; }

    public string? Description { get; set; }

    public string? ReferenceNo { get; set; }

    public string? SourceDocumentType { get; set; }

    public string? SourceDocumentNo { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }

    public string? RejectReason { get; set; }

    /// <summary>Why the last posting attempt failed; cleared when the journal posts or is edited.</summary>
    public string? LastPostError { get; set; }
}
