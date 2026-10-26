using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_JOURNAL_SOURCES: where journals come from: MANUAL (journal entry page) or a sub-ledger module.</summary>
public class GlJournalSource : AuditableEntity
{
    public long SourceId { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public string SourceNameEn { get; set; } = string.Empty;

    public string? SourceNameAr { get; set; }

    /// <summary>Created by the system; cannot be removed.</summary>
    public bool IsSystemSource { get; set; }

    public bool IsManualAllowed { get; set; }

    /// <summary>Journals of the source are approved by a second user before they post.</summary>
    public bool IsApprovalRequired { get; set; } = true;

    public bool IsReversalAllowed { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
