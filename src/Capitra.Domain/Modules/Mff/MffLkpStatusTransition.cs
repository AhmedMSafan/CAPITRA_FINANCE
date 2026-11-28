using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_STATUS_TRANSITIONS: copy of FINOVA REF_MFF_STATUS_TRANSITION (APEX), all columns kept.
/// </summary>
public class MffLkpStatusTransition : AuditableEntity
{
    public string FromStatusCode { get; set; } = string.Empty;

    public string ToStatusCode { get; set; } = string.Empty;

    public string? RoleCode { get; set; }

    public string? ActionCode { get; set; }

    public bool IsActive { get; set; } = true;
}
