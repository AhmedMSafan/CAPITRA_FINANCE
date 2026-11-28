using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_RECURRING_RUNS: copy of FINOVA GL_CLOSE_RECURRING_RUNS (APEX), all columns kept.
/// </summary>
public class GlCloseRecurringRun : AuditableEntity
{
    public long CloseRecurringRunId { get; set; }

    public long CloseRunId { get; set; }

    public long? RecurringTemplateId { get; set; }

    public long? JournalBatchId { get; set; }

    public int? GeneratedJournalsCount { get; set; }

    /// <summary>One of: DRAFT, GENERATED, POSTED, CANCELLED, ERROR.</summary>
    public string RunStatus { get; set; } = "DRAFT";

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
