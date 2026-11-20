using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ATTACHMENTS: copy of FINOVA GL_CLOSE_ATTACHMENTS (APEX), all columns kept.
/// </summary>
public class GlCloseAttachment : AuditableEntity
{
    public long CloseAttachmentId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseRunTaskId { get; set; }

    public long? CloseReportId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? MimeType { get; set; }

    public byte[]? FileBlob { get; set; }

    public string? FileClob { get; set; }

    public string? AttachmentCategory { get; set; }

    public string? Description { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
