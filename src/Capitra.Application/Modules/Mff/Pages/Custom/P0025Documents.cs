using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 25, After Submit "Upload Document" (UPLOAD_DOC): the File Browse file from the APEX session.</summary>
public sealed class P0025Documents(ICapitraDbContext db, ApexSession session, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 25;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "UPLOAD_DOC")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var file = session.File(Text(request, "P25_FILE")) ?? throw new ApexProcessException("ORA-01403: no data found");
            var appId = Long(request, "P25_APPLICATION_ID");
            var person = await db.Set<MffApplication>().Where(x => x.ApplicationId == appId).Select(x => (long?)x.PrimaryPersonId).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            db.Add(new MffApplicationDocument
            {
                ApplicationId = appId!.Value,
                PersonId = person,
                DocumentTypeCode = Req(Text(request, "P25_DOCUMENT_TYPE_CODE"), "MFF_APPLICATION_DOCUMENTS", "DOCUMENT_TYPE_CODE"),
                FileName = file.FileName,
                MimeType = file.MimeType,
                FileSizeBytes = file.Content.Length,
                FileContent = file.Content,
                DocumentNo = Text(request, "P25_DOCUMENT_NO"),
                IssueDate = Date(request, "P25_ISSUE_DATE"),
                ExpiryDate = Date(request, "P25_EXPIRY_DATE"),
                ReceivedDate = Today(clock),
                VerificationStatus = "PENDING",
            });
            await db.SaveChangesAsync(ct);
            return ("Document uploaded.", null);
        }, ct);
    }
}
