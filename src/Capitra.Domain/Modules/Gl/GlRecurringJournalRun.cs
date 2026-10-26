using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RECURRING_JOURNAL_RUNS: copy of FINOVA GL_RECURRING_JOURNAL_RUNS (APEX), all columns kept.
/// </summary>
public class GlRecurringJournalRun : AuditableEntity
{
    public long RecurringRunId { get; set; }

    public long RecurringTemplateId { get; set; }

    public long LedgerPeriodId { get; set; }

    public DateOnly GenerationDate { get; set; }

    /// <summary>One of: NEW, GENERATED, ERROR, CANCELLED.</summary>
    public string RunStatus { get; set; } = "NEW";

    public long? GeneratedHeaderId { get; set; }

    public string? ErrorMessage { get; set; }
}
