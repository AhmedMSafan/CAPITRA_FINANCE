using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Wfe;

/// <summary>WFE_INSTANCE_HIST: what happened to an instance, in order.</summary>
public class WfeInstanceHist : AuditableEntity
{
    public long InstanceHistId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string? ElementId { get; set; }

    public string? ActionCode { get; set; }

    public string? PerformedBy { get; set; }

    public string? Details { get; set; }

    public DateTime EventAt { get; set; }
}
