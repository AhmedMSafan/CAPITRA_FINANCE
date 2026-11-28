using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_FACILITY_TRANCHES: the part of a facility the bank has released (or will release) to us. A facility of 100M may come as a first
/// tranche of 60M now and a second of 40M on request. Only an ACTIVE tranche can be drawn.
/// </summary>
public class TrsFacilityTranche : AuditableEntity
{
    public long TrancheId { get; set; }

    public long FacilityId { get; set; }

    public string TrancheName { get; set; } = string.Empty;

    public decimal TrancheAmount { get; set; }

    /// <summary>One of: DRAFT (not released yet), ACTIVE, CLOSED.</summary>
    public string StatusCode { get; set; } = "DRAFT";

    public DateOnly? ReleasedDate { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public string? Notes { get; set; }
}
