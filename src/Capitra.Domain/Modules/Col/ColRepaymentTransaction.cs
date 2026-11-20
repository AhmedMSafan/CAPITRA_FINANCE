using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Col;

/// <summary>
/// COL_REPAYMENT_TRANSACTIONS: copy of FINOVA FNV_REPAYMENT_TRANSACTIONS (APEX), all columns kept. The Collection module is shared by
/// every business line (Ahmed, 11 Nov 2026): BUSINESS_LINE_CODE says whose receipt it is, and each business line keeps its own
/// allocations (factoring: FCT_REPAYMENT_ALLOCATIONS).
/// </summary>
public class ColRepaymentTransaction : AuditableEntity
{
    public long RepaymentTxnId { get; set; }

    /// <summary>The business line the receipt belongs to (ADM_LKP_BUSINESS_LINES): FACTORING today, then the lending lines.</summary>
    public string BusinessLineCode { get; set; } = BusinessLines.Factoring;

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

    /// <summary>The GL transaction set of the collection (DR the deposit bank account / CR customer liability), CAP-COL-...</summary>
    public string? GlTranSetId { get; set; }
}
