using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RECURRING_JOURNAL_LINES: copy of FINOVA GL_RECURRING_JOURNAL_LINES (APEX), all columns kept.
/// </summary>
public class GlRecurringJournalLine : AuditableEntity
{
    public long RecurringLineId { get; set; }

    public long RecurringTemplateId { get; set; }

    public int LineNo { get; set; }

    public long AccountCombinationId { get; set; }

    public string? LineDescription { get; set; }

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    public string? AllocationFormula { get; set; }

    public bool IsActive { get; set; } = true;
}
