using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_FACILITY_SUBLIMITS: a slice of a tranche that may fund only the business lines in its scope (TRS_SUBLIMIT_SCOPES; no scope rows
/// means every business line). A DEDICATED sublimit is its own pool and the dedicated sublimits of a tranche cannot add up to more than
/// the tranche; a SHARED cap only limits what may be drawn for its scope from the tranche. Usage comes from TRS_SUBLIMIT_MOVEMENTS.
/// </summary>
public class TrsFacilitySublimit : AuditableEntity
{
    public long SublimitId { get; set; }

    public long FacilityId { get; set; }

    public long TrancheId { get; set; }

    public string SublimitName { get; set; } = string.Empty;

    public decimal LimitAmount { get; set; }

    /// <summary>One of: DEDICATED, SHARED.</summary>
    public string PoolTypeCode { get; set; } = "DEDICATED";

    /// <summary>One of: ACTIVE, SUSPENDED, CLOSED.</summary>
    public string StatusCode { get; set; } = "ACTIVE";

    public string? Notes { get; set; }
}
