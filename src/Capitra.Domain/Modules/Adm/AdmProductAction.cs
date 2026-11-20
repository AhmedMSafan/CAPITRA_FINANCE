using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_PRODUCT_ACTIONS: links an action of a product (a button outside the workflow, e.g. cheque batch Submit) to the
/// product event rule it posts. The actions themselves are a fixed list in the code (ProductActions); this table holds
/// only the rule the admin chose for each one.
/// </summary>
public class AdmProductAction : AuditableEntity
{
    public long ProductActionId { get; set; }

    public long ProductId { get; set; }

    /// <summary>One of the fixed action codes, e.g. CHW_BATCH_SUBMIT; an action is linked on one product only.</summary>
    public string ActionCode { get; set; } = string.Empty;

    /// <summary>The rule the action posts; null while none is chosen (the action is then blocked).</summary>
    public long? ProductEventRuleId { get; set; }

    public bool IsActive { get; set; } = true;
}
