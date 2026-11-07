using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPLICATION_STATUS_HIST: copy of FINOVA MFF_APPLICATION_STATUS_HIST (APEX), all columns kept.
/// </summary>
public class MffApplicationStatusHist : AuditableEntity
{
    public long StatusHistId { get; set; }

    public long ApplicationId { get; set; }

    public string? OldStatusCode { get; set; }

    public string NewStatusCode { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public string? RoleCode { get; set; }

    public string? RemarksAr { get; set; }
}
