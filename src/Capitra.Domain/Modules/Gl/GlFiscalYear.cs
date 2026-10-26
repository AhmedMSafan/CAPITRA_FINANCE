using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_FISCAL_YEARS: copy of FINOVA GL_FISCAL_YEARS (APEX), all columns kept.
/// </summary>
public class GlFiscalYear : AuditableEntity
{
    public long FiscalYearId { get; set; }

    public long CalendarId { get; set; }

    public int FiscalYear { get; set; }

    public string? YearName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    /// <summary>One of: DRAFT, OPEN, CLOSED, PERMANENTLY_CLOSED.</summary>
    public string YearStatus { get; set; } = "DRAFT";
}
