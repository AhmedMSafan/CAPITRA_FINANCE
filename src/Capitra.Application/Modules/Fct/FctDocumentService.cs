using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>A row of FINOVA's document list (its GET .../documents shape).</summary>
public sealed record FctAgreementDocumentDto(
    long DocumentId, long AgreementId, long DocTypeId, string DocumentTitle, string FileName, string? FileExtension, long? FileSizeBytes,
    string? MimeType, long StatusId, DateTime CreationDate, string CreatedBy);

public sealed record FctLovItem(long Value, string Label, string Code);

public sealed record FctDocumentLookupsDto(IReadOnlyList<FctLovItem> DocumentTypes, IReadOnlyList<FctLovItem> DocumentStatuses);

public sealed record FctDocumentFile(string FileName, string MimeType, byte[] Content);

/// <summary>
/// The documents of a factoring agreement, from FINOVA_FINANCEING FactoringAgreementService (GetDocumentsAsync,
/// UploadDocumentAsync, GetDocumentAsync). Files are stored in the database, as there (STORAGE_TYPE DB).
/// </summary>
public sealed class FctDocumentService(ICapitraDbContext db)
{
    /// <summary>FINOVA's upload limit.</summary>
    public const long MaxFileBytes = 10 * 1024 * 1024;

    /// <summary>The factoring document types (GROUP_CODE FACTORING), by name as FINOVA's list, and every active status.</summary>
    public async Task<FctDocumentLookupsDto> LookupsAsync(CancellationToken cancellationToken)
    {
        var types = await db.Set<AdmLkpDocumentType>().AsNoTracking()
            .Where(x => x.IsActive && x.GroupCode == "FACTORING")
            .OrderBy(x => x.DocumentTypeName)
            .Select(x => new FctLovItem(x.DocumentTypeId, x.DocumentTypeName, x.DocumentTypeCode))
            .ToListAsync(cancellationToken);
        var statuses = await db.Set<AdmLkpDocumentStatusCode>().AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.StatusName)
            .Select(x => new FctLovItem(x.StatusId, x.StatusName, x.StatusCodeValue))
            .ToListAsync(cancellationToken);
        return new FctDocumentLookupsDto(types, statuses);
    }

    public async Task<IReadOnlyList<FctAgreementDocumentDto>> ListAsync(long agreementId, CancellationToken cancellationToken) =>
        await db.Set<FctAgreementDocument>().AsNoTracking()
            .Where(x => x.AgreementId == agreementId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FctAgreementDocumentDto(
                x.DocumentId, x.AgreementId, x.DocTypeId, x.DocumentTitle, x.FileName, x.FileExtension, x.FileSizeBytes, x.MimeType,
                x.StatusId, x.CreatedAt, x.CreatedBy))
            .ToListAsync(cancellationToken);

    public async Task<FctAgreementDocumentDto> UploadAsync(
        long agreementId, string fileName, string? mimeType, byte[] content, long docTypeId, long statusId, CancellationToken cancellationToken)
    {
        if (content.Length == 0)
            throw new BusinessRuleException("No document file was provided.");
        if (content.Length > MaxFileBytes)
            throw new BusinessRuleException("The selected file exceeds the 10 MB limit.");
        if (!await db.Set<FctAgreement>().AnyAsync(x => x.AgreementId == agreementId, cancellationToken))
            throw new NotFoundException($"Factoring agreement {agreementId} was not found.");
        if (!await db.Set<AdmLkpDocumentType>().AnyAsync(x => x.DocumentTypeId == docTypeId && x.IsActive, cancellationToken))
            throw new BusinessRuleException("Please select document type and status.");
        if (!await db.Set<AdmLkpDocumentStatusCode>().AnyAsync(x => x.StatusId == statusId && x.IsActive, cancellationToken))
            throw new BusinessRuleException("Please select document type and status.");

        var name = Path.GetFileName(fileName);
        var extension = Path.GetExtension(name).TrimStart('.');
        var document = new FctAgreementDocument
        {
            AgreementId = agreementId,
            DocTypeId = docTypeId,
            StatusId = statusId,
            DocumentTitle = name,
            FileName = name,
            FileExtension = extension.Length == 0 ? null : extension,
            FileSizeBytes = content.Length,
            MimeType = mimeType,
            FileContent = content,
            StorageType = "DB",
            VersionNumber = 1,
        };
        db.Add(document);
        await db.SaveChangesAsync(cancellationToken);
        return new FctAgreementDocumentDto(
            document.DocumentId, document.AgreementId, document.DocTypeId, document.DocumentTitle, document.FileName, document.FileExtension,
            document.FileSizeBytes, document.MimeType, document.StatusId, document.CreatedAt, document.CreatedBy);
    }

    public async Task<FctDocumentFile> ContentAsync(long documentId, CancellationToken cancellationToken)
    {
        var document = await db.Set<FctAgreementDocument>().AsNoTracking()
            .Where(x => x.DocumentId == documentId && x.IsActive)
            .Select(x => new { x.FileName, x.MimeType, x.FileContent })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Document {documentId} was not found.");
        if (document.FileContent is not { Length: > 0 })
            throw new NotFoundException($"Document {documentId} has no stored file content.");
        return new FctDocumentFile(document.FileName, string.IsNullOrWhiteSpace(document.MimeType) ? "application/octet-stream" : document.MimeType,
            document.FileContent);
    }
}
