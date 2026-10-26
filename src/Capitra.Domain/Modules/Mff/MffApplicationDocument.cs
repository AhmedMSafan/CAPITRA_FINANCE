using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPLICATION_DOCUMENTS: copy of FINOVA MFF_APPLICATION_DOCUMENTS (APEX), all columns kept.
/// </summary>
public class MffApplicationDocument : AuditableEntity
{
    public long DocumentId { get; set; }

    public long ApplicationId { get; set; }

    public long? PersonId { get; set; }

    public string DocumentTypeCode { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? MimeType { get; set; }

    public int? FileSizeBytes { get; set; }

    public byte[]? FileContent { get; set; }

    public string? DocumentNo { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public string VerificationStatus { get; set; } = "PENDING";

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? RejectionReason { get; set; }
}
