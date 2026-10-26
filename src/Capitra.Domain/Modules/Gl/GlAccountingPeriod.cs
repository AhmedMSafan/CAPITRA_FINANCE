using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_PERIODS: copy of FINOVA GL_ACCOUNTING_PERIODS (APEX), all columns kept.
/// </summary>
public class GlAccountingPeriod : AuditableEntity
{
    public long PeriodId { get; set; }

    public long FiscalYearId { get; set; }

    public long CalendarId { get; set; }

    public int PeriodNo { get; set; }

    public string PeriodCode { get; set; } = string.Empty;

    public string PeriodNameEn { get; set; } = string.Empty;

    public string? PeriodNameAr { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? QuarterNo { get; set; }

    public bool IsAdjustmentPeriod { get; set; }

    /// <summary>One of: NEVER_OPENED, FUTURE_ENTRY, OPEN, CLOSED, PERMANENTLY_CLOSED.</summary>
    public string PeriodStatus { get; set; } = "NEVER_OPENED";

    public string? OpenedBy { get; set; }

    public DateTime? OpenedAt { get; set; }

    public string? ClosedBy { get; set; }

    public DateTime? ClosedAt { get; set; }
}
