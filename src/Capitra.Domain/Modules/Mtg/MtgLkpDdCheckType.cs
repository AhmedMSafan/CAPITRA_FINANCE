using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_LKP_DD_CHECK_TYPES: copy of FINOVA REF_DD_CHECK_TYPE (APEX), all columns kept.
/// </summary>
public class MtgLkpDdCheckType : AuditableEntity
{
    public string CheckTypeCode { get; set; } = string.Empty;

    public string CheckNameAr { get; set; } = string.Empty;

    public string CheckNameEn { get; set; } = string.Empty;

    public bool IsMandatory { get; set; } = true;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
