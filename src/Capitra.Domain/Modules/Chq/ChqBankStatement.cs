using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_BANK_STATEMENTS: copy of FINOVA FND_CHEQUE_BANK_STATEMENT (APEX), all columns kept.
/// </summary>
public class ChqBankStatement : AuditableEntity
{
    public long StatementLineId { get; set; }

    public string StatementRef { get; set; } = string.Empty;

    public long BankAccountId { get; set; }

    public long LegalEntityId { get; set; }

    public DateOnly StatementDate { get; set; }

    public DateOnly? ValueDate { get; set; }

    /// <summary>One of: CHEQUE_CREDIT, CHEQUE_RETURN, TRANSFER_CREDIT, FEE_DEBIT, OTHER_DEBIT, OTHER_CREDIT.</summary>
    public string TransactionType { get; set; } = string.Empty;

    public decimal TransactionAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? BankReference { get; set; }

    public string? Description { get; set; }

    public long? MatchedChequeId { get; set; }

    /// <summary>One of: UNMATCHED, MATCHED, PARTIAL_MATCH, EXCEPTION, IGNORED.</summary>
    public string? ReconStatus { get; set; } = "UNMATCHED";

    public DateTime? ReconAt { get; set; }

    public string? ReconBy { get; set; }

    public string? ReconNotes { get; set; }

    public decimal? ClosingBalance { get; set; }
}
