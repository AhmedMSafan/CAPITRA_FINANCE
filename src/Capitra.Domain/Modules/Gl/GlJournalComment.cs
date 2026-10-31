using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_COMMENTS: copy of FINOVA GL_JOURNAL_COMMENTS (APEX), all columns kept.
/// </summary>
public class GlJournalComment : AuditableEntity
{
    public long CommentId { get; set; }

    public long? BatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    /// <summary>One of: GENERAL, APPROVAL, REJECTION, RETURN, POSTING, REVERSAL, CANCEL, ERROR.</summary>
    public string CommentType { get; set; } = "GENERAL";

    public string CommentText { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}
