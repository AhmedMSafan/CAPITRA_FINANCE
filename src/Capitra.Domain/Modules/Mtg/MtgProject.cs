using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PROJECTS: copy of FINOVA FNV_PROJECT (APEX), all columns kept.
/// </summary>
public class MtgProject : AuditableEntity
{
    public long ProjectId { get; set; }

    public long DeveloperId { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public string ProjectNameAr { get; set; } = string.Empty;

    public string? ProjectNameEn { get; set; }

    public string? LocationText { get; set; }

    public decimal? LandArea { get; set; }

    public string? TitleDeedNo { get; set; }

    public string? BuildingLicenseNo { get; set; }

    public DateOnly? BuildingLicenseDate { get; set; }

    public decimal? ProjectCompletionPct { get; set; }

    public string? ProjectStatus { get; set; }

    public bool IsActive { get; set; } = true;
}
