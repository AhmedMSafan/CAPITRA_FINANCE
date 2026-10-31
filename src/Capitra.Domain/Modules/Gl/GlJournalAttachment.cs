using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_JOURNAL_ATTACHMENTS: copy of FINOVA GL_JOURNAL_ATTACHMENTS (APEX), all columns kept.
/// </summary>
public class GlJournalAttachment : AuditableEntity
{
    public long AttachmentId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    public long? BatchId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? MimeType { get; set; }

    public int? FileSizeBytes { get; set; }

    public byte[]? FileBlob { get; set; }

    public string? FileUrl { get; set; }

    public string AttachmentCategory { get; set; } = "SUPPORTING_DOCUMENT";

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
