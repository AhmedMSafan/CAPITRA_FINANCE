using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_PROJECT_UNIT: a unit inside a project (APEX App 105 pages 107/108).</summary>
public class MtgProjectUnit : AuditableEntity
{
    public long ProjectUnitId { get; set; }

    public long ProjectId { get; set; }

    public string UnitCode { get; set; } = string.Empty;

    /// <summary>MTG_LKP_UNIT_TYPE code.</summary>
    public string? UnitTypeCode { get; set; }

    public string? BuildingNo { get; set; }

    public string? FloorNo { get; set; }

    public string? UnitNo { get; set; }

    public decimal? AreaSqm { get; set; }

    public decimal? ContractAmount { get; set; }

    public decimal? MarketValueAmount { get; set; }

    /// <summary>Free text, as in APEX.</summary>
    public string? LicenseStatus { get; set; }

    public bool IsActive { get; set; } = true;
}
