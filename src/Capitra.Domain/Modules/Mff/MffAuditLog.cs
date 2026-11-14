using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_AUDIT_LOGS: copy of FINOVA MFF_AUDIT_LOG (APEX), all columns kept.
/// </summary>
public class MffAuditLog : AuditableEntity
{
    public long AuditId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public string RecordKey { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public string? OldDataJson { get; set; }

    public string? NewDataJson { get; set; }

    public DateTime ActionAt { get; set; }

    public string ActionBy { get; set; } = string.Empty;

    public long? AppId { get; set; }

    public long? PageId { get; set; }

    public string? SessionId { get; set; }

    public string? ClientIp { get; set; }
}
