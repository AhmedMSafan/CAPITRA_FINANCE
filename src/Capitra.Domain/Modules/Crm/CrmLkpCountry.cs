using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_LKP_COUNTRIES: copy of FINOVA LKP_COUNTRIES (APEX), all columns kept.
/// </summary>
public class CrmLkpCountry : AuditableEntity
{
    public long CountryId { get; set; }

    public string CountryCode { get; set; } = string.Empty;

    public string CountryCodeA3 { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public string CountryNameAr { get; set; } = string.Empty;

    public string? Nationality { get; set; }

    public string? NationalityAr { get; set; }

    public string? PhoneCode { get; set; }

    public string? CurrencyCode { get; set; }

    public string? Continent { get; set; }

    public string? ContinentAr { get; set; }

    public bool IsDomestic { get; set; }

    public string? RiskRating { get; set; }

    public bool IsSanctioned { get; set; }

    public bool IsActive { get; set; } = true;

    public int? DisplayOrder { get; set; }
}
