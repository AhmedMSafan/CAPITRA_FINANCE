using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_REPAYMENT_TRANSACTIONS: copy of FINOVA FNV_REPAYMENT_TRANSACTIONS (APEX), all columns kept.
/// </summary>
public class FctRepaymentTransaction : AuditableEntity
{
    public long RepaymentTxnId { get; set; }

    public string RepaymentNumber { get; set; } = string.Empty;

    public long ClientId { get; set; }

    public long LegalEntityId { get; set; }

    public long? AgreementId { get; set; }

    /// <summary>One of: CHEQUE, BANK_TRANSFER, CREDIT_BALANCE.</summary>
    public string RepaymentMethod { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public DateOnly ValueDate { get; set; }

    public long? CurrencyId { get; set; }

    public string? CurrencyCode { get; set; }

    public decimal ReceiptAmount { get; set; }

    public decimal AllocatedAmount { get; set; }

    public long? SourceGlAccountId { get; set; }

    public string? BankStatementReference { get; set; }

    public string? ExternalReference { get; set; }

    /// <summary>One of: DRAFT, CONFIRMED, PARTIALLY_ALLOCATED, ALLOCATED, REVERSED, CANCELLED.</summary>
    public string StatusCode { get; set; } = "DRAFT";

    public long? AccountingEventId { get; set; }

    public bool IsGlPosted { get; set; }

    public string? GlJournalRef { get; set; }

    public string? ReversalReason { get; set; }

    public string? ReversedBy { get; set; }

    public DateTime? ReversedAt { get; set; }

    public string? Notes { get; set; }
}
