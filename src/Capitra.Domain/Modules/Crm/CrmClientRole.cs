using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_CLIENT_ROLES: the parts a customer plays (Customer 360, agreed with Ahmed 4 Nov 2026). A customer can hold several roles, e.g.
/// supplier in one program and debtor in another. A role starts ONBOARDING and becomes ACTIVE only when its checks pass.
/// </summary>
public class CrmClientRole : AuditableEntity
{
    public const string Onboarding = "ONBOARDING";
    public const string Active = "ACTIVE";
    public const string Suspended = "SUSPENDED";

    public long ClientRoleId { get; set; }

    public long ClientId { get; set; }

    /// <summary>CLIENT, DEBTOR, SUPPLIER, GUARANTOR, REAL_ESTATE_DEVELOPER or INVESTOR.</summary>
    public string RoleCode { get; set; } = string.Empty;

    public string StatusCode { get; set; } = Onboarding;

    /// <summary>
    /// For a SUPPLIER role, the business line it supplies (Ahmed, 25 Nov 2026): FACTORING or CONSUMER_FINANCE. A company can be a
    /// supplier in both, as two roles. A Consumer Finance supplier becomes ACTIVE only through its approval workflow
    /// (CF_SUPPLIER_APPROVAL); a Factoring supplier is activated as before. Null for the other roles, and for suppliers added before.
    /// </summary>
    public string? BusinessLineCode { get; set; }

    /// <summary>The day the role became active.</summary>
    public DateOnly? ActiveSinceDate { get; set; }

    public string? Notes { get; set; }
}
