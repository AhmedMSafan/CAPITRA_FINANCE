using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PROJECT_UNITS: copy of FINOVA FNV_PROJECT_UNIT (APEX), all columns kept.
/// </summary>
public class MtgProjectUnit : AuditableEntity
{
    public long UnitId { get; set; }

    public long ProjectId { get; set; }

    public string UnitCode { get; set; } = string.Empty;

    public string? UnitTypeCode { get; set; }

    public string? BuildingNo { get; set; }

    public string? FloorNo { get; set; }

    public string? UnitNo { get; set; }

    public decimal? AreaSqm { get; set; }

    public decimal? ContractValue { get; set; }

    public decimal? CurrentMarketValue { get; set; }

    public string? LicenseStatus { get; set; }

    public bool IsActive { get; set; } = true;
}
