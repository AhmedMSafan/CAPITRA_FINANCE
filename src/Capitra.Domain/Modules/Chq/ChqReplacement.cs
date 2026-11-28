using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_REPLACEMENTS: copy of FINOVA FND_CHEQUE_REPLACEMENT (APEX), all columns kept.
/// </summary>
public class ChqReplacement : AuditableEntity
{
    public long ReplacementId { get; set; }

    public string ReplacementRef { get; set; } = string.Empty;

    public long OriginalChequeId { get; set; }

    public long LegalEntityId { get; set; }

    /// <summary>One of: REJECTED, WITHDRAWN, EXPIRED.</summary>
    public string TriggerEvent { get; set; } = string.Empty;

    public DateTime TriggerEventAt { get; set; }

    /// <summary>One of: REPLACEMENT_CHEQUE, BANK_TRANSFER, INTERNAL_RESOURCES, PARTIAL_TRANSFER_CHEQUE.</summary>
    public string SettlementMethod { get; set; } = string.Empty;

    public decimal SettlementAmount { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal? OriginalChequeAmount { get; set; }

    public long? NewChequeId { get; set; }

    public long? TransferBankAccountId { get; set; }

    public string? TransferTxnRef { get; set; }

    public DateTime? TransferAt { get; set; }

    public string? InternalFundSource { get; set; }

    public string? InternalApprovalBy { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? SettledAt { get; set; }

    /// <summary>One of: PENDING, PARTIAL, SETTLED, CANCELLED, DEFAULTED.</summary>
    public string? ReplacementStatus { get; set; } = "PENDING";

    public long? PaymentId { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime? GlPostAt { get; set; }

    public string? Notes { get; set; }
}
