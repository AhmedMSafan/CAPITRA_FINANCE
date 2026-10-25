using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_DEVELOPERS: real estate developer (APEX App 105 pages 101/102).</summary>
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

    /// <summary>ADM_LKP_RISK_LEVELS code.</summary>
    public string? RiskLevelCode { get; set; }

    public bool IsActive { get; set; } = true;
}
