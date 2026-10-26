using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_SOURCES: copy of FINOVA GL_JOURNAL_SOURCES (APEX), all columns kept.
/// </summary>
public class GlJournalSource : AuditableEntity
{
    public long SourceId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public string SourceNameEn { get; set; } = string.Empty;

    public string? SourceNameAr { get; set; }

    /// <summary>One of: GL, LEASING, FACTORING, MORTGAGE, TREASURY, CASH, FA, SYSTEM, MIGRATION, OTHER.</summary>
    public string SourceGroupCode { get; set; } = "GL";

    public bool IsSystemSource { get; set; }

    public bool IsManualAllowed { get; set; }

    public bool IsImportAllowed { get; set; } = true;

    public bool IsApprovalRequired { get; set; } = true;

    public bool IsAutoPostAllowed { get; set; }

    public bool IsReversalAllowed { get; set; } = true;

    public bool IsDuplicateCheck { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
