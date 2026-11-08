using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_FORMS: a form drawn in the form designer (K2's SmartForms, kept simple) for one workflow. Its controls show and edit the
/// workflow's data fields. A step or the start of the workflow uses it through finova:formCode; steps with a business screen keep it.
/// </summary>
public class WfeForm : AuditableEntity
{
    public long FormId { get; set; }

    public string FormCode { get; set; } = string.Empty;

    public string FormName { get; set; } = string.Empty;

    public long WorkflowDefinitionId { get; set; }

    public WfeWorkflowDefinition? Definition { get; set; }

    public string? Description { get; set; }

    /// <summary>The layout as JSON: columns and the ordered controls (see WfeForms).</summary>
    public string LayoutJson { get; set; } = "{}";

    public bool IsActive { get; set; } = true;
}
