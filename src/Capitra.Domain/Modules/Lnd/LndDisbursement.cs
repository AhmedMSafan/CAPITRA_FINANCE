using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Lnd;

/// <summary>
/// LND_DISBURSEMENTS: paying a loan out. A user asks for it (REQUESTED) with the account we pay from and the payee; another user
/// pays it (PAID): the charges are worked out, GL posts the rule LND_DISBURSE of the loan's product, and the loan becomes ACTIVE.
/// It can be REJECTED by the payer or WITHDRAWN by the requester.
/// </summary>
public class LndDisbursement : AuditableEntity
{
    public long DisbursementId { get; set; }

    public long LoanId { get; set; }

    /// <summary>One of: REQUESTED, PAID, REJECTED, WITHDRAWN.</summary>
    public string StatusCode { get; set; } = "REQUESTED";

    /// <summary>TRS_BANK_ACCOUNTS: our account the money is paid from (its GL account is the credit of the payout).</summary>
    public long PaidFromAccountId { get; set; }

    /// <summary>One of: BANK_TRANSFER, CASH, MERCHANT, WALLET.</summary>
    public string PayoutMethodCode { get; set; } = "BANK_TRANSFER";

    public string PayeeName { get; set; } = string.Empty;

    /// <summary>The customer's or merchant's account, IBAN or wallet number.</summary>
    public string? PayeeAccount { get; set; }

    public decimal PrincipalAmount { get; set; }

    /// <summary>The charges taken from the payout (the rule's charges, deducted from the disbursement).</summary>
    public decimal ChargesAmount { get; set; }

    public decimal NetPayoutAmount { get; set; }

    /// <summary>The day it was paid (the GL accounting date); empty until paid.</summary>
    public DateOnly? PaidDate { get; set; }

    /// <summary>The GL transaction set of the payout, CAP-LND-...</summary>
    public string? GlTranSetId { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? DecisionNotes { get; set; }
}

/// <summary>LND_DISBURSEMENT_CHARGES: each charge taken from a payout, and how its income is recognised (later, by end of day, when pro-rated).</summary>
public class LndDisbursementCharge : AuditableEntity
{
    public long DisbursementChargeId { get; set; }

    public long DisbursementId { get; set; }

    public string ChargeCode { get; set; } = string.Empty;

    public string ChargeName { get; set; } = string.Empty;

    public decimal ChargeAmount { get; set; }

    /// <summary>PRORATE: deferred and earned over the loan; IMMEDIATE: income on the payout date.</summary>
    public string IncomeRecognitionCode { get; set; } = string.Empty;
}

/// <summary>
/// LND_LOAN_EVENTS: the permanent record of what happened to a loan and its GL (design deck: the sub-ledger behind GL, I-Score and
/// audit). Every money event of a loan writes one row with the transaction set it posted.
/// </summary>
public class LndLoanEvent : AuditableEntity
{
    public long LoanEventId { get; set; }

    public long LoanId { get; set; }

    /// <summary>DISBURSED today; ACCRUED, RECEIPT, LATE_FEE, NON_ACCRUAL, WRITTEN_OFF... with the next steps.</summary>
    public string EventCode { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public decimal EventAmount { get; set; }

    public string? GlTranSetId { get; set; }

    /// <summary>The document behind the event, e.g. DISBURSEMENT 12.</summary>
    public string? SourceDocumentType { get; set; }

    public long? SourceDocumentId { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// LND_ALLOCATIONS: one part of one instalment paid by a receipt of the shared Collection module (COL_REPAYMENT_TRANSACTIONS).
/// Each business line keeps its own allocations; this is Retail Lending's (factoring: FCT_REPAYMENT_ALLOCATIONS).
/// </summary>
public class LndAllocation : AuditableEntity
{
    public long AllocationId { get; set; }

    public long RepaymentTxnId { get; set; }

    public long LoanId { get; set; }

    public long InstallmentId { get; set; }

    /// <summary>One of: LATE_FEE, CHARGES, INSURANCE, INTEREST, PRINCIPAL.</summary>
    public string PartCode { get; set; } = string.Empty;

    public decimal AllocatedAmount { get; set; }

    public DateOnly ValueDate { get; set; }

    /// <summary>ACTIVE, or REVERSED once the receipt is reversed.</summary>
    public string StatusCode { get; set; } = "ACTIVE";
}

/// <summary>
/// LND_EOD_RUNS: one end of day of Retail Lending for one business day (design deck: the daily batch). Days run in order; a failed
/// day can be run again. Its totals and GL transaction sets are kept for reconciliation.
/// </summary>
public class LndEodRun : AuditableEntity
{
    public long EodRunId { get; set; }

    public DateOnly RunDate { get; set; }

    /// <summary>RUNNING, DONE or FAILED.</summary>
    public string StatusCode { get; set; } = "RUNNING";

    public int LoansCount { get; set; }

    public decimal CreditAppliedAmount { get; set; }

    public decimal InterestAccruedAmount { get; set; }

    public decimal InterestSuspendedAmount { get; set; }

    public decimal LateFeeAmount { get; set; }

    public int NonAccrualCount { get; set; }

    public int CuredCount { get; set; }

    public int WriteOffProposedCount { get; set; }

    public string? GlTranSetIds { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }
}
