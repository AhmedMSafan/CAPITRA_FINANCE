using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 54, After Submit "Add Review Check" (ADD_CHECK) and "Update Document Verification" (UPDATE_DOCUMENT).</summary>
public sealed class P0054DocumentReview(ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 54;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("ADD_CHECK" or "UPDATE_DOCUMENT"))
            return Task.FromResult<object>(new { ok = true });
        var appId = Long(request, "P54_APPLICATION_ID");
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "ADD_CHECK")
            {
                var type = Text(request, "P54_CHECK_TYPE");
                var code = Text(request, "P54_CHECK_CODE");
                var result = Text(request, "P54_RESULT_CODE");
                if (type is null || code is null || result is null)
                    throw new ApexProcessException("ORA-20047: Check Type, Check Code and Check Result are required.");
                db.Add(new MffReviewCheck
                {
                    ApplicationId = Req(appId, "MFF_REVIEW_CHECKS", "APPLICATION_ID"),
                    CheckType = type,
                    CheckCode = code,
                    ResultCode = result,
                    CheckedBy = user.UserName,
                    CheckedAt = clock.UtcNow,
                    Notes = Text(request, "P54_CHECK_NOTES"),
                });
                await db.SaveChangesAsync(ct);
                return ("Review check recorded.", null);
            }

            var docId = Long(request, "P54_DOCUMENT_ID") ?? throw new ApexProcessException("ORA-20045: Select a document.");
            var d = await db.Set<MffApplicationDocument>().FirstOrDefaultAsync(x => x.DocumentId == docId && x.ApplicationId == appId, ct)
                ?? throw new ApexProcessException("ORA-20046: Document not found for this application.");
            var status = Text(request, "P54_VERIFICATION_STATUS");
            d.VerificationStatus = Req(status, "MFF_APPLICATION_DOCUMENTS", "VERIFICATION_STATUS");
            d.VerifiedBy = user.UserName;
            d.VerifiedAt = clock.UtcNow;
            d.RejectionReason = status == "REJECTED" ? Text(request, "P54_REJECTION_REASON") : null;
            await db.SaveChangesAsync(ct);
            return ("Document verification updated.", null);
        }, ct);
    }
}
