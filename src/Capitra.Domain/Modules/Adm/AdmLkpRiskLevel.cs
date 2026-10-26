using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_RISK_LEVELS: copy of FINOVA REF_RISK_LEVEL (APEX), all columns kept.
/// </summary>
public class AdmLkpRiskLevel : AuditableEntity
{
    public string RiskLevelCode { get; set; } = string.Empty;

    public string RiskLevelNameAr { get; set; } = string.Empty;

    public string RiskLevelNameEn { get; set; } = string.Empty;

    public int? SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
