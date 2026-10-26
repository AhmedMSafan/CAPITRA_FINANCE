using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_ORGANIZATIONS: copy of FINOVA MFF_ORGANIZATIONS (APEX), all columns kept.
/// </summary>
public class MffOrganization : AuditableEntity
{
    public long OrgId { get; set; }

    public string OrgCode { get; set; } = string.Empty;

    public string OrgNameAr { get; set; } = string.Empty;

    public string? LegacyNameAr { get; set; }

    public string? OrgNameEn { get; set; }

    public string OrgTypeCode { get; set; } = string.Empty;

    public string? RegulatorCode { get; set; }

    public string? LicenseNo { get; set; }

    public DateOnly? LicenseDate { get; set; }

    public string? BankCode { get; set; }

    public string? TaxId { get; set; }

    public string? AddressAr { get; set; }

    public string? PhoneNo { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;
}
