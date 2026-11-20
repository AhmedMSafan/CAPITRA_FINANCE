using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_RATE_INDEXES: copy of FINOVA FNV_RATE_INDEXES (APEX), all columns kept.
/// </summary>
public class AdmRateIndex : AuditableEntity
{
    public long RateIndexId { get; set; }

    public string RateIndexCode { get; set; } = string.Empty;

    public string RateIndexName { get; set; } = string.Empty;

    public long? CurrencyId { get; set; }

    public bool IsActive { get; set; } = true;
}
