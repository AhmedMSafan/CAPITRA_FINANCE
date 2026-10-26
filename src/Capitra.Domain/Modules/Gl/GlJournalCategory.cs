using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_JOURNAL_CATEGORIES: what kind of entry a journal is: standard, adjustment ...</summary>
public class GlJournalCategory : AuditableEntity
{
    public long CategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryNameEn { get; set; } = string.Empty;

    public string? CategoryNameAr { get; set; }

    /// <summary>Offered in journal entry.</summary>
    public bool IsManualAllowed { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
