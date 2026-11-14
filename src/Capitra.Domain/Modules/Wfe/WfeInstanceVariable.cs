using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_INSTANCE_VARIABLES: named values of an instance (entity id, amounts, comments) used by routes and pages.</summary>
public class WfeInstanceVariable : AuditableEntity
{
    public long InstanceVariableId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public string VariableName { get; set; } = string.Empty;

    public string? VariableType { get; set; }

    public string? VariableValue { get; set; }

    public WfeWorkflowInstance? Instance { get; set; }
}
