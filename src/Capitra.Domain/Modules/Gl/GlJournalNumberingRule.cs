using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_NUMBERING_RULES: copy of FINOVA GL_JOURNAL_NUMBERING_RULES (APEX), all columns kept.
/// </summary>
public class GlJournalNumberingRule : AuditableEntity
{
    public long NumberingRuleId { get; set; }

    public long LedgerId { get; set; }

    public long? SourceId { get; set; }

    public long? CategoryId { get; set; }

    /// <summary>One of: BATCH, JOURNAL, INTERFACE, POSTING_RUN, REVALUATION_RUN.</summary>
    public string JournalType { get; set; } = "JOURNAL";

    public string Prefix { get; set; } = string.Empty;

    public bool IsIncludeYear { get; set; } = true;

    public bool IsIncludePeriod { get; set; }

    public int NextNumber { get; set; } = 1;

    public int NumberLength { get; set; } = 6;

    /// <summary>One of: NEVER, YEARLY, PERIODIC, MONTHLY.</summary>
    public string ResetFrequency { get; set; } = "YEARLY";

    public bool IsActive { get; set; } = true;
}
