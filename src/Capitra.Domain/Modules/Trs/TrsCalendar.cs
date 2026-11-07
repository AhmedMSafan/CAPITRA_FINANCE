using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_CALENDARS: copy of FINOVA TRS_CALENDAR (APEX), all columns kept.
/// </summary>
public class TrsCalendar : AuditableEntity
{
    public long CalendarId { get; set; }

    public long CompanyId { get; set; }

    public DateOnly CalendarDate { get; set; }

    public string CountryCode { get; set; } = "EG";

    public bool IsBusinessDay { get; set; } = true;

    public string? HolidayNameEn { get; set; }

    public string? HolidayNameAr { get; set; }

    public bool IsCbeNonWorking { get; set; }

    public bool IsActive { get; set; } = true;
}
