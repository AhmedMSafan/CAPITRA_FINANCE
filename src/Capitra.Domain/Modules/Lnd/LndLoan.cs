using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Lnd;

/// <summary>
/// LND_LOANS: one retail loan (microfinance or consumer finance). It keeps a copy of the product version's terms it was made under
/// (design deck: later product versions never touch a live loan). STATUS_CODE changes only through LndLoanService, which checks the
/// transition and writes LND_LOAN_STATUS_HIST.
/// </summary>
public class LndLoan : AuditableEntity
{
    public long LoanId { get; set; }

    /// <summary>LN-yyyy-nnnnnnn.</summary>
    public string LoanNumber { get; set; } = string.Empty;

    public long ClientId { get; set; }

    public long ProductId { get; set; }

    /// <summary>The product version the terms below were copied from.</summary>
    public long ProductVersionId { get; set; }

    /// <summary>MF or CF: the licence, and the scope of the Lending Parameters that apply.</summary>
    public string LicenceCode { get; set; } = string.Empty;

    /// <summary>One of: APPROVED, ACTIVE, CLOSED, CANCELLED (more states come with servicing).</summary>
    public string StatusCode { get; set; } = "APPROVED";

    /// <summary>Interest goes to suspense instead of income (set by end of day from the non-accrual days).</summary>
    public bool IsNonAccrual { get; set; }

    /// <summary>Days the oldest unpaid instalment is late, as of LAST_EOD_DATE (set by end of day).</summary>
    public int DaysLate { get; set; }

    /// <summary>Interest taken to suspense while the loan is in non-accrual, released to income when it cures.</summary>
    public decimal InterestSuspenseAmount { get; set; }

    /// <summary>Late fees taken to suspense while the loan is in non-accrual.</summary>
    public decimal LateFeeSuspenseAmount { get; set; }

    /// <summary>End of day found the loan at or beyond the write-off days: it waits for a write-off decision.</summary>
    public bool IsWriteOffProposed { get; set; }

    /// <summary>The last business day end of day processed this loan.</summary>
    public DateOnly? LastEodDate { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public decimal PrincipalAmount { get; set; }

    public int TermMonths { get; set; }

    public int InstallmentCount { get; set; }

    public decimal InstallmentAmount { get; set; }

    public decimal TotalInterestAmount { get; set; }

    public DateOnly DisbursementDate { get; set; }

    public DateOnly FirstDueDate { get; set; }

    public DateOnly MaturityDate { get; set; }

    // ---- terms copied from the product version
    public string RepaymentFrequencyCode { get; set; } = string.Empty;

    public string InterestMethodCode { get; set; } = string.Empty;

    public decimal AnnualRatePct { get; set; }

    public string DayCountCode { get; set; } = string.Empty;

    public int LateFeeGraceDays { get; set; }

    public string LateFeeTypeCode { get; set; } = string.Empty;

    public decimal? LateFeeRatePct { get; set; }

    public decimal? LateFeeFlatAmount { get; set; }

    public string LateFeeBaseCode { get; set; } = string.Empty;

    public string PaymentOrder { get; set; } = string.Empty;

    public bool IsInsuranceRequired { get; set; }

    /// <summary>Copied from the product version: the customer may pay principal before it falls due (part prepayment or full early settlement).</summary>
    public bool IsPrepaymentAllowed { get; set; } = true;

    /// <summary>After a part prepayment: REDUCE_INSTALMENT (same number of instalments, smaller) or REDUCE_TERM (same instalment, fewer).</summary>
    public string PrepaymentRecalcCode { get; set; } = "REDUCE_INSTALMENT";

    /// <summary>Fee on the principal paid early by a part prepayment, in % (0 for none).</summary>
    public decimal PrepaymentFeePct { get; set; }

    /// <summary>Fee on the principal settled by a full early settlement, in % (0 for none).</summary>
    public decimal EarlySettlementFeePct { get; set; }

    public string? Notes { get; set; }
}

/// <summary>LND_INSTALLMENTS: one instalment of a loan's schedule, with what is due and what is paid of each part.</summary>
public class LndInstallment : AuditableEntity
{
    public long InstallmentId { get; set; }

    public long LoanId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal OpeningPrincipalAmount { get; set; }

    public decimal PrincipalDueAmount { get; set; }

    public decimal InterestDueAmount { get; set; }

    public decimal ChargesDueAmount { get; set; }

    public decimal InsuranceDueAmount { get; set; }

    public decimal LateFeeDueAmount { get; set; }

    public decimal PrincipalPaidAmount { get; set; }

    public decimal InterestPaidAmount { get; set; }

    public decimal ChargesPaidAmount { get; set; }

    public decimal InsurancePaidAmount { get; set; }

    public decimal LateFeePaidAmount { get; set; }

    /// <summary>Interest of this instalment accrued so far by end of day; it reaches INTEREST_DUE_AMOUNT on the due date.</summary>
    public decimal InterestAccruedAmount { get; set; }

    /// <summary>Days of late fee charged so far (a fee charged once counts the days late when it was charged).</summary>
    public int LateFeeDays { get; set; }

    public decimal ClosingPrincipalAmount { get; set; }

    /// <summary>One of: DUE, PART_PAID, PAID.</summary>
    public string StatusCode { get; set; } = "DUE";

    public DateOnly? PaidDate { get; set; }
}

/// <summary>LND_LOAN_STATUS_HIST: every change of a loan's status, with who made it and why.</summary>
public class LndLoanStatusHist : AuditableEntity
{
    public long LoanStatusHistId { get; set; }

    public long LoanId { get; set; }

    public string? FromStatusCode { get; set; }

    public string ToStatusCode { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
