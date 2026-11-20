using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_DEPOSIT_BATCHES: copy of FINOVA FND_CHEQUE_DEPOSIT_BATCH (APEX), all columns kept.
/// </summary>
public class ChqDepositBatch : AuditableEntity
{
    public long DepositBatchId { get; set; }

    public string BatchRef { get; set; } = string.Empty;

    public long LegalEntityId { get; set; }

    public long DepositBankAccountId { get; set; }

    public DateOnly BatchDate { get; set; }

    public decimal? TotalCheques { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public int? CollectedCount { get; set; }

    public decimal? CollectedAmount { get; set; }

    public int? RejectedCount { get; set; }

    public decimal? RejectedAmount { get; set; }

    public int? WithdrawnCount { get; set; }

    public decimal? WithdrawnAmount { get; set; }

    public int? PendingCount { get; set; }

    public decimal? PendingAmount { get; set; }

    /// <summary>One of: PENDING, SUBMITTED, PARTIALLY_CLEARED, FULLY_CLEARED, CLOSED.</summary>
    public string? BatchStatus { get; set; } = "PENDING";

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ClearedAt { get; set; }

    public string? BankBatchRef { get; set; }

    public bool IsBankConfirmed { get; set; }

    public DateTime? BankConfirmedAt { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime? GlPostAt { get; set; }

    public string? Notes { get; set; }
}
