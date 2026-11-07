using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_LKP_CURRENCIES: copy of FINOVA LKP_CURRENCIES (APEX), all columns kept.
/// </summary>
public class CrmLkpCurrency : AuditableEntity
{
    public long CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string CurrencyName { get; set; } = string.Empty;

    public string CurrencyNameAr { get; set; } = string.Empty;

    public string? CurrencySymbol { get; set; }

    public string? NumericCode { get; set; }

    public int MinorUnit { get; set; } = 2;

    public long? CountryId { get; set; }

    public bool IsBaseCurrency { get; set; }

    public bool IsActive { get; set; } = true;

    public int? DisplayOrder { get; set; }
}
