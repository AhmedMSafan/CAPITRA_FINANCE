using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_LKP_PARTY_ROLES: copy of FINOVA REF_MFF_PARTY_ROLE (APEX), all columns kept.
/// </summary>
public class MffLkpPartyRole : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
