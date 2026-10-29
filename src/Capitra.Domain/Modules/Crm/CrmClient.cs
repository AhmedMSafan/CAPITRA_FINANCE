using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_CLIENTS: copy of FINOVA FCT_CLIENTS (APEX), all columns kept.
/// </summary>
public class CrmClient : AuditableEntity
{
    public long ClientId { get; set; }

    public string ClientNumber { get; set; } = string.Empty;

    public string? ClientRefCode { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string ClientNameAr { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string? ShortNameAr { get; set; }

    public string? TradeName { get; set; }

    public string? TradeNameAr { get; set; }

    public string ClientType { get; set; } = string.Empty;

    /// <summary>CLIENT_ROLE_CODE -> ADM_LKP_CLIENT_ROLES (added by FINOVA_FINANCEING); optional.</summary>
    public string? ClientRoleCode { get; set; }

    public string LegalStructure { get; set; } = string.Empty;

    public string? LegalStructureAr { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? TaxNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? NationalId { get; set; }

    public string? UnifiedNumber { get; set; }

    public DateOnly? DateOfIncorporationDate { get; set; }

    public DateOnly? DateOfRegistrationDate { get; set; }

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseExpiryDate { get; set; }

    public long CountryId { get; set; }

    public long? RegionId { get; set; }

    public long? CityId { get; set; }

    public long? IndustryId { get; set; }

    public long? SectorId { get; set; }

    public long BaseCurrencyId { get; set; }

    public long StatusId { get; set; }

    public string? ClientCategory { get; set; }

    public string? ClientSegment { get; set; }

    public string? RiskGrade { get; set; }

    public decimal? AnnualTurnover { get; set; }

    public long? NumberOfEmployees { get; set; }

    public decimal? CapitalAmount { get; set; }

    public string? Website { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MobileNumber { get; set; }

    public string? FaxNumber { get; set; }

    public string? RelationshipManager { get; set; }

    public long? RmEmployeeId { get; set; }

    public DateOnly? OnboardingDate { get; set; }

    public DateOnly? FirstFacilityDate { get; set; }

    public DateOnly? LastReviewDate { get; set; }

    public DateOnly? NextReviewDate { get; set; }

    public string? KycStatus { get; set; } = "PENDING";

    public DateOnly? KycExpiryDate { get; set; }

    public string? AmlStatus { get; set; } = "PENDING";

    public DateOnly? AmlCheckDate { get; set; }

    public bool IsBlacklisted { get; set; }

    public string? BlacklistReason { get; set; }

    public string? BlacklistReasonAr { get; set; }

    public bool IsRelatedParty { get; set; }

    public long? ParentClientId { get; set; }

    public string? GroupCode { get; set; }

    public string? GroupName { get; set; }

    public string? GroupNameAr { get; set; }

    public string? LogoUrl { get; set; }

    public string? Notes { get; set; }

    public string? NotesAr { get; set; }

    public string? Attribute1 { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public bool IsActive { get; set; } = true;

    public long? SupplierBankId { get; set; }

    public string? SupplierBankAccount { get; set; }

    public bool IsSupplier { get; set; }
}
