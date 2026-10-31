using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_LKP_DECISION_TYPES: copy of FINOVA REF_DECISION_TYPE (APEX), all columns kept.
/// </summary>
public class AdmLkpDecisionType : AuditableEntity
{
    public string DecisionCode { get; set; } = string.Empty;

    public string DecisionNameAr { get; set; } = string.Empty;

    public string DecisionNameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
