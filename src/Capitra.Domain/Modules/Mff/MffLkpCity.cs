using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_CITYS: copy of FINOVA REF_MFF_CITY (APEX), all columns kept.
/// </summary>
public class MffLkpCity : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string GovernorateCode { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
