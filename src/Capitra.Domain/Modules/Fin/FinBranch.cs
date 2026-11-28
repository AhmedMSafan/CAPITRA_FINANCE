using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fin;

/// <summary>
/// FIN_BRANCHES: copy of FINOVA FIN_BRANCHES (APEX), all columns kept.
/// </summary>
public class FinBranch : AuditableEntity
{
    public long BranchId { get; set; }

    public long LegalEntityId { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public string BranchNameEn { get; set; } = string.Empty;

    public string? BranchNameAr { get; set; }

    public long? ParentBranchId { get; set; }

    /// <summary>One of: HQ, REGION, BRANCH, SERVICE_CENTER, VIRTUAL.</summary>
    public string BranchType { get; set; } = "BRANCH";

    public string CountryCode { get; set; } = "EG";

    public string? CityName { get; set; }

    public string? AddressEn { get; set; }

    public string? AddressAr { get; set; }

    public string? PhoneNo { get; set; }

    public string? EmailAddress { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
