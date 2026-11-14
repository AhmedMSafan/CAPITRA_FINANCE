using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_VALIDATION_RESULTS: copy of FINOVA GL_JOURNAL_VALIDATION_RESULTS (APEX), all columns kept.
/// </summary>
public class GlJournalValidationResult : AuditableEntity
{
    public long ValidationResultId { get; set; }

    public long? ValidationRuleId { get; set; }

    public long? BatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>One of: OPEN, RESOLVED, WAIVED.</summary>
    public string ResultStatus { get; set; } = "OPEN";

    public string ResultMessage { get; set; } = string.Empty;

    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }
}
