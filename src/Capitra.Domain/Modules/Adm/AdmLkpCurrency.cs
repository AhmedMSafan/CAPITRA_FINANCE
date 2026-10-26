using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_CURRENCIES: copy of FINOVA REF_CURRENCY (APEX), all columns kept.
/// </summary>
public class AdmLkpCurrency : AuditableEntity
{
    public string CurrencyCode { get; set; } = string.Empty;

    public string CurrencyNameEn { get; set; } = string.Empty;

    public string CurrencyNameAr { get; set; } = string.Empty;

    public int DecimalPlaces { get; set; } = 2;

    public bool IsActive { get; set; } = true;
}
