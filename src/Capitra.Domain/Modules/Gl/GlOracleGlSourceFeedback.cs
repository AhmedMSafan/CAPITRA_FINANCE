using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_SOURCE_FEEDBACKS: copy of FINOVA GL_ORACLE_GL_SOURCE_FEEDBACK (APEX), all columns kept.
/// </summary>
public class GlOracleGlSourceFeedback : AuditableEntity
{
    public long FeedbackId { get; set; }

    public long ExportBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public string? SourceModule { get; set; }

    public string? SourceDocumentType { get; set; }

    public string? SourceDocumentId { get; set; }

    public string? SourceDocumentNo { get; set; }

    /// <summary>One of: EXPORT_READY, EXPORTED, IMPORT_SUBMITTED, IMPORTED, ORACLE_POSTED, ERROR, REVERSED.</summary>
    public string OracleStatus { get; set; } = string.Empty;

    public long? OracleJeBatchId { get; set; }

    public long? OracleJeHeaderId { get; set; }

    public string? OracleReference { get; set; }

    public string? FeedbackPayload { get; set; }

    public bool IsFeedbackSent { get; set; }

    public DateTime? FeedbackSentAt { get; set; }

    public string? ErrorMessage { get; set; }
}
