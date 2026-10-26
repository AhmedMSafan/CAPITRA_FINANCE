using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_CALENDARS: an accounting calendar, the set of fiscal years and periods ledgers post into (APEX App 120 page 30).</summary>
public class GlCalendar : AuditableEntity
{
    public long CalendarId { get; set; }

    public string CalendarCode { get; set; } = string.Empty;

    public string CalendarNameEn { get; set; } = string.Empty;

    public string? CalendarNameAr { get; set; }

    public string CalendarType { get; set; } = GlCodes.Monthly;

    public string PeriodFrequency { get; set; } = GlCodes.Monthly;

    /// <summary>A generated year gets a 13th adjustment period by default.</summary>
    public bool IsAdjustmentPeriodUsed { get; set; } = true;

    public bool IsActive { get; set; } = true;
}
