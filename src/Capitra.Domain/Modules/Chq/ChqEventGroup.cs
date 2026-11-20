using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_EVENT_GROUPS: copy of FINOVA FND_CHEQUE_EVENT_GROUP (APEX), all columns kept.
/// </summary>
public class ChqEventGroup : AuditableEntity
{
    public long MovementGroupId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime EventAt { get; set; }

    public DateTime? EffectiveAt { get; set; }

    public long? DepositBatchId { get; set; }

    public string? ToLocationCode { get; set; }

    public string? MovementRef { get; set; }

    public string? Notes { get; set; }
}
