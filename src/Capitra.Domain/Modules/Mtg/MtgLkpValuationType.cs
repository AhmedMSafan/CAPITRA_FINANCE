using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_VALUATION_TYPES: copy of FINOVA REF_VALUATION_TYPE (APEX), all columns kept.
/// </summary>
public class MtgLkpValuationType : AuditableEntity
{
    public string ValuationTypeCode { get; set; } = string.Empty;

    public string ValuationNameAr { get; set; } = string.Empty;

    public string ValuationNameEn { get; set; } = string.Empty;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
