using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_HOUSING_ANNOUNCEMENTS: copy of FINOVA MFF_HOUSING_ANNOUNCEMENTS (APEX), all columns kept.
/// </summary>
public class MffHousingAnnouncement : AuditableEntity
{
    public long AnnouncementId { get; set; }

    public string AnnouncementCode { get; set; } = string.Empty;

    public string AnnouncementNameAr { get; set; } = string.Empty;

    public DateOnly? IssueDate { get; set; }

    public DateOnly? CloseDate { get; set; }

    public string? SourceReference { get; set; }

    public bool IsActive { get; set; } = true;
}
