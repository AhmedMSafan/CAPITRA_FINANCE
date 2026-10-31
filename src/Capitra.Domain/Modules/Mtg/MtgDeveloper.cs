using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_DEVELOPERS: copy of FINOVA FNV_REALESTATE_DEVELOPER (APEX), all columns kept.
/// </summary>
public class MtgDeveloper : AuditableEntity
{
    public long DeveloperId { get; set; }

    public string DeveloperCode { get; set; } = string.Empty;

    public string LegalNameAr { get; set; } = string.Empty;

    public string? LegalNameEn { get; set; }

    public string? CommercialRegNo { get; set; }

    public string? TaxRegNo { get; set; }

    public DateOnly? IncorporationDate { get; set; }

    public string? AddressText { get; set; }

    public string? PhoneNo { get; set; }

    public string? EmailAddress { get; set; }

    public string? RiskLevelCode { get; set; }

    public bool IsActive { get; set; } = true;
}
