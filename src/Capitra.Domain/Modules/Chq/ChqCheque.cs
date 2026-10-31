using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_CHEQUES: copy of FINOVA FND_CHEQUE (APEX), all columns kept.
/// </summary>
public class ChqCheque : AuditableEntity
{
    public long ChequeId { get; set; }

    public string ChequeRef { get; set; } = string.Empty;

    public string ChequeNo { get; set; } = string.Empty;

    public DateTime ChequeAt { get; set; }

    public DateOnly DueDate { get; set; }

    public string IssuerBankName { get; set; } = string.Empty;

    public string? IssuerBankCode { get; set; }

    public string? IssuerBankBranch { get; set; }

    public string? IssuerAccountNo { get; set; }

    public string DrawerNameEn { get; set; } = string.Empty;

    public string? DrawerNameAr { get; set; }

    public string? DrawerNationalId { get; set; }

    public string? CustomerId { get; set; }

    public string? CustomerNameEn { get; set; }

    public string? CustomerNameAr { get; set; }

    public long? DrawdownId { get; set; }

    public long? ScheduleId { get; set; }

    public long? DepositBankAccountId { get; set; }

    public decimal ChequeAmount { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>One of: LEASING, MORTGAGE, FACTORING, DIRECT, OTHER.</summary>
    public string? SourceModule { get; set; } = "LEASING";

    public string? SourceRefType { get; set; }

    public string? SourceRefId { get; set; }

    public DateTime ReceivedAt { get; set; }

    public DateTime? DepositAt { get; set; }

    public DateTime? CollectionAt { get; set; }

    public DateTime? RejectionAt { get; set; }

    public DateTime? WithdrawalAt { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string? RejectionReasonCode { get; set; }

    public string? RejectionNotes { get; set; }

    public bool IsReplaced { get; set; }

    public long? ReplacementChequeId { get; set; }

    public long? DepositBatchId { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public DateTime? GlPostAt { get; set; }

    public long? PaymentId { get; set; }

    public long LegalEntityId { get; set; }

    public string? Notes { get; set; }

    public string? DrawerAccountNo { get; set; }

    public string? PartyTaxId { get; set; }

    public string? CurrentLocationCode { get; set; }

    public string? CurrentCustodianUser { get; set; }

    public long? LastEventId { get; set; }

    public DateTime? LastEventAt { get; set; }

    public long? BatchId { get; set; }

    public long? DepositBankId { get; set; }
}
