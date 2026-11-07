using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_EVENTS: copy of FINOVA FND_CHEQUE_EVENT (APEX), all columns kept.
/// </summary>
public class ChqEvent : AuditableEntity
{
    public long EventId { get; set; }

    public long ChequeId { get; set; }

    public long? DepositBatchId { get; set; }

    /// <summary>One of: RECEIVED, DEPOSITED, COLLECTED, REJECTED, WITHDRAWN, REPLACEMENT_ISSUED, SETTLED_TRANSFER, SETTLED_INTERNAL, CANCELLED, REDEPOSITED.</summary>
    public string EventType { get; set; } = string.Empty;

    public DateTime EventAt { get; set; }

    public DateTime? EffectiveAt { get; set; }

    public decimal? EventAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? RejectionReasonCode { get; set; }

    public string? BankRef { get; set; }

    public string? NewStatusCode { get; set; }

    public string? OldStatusCode { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime? GlPostAt { get; set; }

    public string? GlDebitAccount { get; set; }

    public string? GlCreditAccount { get; set; }

    public string? Notes { get; set; }

    public long? MovementGroupId { get; set; }

    public string? FromLocationCode { get; set; }

    public string? ToLocationCode { get; set; }

    public string? FromCustodianUser { get; set; }

    public string? ToCustodianUser { get; set; }

    public string? HandoverBy { get; set; }

    public string? ReceivedBy { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public string? MovementRef { get; set; }

    public long? SourcePageId { get; set; }

    public long? SourceAppId { get; set; }

    public string? SourceSessionId { get; set; }
}
