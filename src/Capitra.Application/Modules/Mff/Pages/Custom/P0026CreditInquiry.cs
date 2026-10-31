using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 26, After Submit "Save Credit Inquiry Result" (SAVE_RESULT).</summary>
public sealed class P0026CreditInquiry(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 26;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_RESULT")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_CREDIT_INQUIRIES";
            var appId = Req(Long(request, "P26_APPLICATION_ID"), t, "APPLICATION_ID");
            var result = Text(request, "P26_RESULT_CODE");
            var notes = Text(request, "P26_NOTES");
            db.Add(new MffCreditInquiry
            {
                ApplicationId = appId,
                BureauOrgId = Req(Long(request, "P26_BUREAU_ORG_ID"), t, "BUREAU_ORG_ID"),
                RequestDate = Req(Date(request, "P26_REQUEST_DATE"), t, "REQUEST_DATE"),
                ResponseDate = Date(request, "P26_RESPONSE_DATE"),
                CreditScore = Number(request, "P26_CREDIT_SCORE"),
                ResultCode = result,
                ReferenceNo = Text(request, "P26_REFERENCE_NO"),
                Notes = notes,
            });
            await db.SaveChangesAsync(ct);

            var status = await db.Set<MffApplication>().Where(x => x.ApplicationId == appId).Select(x => x.StatusCode).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            var me = user.UserName;
            if (status == "DOCUMENTS_PENDING")
                await control.ChangeStatusCheckedAsync(appId, "CREDIT_INQUIRY", "CREDIT_BUREAU", me, "Credit inquiry started", ct);
            if (result == "PASS")
                await control.ChangeStatusCheckedAsync(appId, "CREDIT_ACCEPTED", "CREDIT_BUREAU", me, "Credit inquiry accepted", ct);
            else if (result == "FAIL")
                await control.ChangeStatusCheckedAsync(appId, "REJECTED", "CREDIT_BUREAU", me, notes ?? "Credit inquiry rejected", ct);
            return ("Credit inquiry result saved.", null);
        }, ct);
    }
}
