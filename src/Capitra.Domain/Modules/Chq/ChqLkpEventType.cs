using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_LKP_EVENT_TYPES: copy of FINOVA LKP_CHEQUE_EVENT_TYPE (APEX), all columns kept.
/// </summary>
public class ChqLkpEventType : AuditableEntity
{
    public string EventType { get; set; } = string.Empty;

    public string EventDescEn { get; set; } = string.Empty;

    public string? EventDescAr { get; set; }
}
