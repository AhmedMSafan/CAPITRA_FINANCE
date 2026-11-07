using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_AGREEMENT_DOCUMENTS: copy of FINOVA FCT_AGREEMENT_DOCUMENTS (APEX), all columns kept.
/// </summary>
public class FctAgreementDocument : AuditableEntity
{
    public long DocumentId { get; set; }

    public long AgreementId { get; set; }

    public long DocTypeId { get; set; }

    public string? DocumentNumber { get; set; }

    public string DocumentTitle { get; set; } = string.Empty;

    public string? DocumentTitleAr { get; set; }

    public string? DocumentDesc { get; set; }

    public string? DocumentDescAr { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? FileExtension { get; set; }

    public int? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public byte[]? FileContent { get; set; }

    public string? FilePath { get; set; }

    public string StorageType { get; set; } = "DB";

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? ExecutionDate { get; set; }

    public DateOnly? NotarizationDate { get; set; }

    public string? NotarizationRef { get; set; }

    public int? VersionNumber { get; set; }

    public bool IsLatestVersion { get; set; } = true;

    public long? PreviousVersionId { get; set; }

    public bool IsExecuted { get; set; }

    public bool IsOriginalReceived { get; set; }

    public DateOnly? OriginalReceivedDate { get; set; }

    public long StatusId { get; set; }

    public string? VerifiedBy { get; set; }

    public DateOnly? VerificationDate { get; set; }

    public bool IsConfidential { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public bool IsActive { get; set; } = true;
}
