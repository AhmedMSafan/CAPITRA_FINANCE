using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fin;

/// <summary>
/// FIN_CURRENCIES: copy of FINOVA FIN_CURRENCIES (APEX), all columns kept.
/// </summary>
public class FinCurrency : AuditableEntity
{
    public string CurrencyCode { get; set; } = string.Empty;

    public string CurrencyNameEn { get; set; } = string.Empty;

    public string? CurrencyNameAr { get; set; }

    public string? Symbol { get; set; }

    public int PrecisionDigits { get; set; } = 2;

    public bool IsActive { get; set; } = true;

    public long? CurrencyId { get; set; }
}
