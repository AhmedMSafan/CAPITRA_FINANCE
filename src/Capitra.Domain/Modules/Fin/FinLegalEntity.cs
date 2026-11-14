using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fin;

/// <summary>
/// FIN_LEGAL_ENTITIES: copy of FINOVA FIN_LEGAL_ENTITIES (APEX), all columns kept.
/// </summary>
public class FinLegalEntity : AuditableEntity
{
    public long LegalEntityId { get; set; }

    public string LegalEntityCode { get; set; } = string.Empty;

    public string LegalEntityNameEn { get; set; } = string.Empty;

    public string? LegalEntityNameAr { get; set; }

    public string? ShortName { get; set; }

    public string CountryCode { get; set; } = "EG";

    public string BaseCurrencyCode { get; set; } = string.Empty;

    public string? TaxRegistrationNo { get; set; }

    public string? CommercialRegisterNo { get; set; }

    public string? RegulatorCode { get; set; }

    public string? LegalAddressEn { get; set; }

    public string? LegalAddressAr { get; set; }

    public string? PhoneNo { get; set; }

    public string? EmailAddress { get; set; }

    public string? WebsiteUrl { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>One of: DRAFT, ACTIVE, INACTIVE, SUSPENDED.</summary>
    public string StatusCode { get; set; } = "ACTIVE";

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }

    public long? ParentLegalEntityId { get; set; }

    public string LegalEntityType { get; set; } = "SUBSIDIARY";

    public bool IsConsolidation { get; set; } = true;

    public decimal OwnershipPercent { get; set; } = 100m;

    public string ConsolidationMethod { get; set; } = "FULL";
}
