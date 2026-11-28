using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// A member company's licence: one company administrator plus MaxUsers further users, the period it may use the service, and its
/// yearly inquiry quota. Every change is a new row (the newest is the one in force); earlier rows stay for billing and audit.
/// </summary>
public class ScrCompanyLicence : AuditableEntity
{
    public long LicenceId { get; set; }

    public long MemberCompanyId { get; set; }

    /// <summary>Users besides the company administrator; 0 = the administrator alone.</summary>
    public int MaxUsers { get; set; }

    public DateOnly ValidFrom { get; set; }

    /// <summary>Null = open-ended.</summary>
    public DateOnly? ValidTo { get; set; }

    /// <summary>Inquiries a year; null = no limit.</summary>
    public int? YearlyInquiryQuota { get; set; }

    public string? Notes { get; set; }

    public bool CoversDay(DateOnly day) => ValidFrom <= day && (ValidTo is null || day <= ValidTo);
}
