using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_LEGAL_ENTITIES: companies that keep their own books (APEX FIN_LEGAL_ENTITIES, set up by script in App 120).
/// GL segment values and ledgers point to a legal entity by its code.
/// </summary>
public class AdmLkpLegalEntity : LookupEntity
{
    /// <summary>ADM_LKP_CURRENCIES code.</summary>
    public string? BaseCurrencyCode { get; set; }

    /// <summary>ADM_LKP_COUNTRIES code.</summary>
    public string? CountryCode { get; set; }
}
