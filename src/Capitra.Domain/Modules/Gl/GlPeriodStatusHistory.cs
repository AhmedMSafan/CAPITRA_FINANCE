using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_STATUS_HIST: copy of FINOVA GL_PERIOD_STATUS_HISTORY (APEX), all columns kept.
/// </summary>
public class GlPeriodStatusHistory : AuditableEntity
{
    public long PeriodHistoryId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string? OldPeriodStatus { get; set; }

    /// <summary>One of: NEVER_OPENED, FUTURE_ENTRY, OPEN, CLOSED, PERMANENTLY_CLOSED.</summary>
    public string NewPeriodStatus { get; set; } = string.Empty;

    /// <summary>One of: CREATE, SET_FUTURE_ENTRY, OPEN, CLOSE, REOPEN, PERMANENT_CLOSE, SYNC, SYSTEM_ADJUST.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? ActionReason { get; set; }

    public string ActionBy { get; set; } = string.Empty;

    public DateTime ActionAt { get; set; }

    public long? SourceAppId { get; set; }

    public long? SourcePageId { get; set; }

    public string? SourceSessionId { get; set; }
}
