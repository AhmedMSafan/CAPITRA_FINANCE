using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_GUARANTEE_TYPES: copy of FINOVA REF_GUARANTEE_TYPE (APEX), all columns kept.
/// </summary>
public class AdmLkpGuaranteeType : AuditableEntity
{
    public string GuaranteeTypeCode { get; set; } = string.Empty;

    public string GuaranteeNameAr { get; set; } = string.Empty;

    public string GuaranteeNameEn { get; set; } = string.Empty;

    public bool IsMandatory { get; set; } = true;

    public int? DisplaySeq { get; set; }

    public bool IsActive { get; set; } = true;
}
