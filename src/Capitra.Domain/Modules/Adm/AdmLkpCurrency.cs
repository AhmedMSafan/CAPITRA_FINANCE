using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_LKP_CURRENCY: currencies (APEX App 105 pages 37/38).</summary>
public class AdmLkpCurrency : LookupEntity
{
    /// <summary>Number of decimal places used when showing amounts in this currency.</summary>
    public int DecimalPlaces { get; set; } = 2;
}
