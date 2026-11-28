using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_SOURCE_POSTING_LINKS: copy of FINOVA GL_SOURCE_POSTING_LINKS (APEX), all columns kept.
/// </summary>
public class GlSourcePostingLink : AuditableEntity
{
    public long SourcePostingLinkId { get; set; }

    public long SourceId { get; set; }

    public long LedgerId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string SourceDocumentType { get; set; } = string.Empty;

    public string SourceDocumentId { get; set; } = string.Empty;

    public string? SourceDocumentNo { get; set; }

    public long? AccountingEventId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    /// <summary>One of: POSTED, REVERSED, CANCELLED.</summary>
    public string PostingStatus { get; set; } = "POSTED";

    public DateTime PostedAt { get; set; }

    public long? ReversalLinkId { get; set; }
}
