using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// SCR_MEMBER_COMPANIES: a member company of the federation that screens people and companies against the Federation Final List.
/// Its staff sign in with ordinary CAPITRA accounts linked to it (SEC_USERS.MEMBER_COMPANY_ID) and see only their company's inquiries.
/// </summary>
public class ScrMemberCompany : AuditableEntity
{
    public long MemberCompanyId { get; set; }

    /// <summary>Short unique code, e.g. COMPANY-A.</summary>
    public string CompanyCode { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    /// <summary>The regulator's licence number of the company.</summary>
    public string? LicenceNo { get; set; }

    public string? ContactEmail { get; set; }

    /// <summary>One of: ACTIVE, SUSPENDED. A suspended company's users cannot screen.</summary>
    public string StatusCode { get; set; } = "ACTIVE";

    public string? Notes { get; set; }

    /// <summary>The price plan the company is billed by (user fee per month, fee per inquiry); empty = not billed yet.</summary>
    public long? PricePlanId { get; set; }

    /// <summary>How often the company is invoiced: MONTHLY, QUARTERLY or YEARLY (calendar periods).</summary>
    public string BillingCycleCode { get; set; } = "MONTHLY";
}
