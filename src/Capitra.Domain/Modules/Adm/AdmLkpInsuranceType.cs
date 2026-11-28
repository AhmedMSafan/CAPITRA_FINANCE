using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_INSURANCE_TYPES: copy of FINOVA REF_INSURANCE_TYPE (APEX), all columns kept.
/// </summary>
public class AdmLkpInsuranceType : AuditableEntity
{
    public string InsuranceTypeCode { get; set; } = string.Empty;

    public string InsuranceNameAr { get; set; } = string.Empty;

    public string InsuranceNameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
