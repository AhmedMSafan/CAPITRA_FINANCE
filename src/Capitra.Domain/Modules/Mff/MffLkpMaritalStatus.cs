using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_MARITAL_STATUSES: copy of FINOVA REF_MFF_MARITAL_STATUS (APEX), all columns kept.
/// </summary>
public class MffLkpMaritalStatus : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
