using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RECURRING_JOURNAL_TEMPLATES: copy of FINOVA GL_RECURRING_JOURNAL_TEMPLATES (APEX), all columns kept.
/// </summary>
public class GlRecurringJournalTemplate : AuditableEntity
{
    public long RecurringTemplateId { get; set; }

    public long LedgerId { get; set; }

    public long SourceId { get; set; }

    public long CategoryId { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>One of: DAILY, WEEKLY, MONTHLY, QUARTERLY, YEARLY, PERIODIC.</summary>
    public string RecurrenceFrequency { get; set; } = "MONTHLY";

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly? NextGenerationDate { get; set; }

    public bool IsAutoSubmit { get; set; }

    public bool IsAutoPost { get; set; }

    public bool IsActive { get; set; } = true;
}
