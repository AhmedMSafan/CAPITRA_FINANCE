using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_HOUSING_PROJECTS: copy of FINOVA MFF_HOUSING_PROJECTS (APEX), all columns kept.
/// </summary>
public class MffHousingProject : AuditableEntity
{
    public long ProjectId { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public long? AnnouncementId { get; set; }

    public string ProjectNameAr { get; set; } = string.Empty;

    public string? GovernorateCode { get; set; }

    public string? CityCode { get; set; }

    public string? DistrictAr { get; set; }

    public string? NeighborhoodAr { get; set; }

    public string? SiteDescription { get; set; }

    public decimal? DefaultAreaSqm { get; set; }

    public bool IsActive { get; set; } = true;
}
