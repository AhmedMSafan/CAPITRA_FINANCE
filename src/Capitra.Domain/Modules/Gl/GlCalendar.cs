using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CALENDARS: copy of FINOVA GL_CALENDARS (APEX), all columns kept.
/// </summary>
public class GlCalendar : AuditableEntity
{
    public long CalendarId { get; set; }

    public string CalendarCode { get; set; } = string.Empty;

    public string CalendarNameEn { get; set; } = string.Empty;

    public string? CalendarNameAr { get; set; }

    /// <summary>One of: MONTHLY, QUARTERLY, CUSTOM.</summary>
    public string CalendarType { get; set; } = "MONTHLY";

    /// <summary>One of: MONTHLY, QUARTERLY, YEARLY, CUSTOM.</summary>
    public string PeriodFrequency { get; set; } = "MONTHLY";

    public bool IsAdjustmentPeriod { get; set; } = true;

    public bool IsActive { get; set; } = true;
}
