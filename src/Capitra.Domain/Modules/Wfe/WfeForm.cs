using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>
/// WFE_FORMS: a document drawn in the Document Designer (K2's SmartForms, kept simple). A document has its own fields
/// (FIELDS_JSON) and belongs to no workflow; a workflow links it on a step (finova:formCode) or, for a user-defined workflow,
/// on the whole workflow (finova:documentCode). Forms made before documents belong to one workflow and use its data fields.
/// </summary>
public class WfeForm : AuditableEntity
{
    public long FormId { get; set; }

    public string FormCode { get; set; } = string.Empty;

    public string FormName { get; set; } = string.Empty;

    /// <summary>Only for a form made before documents: the workflow whose data fields it shows. Null for a document.</summary>
    public long? WorkflowDefinitionId { get; set; }

    public WfeWorkflowDefinition? Definition { get; set; }

    public string? Description { get; set; }

    /// <summary>The document's own fields as JSON (name, label, type, required, choices, default, help); null on an older form.</summary>
    public string? FieldsJson { get; set; }

    /// <summary>The layout as JSON: columns and the ordered controls (see WfeForms).</summary>
    public string LayoutJson { get; set; } = "{}";

    public bool IsActive { get; set; } = true;
}
