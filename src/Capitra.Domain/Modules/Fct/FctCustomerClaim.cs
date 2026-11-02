using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_CUSTOMER_CLAIMS: the claim of a disbursement request's charges on the customer (Ahmed, 2 Nov 2026). Create Claim on the Invoice
/// Upload step makes it; Collection pays it from the customer's payment sources, in one or more payments. Complete of the step needs it PAID.
/// </summary>
public class FctCustomerClaim : AuditableEntity
{
    public const string Open = "OPEN";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";

    public long ClaimId { get; set; }

    /// <summary>CLM-{yyyyMM}-{CLAIM_ID padded to 8}.</summary>
    public string ClaimNumber { get; set; } = string.Empty;

    public long ClientId { get; set; }

    public long DisbursementId { get; set; }

    public long? ProductId { get; set; }

    /// <summary>The product event rule of the charges; its GL lines post the payments.</summary>
    public long? ProductEventRuleId { get; set; }

    public long? CurrencyId { get; set; }

    public DateOnly ClaimDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    /// <summary>OPEN, PARTIALLY_PAID, PAID or CANCELLED.</summary>
    public string StatusCode { get; set; } = Open;

    public DateTime? PaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelledBy { get; set; }
}
