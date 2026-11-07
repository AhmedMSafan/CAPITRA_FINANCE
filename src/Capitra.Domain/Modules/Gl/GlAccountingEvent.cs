using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_EVENTS: copy of FINOVA GL_ACCOUNTING_EVENTS (APEX), all columns kept.
/// </summary>
public class GlAccountingEvent : AuditableEntity
{
    public long AccountingEventId { get; set; }

    public long LedgerId { get; set; }

    public long SourceId { get; set; }

    public long EventTypeId { get; set; }

    public string EventNo { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public DateOnly AccountingDate { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal EventAmount { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string SourceDocumentType { get; set; } = string.Empty;

    public string SourceDocumentId { get; set; } = string.Empty;

    public string? SourceDocumentNo { get; set; }

    public string? SourceTransactionId { get; set; }

    /// <summary>One of: NEW, VALIDATED, ACCOUNTED, REVERSED, CANCELLED, ERROR.</summary>
    public string EventStatus { get; set; } = "NEW";

    /// <summary>One of: NOT_GENERATED, DRAFT_ACCOUNTING, READY_FOR_REVIEW, APPROVED_FOR_POSTING, POSTED, REVERSED, ERROR.</summary>
    public string AccountingStatus { get; set; } = "NOT_GENERATED";

    public long? JournalHeaderId { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Description { get; set; }

    public long? SelectedRuleSetId { get; set; }

    public string? SourcePayloadJson { get; set; }
}
