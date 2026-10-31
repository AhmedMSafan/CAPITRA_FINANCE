using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROPERTIES: copy of FINOVA MFF_PROPERTIES (APEX), all columns kept.
/// </summary>
public class MffProperty : AuditableEntity
{
    public long PropertyId { get; set; }

    public long? ProjectId { get; set; }

    public long? SellerPersonId { get; set; }

    public string? GovernorateCode { get; set; }

    public string? CityCode { get; set; }

    public string? DistrictAr { get; set; }

    public string? NeighborhoodAr { get; set; }

    public string? BuildingNo { get; set; }

    public string? FloorNo { get; set; }

    public string? UnitNo { get; set; }

    public decimal? AreaSqm { get; set; }

    public string? AddressAr { get; set; }

    public string? RegistrationStatusCode { get; set; }

    public string? RegistryReference { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? AppraisedValue { get; set; }

    public decimal? MaintenanceDeposit { get; set; }
}
