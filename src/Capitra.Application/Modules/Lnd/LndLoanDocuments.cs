using System.Security.Cryptography;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>A finance's document as its page lists it: where it was added, by whom and when; CanRemove for the user who added it.</summary>
public sealed record LndLoanDocumentDto(long DocumentId, string? DocumentTypeCode, string DocumentTitle, string FileName, string MimeType, long FileSizeBytes,
    string StepName, string CreatedBy, DateTime CreatedAt, bool CanRemove);

/// <summary>
/// A finance's documents (LND_LOAN_DOCUMENTS, 27 Nov 2026), kept like the supplier documents: any user who sees the finance adds one at
/// any time, noting the workflow step waiting then (e.g. the delivery note the customer signed, at Delivery and customer receipt);
/// only the user who added it removes it (kept, marked inactive), and not once the finance is booked.
/// </summary>
public sealed class LndLoanDocumentService(ICapitraDbContext db, ICurrentUser currentUser)
{
    public const long MaxFileBytes = 20 * 1024 * 1024;
    public static readonly string[] DocumentTypes = ["DELIVERY_NOTE", "CONTRACT", "OTHER"];

    private static readonly HashSet<string> Mimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg", "image/tiff",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/plain",
    };

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    public async Task<IReadOnlyList<LndLoanDocumentDto>> ListAsync(long loanId, CancellationToken ct)
    {
        var loan = await LoanAsync(loanId, ct);
        var me = currentUser.UserName;
        return await db.Set<LndLoanDocument>().AsNoTracking().Where(x => x.LoanId == loanId && x.IsActive).OrderByDescending(x => x.DocumentId)
            .Select(x => new LndLoanDocumentDto(x.DocumentId, x.DocumentTypeCode, x.DocumentTitle, x.FileName, x.MimeType, x.FileSizeBytes, x.StepName,
                x.CreatedBy, x.CreatedAt, x.CreatedBy == me && loan.StatusCode != LndLoanService.Statuses.Active))
            .ToListAsync(ct);
    }

    public async Task<LndLoanDocumentDto> AddAsync(long loanId, string? typeCode, string? title, string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        var loan = await LoanAsync(loanId, ct);
        var type = string.IsNullOrWhiteSpace(typeCode) ? null : typeCode.Trim().ToUpperInvariant();
        if (type is not null && !DocumentTypes.Contains(type)) throw BusinessValidationException.For("documentTypeCode", "Choose the document type.");
        if (content.Length == 0 || content.Length > MaxFileBytes) throw BusinessValidationException.For("file", "The file is empty or larger than 20 MB.");
        var mime = mimeType.Trim().ToLowerInvariant();
        if (!Mimes.Contains(mime)) throw BusinessValidationException.For("file", "Attach a PDF, image, Word, Excel or text file.");

        // where it was added: the workflow step waiting now, else the finance's status
        string stepCode = loan.StatusCode, stepName = loan.StatusCode.ToLowerInvariant().Replace('_', ' ');
        if (loan.WorkflowInstanceId is { } run)
        {
            var open = await db.Set<WfeStepInstance>().AsNoTracking()
                .Where(s => s.WorkflowInstanceId == run && s.StepStatus != WfeRuntime.Completed && s.StepStatus != WfeRuntime.Cancelled && s.StepCode != null && s.StepCode != "")
                .OrderByDescending(s => s.StepInstanceId).Select(s => s.StepCode).FirstOrDefaultAsync(ct);
            if (open is not null) (stepCode, stepName) = (open, LndCfFinanceRules.StepName(open));
        }
        var name = Path.GetFileName(fileName.Trim());
        var doc = new LndLoanDocument
        {
            LoanId = loanId, StepCode = Cut(stepCode, 50), StepName = Cut(stepName, 200), DocumentTypeCode = type,
            DocumentTitle = Cut(string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(name) : title.Trim(), 300), FileName = Cut(name, 260),
            MimeType = mime, FileSizeBytes = content.Length, Sha256Hash = Convert.ToHexStringLower(SHA256.HashData(content)), FileBlob = content,
        };
        db.Add(doc);
        await db.SaveChangesAsync(ct);
        return new LndLoanDocumentDto(doc.DocumentId, doc.DocumentTypeCode, doc.DocumentTitle, doc.FileName, doc.MimeType, doc.FileSizeBytes, doc.StepName,
            Me, doc.CreatedAt, loan.StatusCode != LndLoanService.Statuses.Active);
    }

    public async Task RemoveAsync(long documentId, CancellationToken ct)
    {
        var doc = await db.Set<LndLoanDocument>().FirstOrDefaultAsync(x => x.DocumentId == documentId && x.IsActive, ct)
            ?? throw new NotFoundException("The document was not found.");
        var loan = await LoanAsync(doc.LoanId, ct);
        if (!string.Equals(doc.CreatedBy, Me, StringComparison.OrdinalIgnoreCase) || loan.StatusCode == LndLoanService.Statuses.Active)
            throw new BusinessRuleException("Only the user who added a document can remove it, and not once the finance is booked.");
        doc.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CrmCfDocumentFile> FileAsync(long documentId, CancellationToken ct)
    {
        var f = await db.Set<LndLoanDocument>().AsNoTracking().Where(x => x.DocumentId == documentId && x.IsActive)
            .Select(x => new CrmCfDocumentFile(x.FileName, x.MimeType, x.Sha256Hash, x.FileBlob)).FirstOrDefaultAsync(ct);
        return f ?? throw new NotFoundException("The document was not found.");
    }

    private async Task<LndLoan> LoanAsync(long loanId, CancellationToken ct) =>
        await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct) ?? throw new NotFoundException($"Finance {loanId} was not found.");

    private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;
}
