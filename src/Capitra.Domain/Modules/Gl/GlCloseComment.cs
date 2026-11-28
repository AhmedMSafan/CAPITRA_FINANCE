using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_COMMENTS: copy of FINOVA GL_CLOSE_COMMENTS (APEX), all columns kept.
/// </summary>
public class GlCloseComment : AuditableEntity
{
    public long CloseCommentId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseRunTaskId { get; set; }

    public long? CloseExceptionId { get; set; }

    public string CommentText { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}
