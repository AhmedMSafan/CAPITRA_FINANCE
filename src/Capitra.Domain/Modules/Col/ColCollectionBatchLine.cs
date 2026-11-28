using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Col;

/// <summary>COL_COLLECTION_BATCH_LINES: one cheque of a collection batch, the bank's result on it and the repayment it made.</summary>
public class ColCollectionBatchLine : AuditableEntity
{
    public long CollectionBatchLineId { get; set; }

    public long CollectionBatchId { get; set; }

    public int LineNo { get; set; }

    public long ChequeId { get; set; }

    public string? ChequeRef { get; set; }

    public string ChequeNo { get; set; } = string.Empty;

    public DateOnly DueDate { get; set; }

    public string? CustomerId { get; set; }

    public long? DepositBankAccountId { get; set; }

    public decimal ChequeAmount { get; set; }

    public string? CurrencyCode { get; set; }

    /// <summary>COLLECTED, BOUNCED or REJECTED.</summary>
    public string ResultCode { get; set; } = string.Empty;

    public DateOnly StatusDate { get; set; }

    public string? BankStatementReference { get; set; }

    public string? ReasonCode { get; set; }

    public string? Notes { get; set; }

    /// <summary>The client repayment a collected cheque went to.</summary>
    public long? RepaymentTxnId { get; set; }
}
