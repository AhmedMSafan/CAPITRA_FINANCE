using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Lnd;

/// <summary>
/// LND_WRITE_OFFS: a request to write off a retail loan, and its decision (four eyes: never one's own request). The amounts are worked
/// out again on approval, since the customer may have paid in between; on approval the loan becomes WRITTEN_OFF and GL posts the
/// product's rule LND_WRITE_OFF.
/// </summary>
public class LndWriteOff : AuditableEntity
{
    public long WriteOffId { get; set; }

    public long LoanId { get; set; }

    /// <summary>PENDING, APPROVED, REJECTED, or WITHDRAWN by the user who asked.</summary>
    public string StatusCode { get; set; } = "PENDING";

    public string Reason { get; set; } = string.Empty;

    /// <summary>The loan's days late when asked, and on approval.</summary>
    public int DaysLate { get; set; }

    /// <summary>Principal still owed, all instalments.</summary>
    public decimal PrincipalAmount { get; set; }

    /// <summary>Interest accrued and not paid that sits in income (accrued before non-accrual).</summary>
    public decimal InterestAmount { get; set; }

    /// <summary>Interest accrued and not paid that sits in suspense.</summary>
    public decimal InterestSuspenseAmount { get; set; }

    public decimal LateFeeAmount { get; set; }

    public decimal LateFeeSuspenseAmount { get; set; }

    /// <summary>Charges and insurance of instalments due by the day and not paid.</summary>
    public decimal ChargesAmount { get; set; }

    public decimal InsuranceAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? DecisionNotes { get; set; }

    public DateOnly? WriteOffDate { get; set; }

    public string? GlTranSetId { get; set; }
}
