using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_VALIDATION_RULES: copy of FINOVA GL_JOURNAL_VALIDATION_RULES (APEX), all columns kept.
/// </summary>
public class GlJournalValidationRule : AuditableEntity
{
    public long ValidationRuleId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    public long? SourceId { get; set; }

    public long? CategoryId { get; set; }

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string Severity { get; set; } = "ERROR";

    public bool IsActive { get; set; } = true;

    public string? ValidationMessage { get; set; }
}
