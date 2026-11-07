using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_FACILITY_CALENDAR_DAYS: copy of FINOVA TRS_FACILITY_CALENDAR_DAYS (APEX), all columns kept.
/// </summary>
public class TrsFacilityCalendarDay : AuditableEntity
{
    public long CalendarDayId { get; set; }

    public long? BankId { get; set; }

    public DateOnly CalendarDate { get; set; }

    public bool IsWorkingDay { get; set; } = true;

    public string? HolidayNameEn { get; set; }

    public string? HolidayNameAr { get; set; }

    public string? Notes { get; set; }
}
