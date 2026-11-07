using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_REPAYMENT_REQUESTS: copy of FINOVA FNV_REPAYMENT_REQUESTS (APEX), all columns kept.
/// </summary>
public class FctRepaymentRequest : AuditableEntity
{
    public long RepaymentRequestId { get; set; }

    public long? ClientId { get; set; }

    public int? ChequesCount { get; set; }

    public decimal? ChequesAmount { get; set; }

    public decimal? RemainingAmount { get; set; }

    public string? StatusCode { get; set; }

    public long RepaymentTranNo { get; set; }

    /// <summary>One of: CHEQUE, BANK_TRANSFER, CREDIT_BALANCE.</summary>
    public string? Source { get; set; }

    public decimal? BankTransfer { get; set; }

    public decimal? CreditBalance { get; set; }
}
