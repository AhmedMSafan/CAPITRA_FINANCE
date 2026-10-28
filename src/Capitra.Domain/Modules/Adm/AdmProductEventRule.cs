using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_PRODUCT_EVENT_RULES: the charges a product applies on an accounting event and their GL entries
/// (FINOVA_FINANCEING ADM_PRODUCT_EVENT_RULES).
/// </summary>
public class AdmProductEventRule : AuditableEntity
{
    public long ProductEventRuleId { get; set; }

    public long ProductId { get; set; }

    /// <summary>GL_ACCOUNTING_EVENT_TYPES.EVENT_TYPE_ID.</summary>
    public long EventTypeId { get; set; }

    /// <summary>WFE_WORKFLOW_DEFINITIONS.WORKFLOW_CODE of the workflow whose step this rule serves, e.g. PURCHASED_PORTFOLIO_WF.</summary>
    public string? WorkflowCode { get; set; }

    /// <summary>The step code in that workflow's BPMN diagram, e.g. DISBURSEMENT.</summary>
    public string? StepCode { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    public int PriorityNo { get; set; } = 10;

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<AdmProductEventRuleCharge> Charges { get; set; } = [];

    public List<AdmProductEventRuleGlLine> GlLines { get; set; } = [];
}
