using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_RATE_INDEX_VALUES: copy of FINOVA FNV_RATE_INDEX_VALUES (APEX), all columns kept.
/// </summary>
public class AdmRateIndexValue : AuditableEntity
{
    public long RateValueId { get; set; }

    public long RateIndexId { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public decimal AnnualRatePct { get; set; }
}
