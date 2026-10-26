using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_FISCAL_YEARS: one fiscal year of a calendar. Its status is a label; posting is controlled per ledger period.</summary>
public class GlFiscalYear : AuditableEntity
{
    public long FiscalYearId { get; set; }

    public long CalendarId { get; set; }

    public int FiscalYear { get; set; }

    public string? YearName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    /// <summary>DRAFT, OPEN, CLOSED or PERMANENTLY_CLOSED.</summary>
    public string YearStatus { get; set; } = GlCodes.YearOpen;
}
