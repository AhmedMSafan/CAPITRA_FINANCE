using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_PROJECT: a developer's real estate project (APEX App 105 pages 105/106).</summary>
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

    /// <summary>Completion percentage, 0 to 100.</summary>
    public decimal? ProjectCompletionPct { get; set; }

    /// <summary>Free text, as in APEX.</summary>
    public string? ProjectStatus { get; set; }

    public bool IsActive { get; set; } = true;
}
