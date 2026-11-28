using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_LKP_LOCATIONS: copy of FINOVA LKP_CHEQUE_LOCATION (APEX), all columns kept.
/// </summary>
public class ChqLkpLocation : AuditableEntity
{
    public string LocationCode { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public string? LocationType { get; set; }

    public bool IsActive { get; set; } = true;

    public int? DisplaySeq { get; set; }
}
