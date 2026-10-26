using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_ACCOUNTING_PERIODS: a period of a fiscal year, e.g. JAN-2026, or the adjustment period ADJ-2026.</summary>
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

    /// <summary>An adjustment period may overlap the regular periods; it accepts adjustment journals only.</summary>
    public bool IsAdjustmentPeriod { get; set; }

    /// <summary>Copied to a ledger period when it is first created; each ledger keeps its own status afterwards.</summary>
    public string DefaultPeriodStatus { get; set; } = GlCodes.PeriodNeverOpened;
}
