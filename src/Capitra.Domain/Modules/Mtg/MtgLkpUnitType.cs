using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_UNIT_TYPES: copy of FINOVA REF_UNIT_TYPE (APEX), all columns kept.
/// </summary>
public class MtgLkpUnitType : AuditableEntity
{
    public string UnitTypeCode { get; set; } = string.Empty;

    public string UnitTypeNameAr { get; set; } = string.Empty;

    public string UnitTypeNameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
