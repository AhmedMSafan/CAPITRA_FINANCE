using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_ACCOUNT_EVENTS: copy of FINOVA REF_MFF_ACCOUNT_EVENT (APEX), all columns kept.
/// </summary>
public class MffLkpAccountEvent : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
