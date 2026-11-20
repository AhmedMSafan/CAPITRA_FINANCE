using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// SCR_PRICE_PLANS: what a member company pays for the screening service, in EGP: a fee per user per month (the administrator
/// counts as a user) and a fee per inquiry (whatever its result). Invoices copy the prices, so changing a plan changes only the
/// invoices issued after it.
/// </summary>
public class ScrPricePlan : AuditableEntity
{
    public long PricePlanId { get; set; }

    public string PlanCode { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    /// <summary>Fee per user per month: a user active on any day of a month is billed for that month.</summary>
    public decimal UserMonthlyFee { get; set; }

    /// <summary>Fee per screening inquiry.</summary>
    public decimal InquiryFee { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}

/// <summary>
/// SCR_USER_ACTIVITY: the days each member-company account could use the service: from the day it was activated (or reactivated)
/// to the day it was deactivated, or still open. Written automatically whenever an account changes; it is what the user fee is
/// billed from, also for past months.
/// </summary>
public class ScrUserActivity : AuditableEntity
{
    public long UserActivityId { get; set; }

    public long MemberCompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    /// <summary>The user name at the time, so the history reads even if the account is renamed.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>First active day (Cairo date).</summary>
    public DateOnly ActiveFrom { get; set; }

    /// <summary>Last active day (Cairo date); empty while still active.</summary>
    public DateOnly? ActiveTo { get; set; }
}

/// <summary>
/// SCR_INVOICES: what a member company is billed for one period: its users' months and its inquiries, at the prices copied from
/// its price plan when the invoice was issued. Issued invoices never change; a wrong one is cancelled (with a reason, kept) and
/// issued again. Two invoices of a company that are not cancelled never cover the same day.
/// </summary>
public class ScrInvoice : AuditableEntity
{
    public long InvoiceId { get; set; }

    /// <summary>INV-2026-000001: per year, in order.</summary>
    public string InvoiceNo { get; set; } = string.Empty;

    public long MemberCompanyId { get; set; }

    public DateOnly PeriodFrom { get; set; }

    public DateOnly PeriodTo { get; set; }

    public long PricePlanId { get; set; }

    public string PlanCode { get; set; } = string.Empty;

    public decimal UserMonthlyFee { get; set; }

    public decimal InquiryFee { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    /// <summary>The users' billed months, added up (3 users × 2 months = 6).</summary>
    public int UserMonths { get; set; }

    public decimal UserAmount { get; set; }

    public int InquiryCount { get; set; }

    public decimal InquiryAmount { get; set; }

    public decimal TotalAmount { get; set; }

    /// <summary>ISSUED or CANCELLED.</summary>
    public string StatusCode { get; set; } = "ISSUED";

    public DateTime IssuedAt { get; set; }

    public string IssuedBy { get; set; } = string.Empty;

    public DateTime? CancelledAt { get; set; }

    public string? CancelledBy { get; set; }

    public string? CancelReason { get; set; }

    public string? Notes { get; set; }

    public List<ScrInvoiceLine> Lines { get; set; } = [];
}

/// <summary>SCR_INVOICE_LINES: one line per user: the months billed and the inquiries they made, at the invoice's prices.</summary>
public class ScrInvoiceLine : AuditableEntity
{
    public long InvoiceLineId { get; set; }

    public long InvoiceId { get; set; }

    public int LineNo { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? FullName { get; set; }

    /// <summary>The months of the period the user was active on at least one day, e.g. "2026-10, 2026-11".</summary>
    public string? MonthsText { get; set; }

    public int UserMonths { get; set; }

    public decimal UserAmount { get; set; }

    public int InquiryCount { get; set; }

    public decimal InquiryAmount { get; set; }

    public decimal LineAmount { get; set; }
}
