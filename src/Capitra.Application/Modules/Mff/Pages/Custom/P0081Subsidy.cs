using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 81, After Submit "Save Subsidy" (SAVE_SUBSIDY).</summary>
public sealed class P0081Subsidy(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 81;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_SUBSIDY")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_SUBSIDY_APPROVALS";
            var appId = Long(request, "P81_APPLICATION_ID");
            var npv = Number(request, "P81_INSTALLMENT_SUPPORT_NPV");
            var dps = Number(request, "P81_DOWNPAYMENT_SUPPORT");
            var total = Number(request, "P81_TOTAL_SUPPORT") ?? (npv ?? 0) + (dps ?? 0);
            var approvalNo = Text(request, "P81_APPROVAL_NO");
            var approvalDate = Date(request, "P81_APPROVAL_DATE");
            var expiry = Date(request, "P81_EXPIRY_DATE");
            var status = Text(request, "P81_APPROVAL_STATUS");
            var notes = Text(request, "P81_FUND_NOTES");
            var id = Long(request, "P81_SUBSIDY_APPROVAL_ID");
            MffSubsidyApproval? s;
            if (id is null)
            {
                s = new MffSubsidyApproval { ApplicationId = Req(appId, t, "APPLICATION_ID") };
                db.Add(s);
            }
            else
            {
                s = await db.Set<MffSubsidyApproval>().FirstOrDefaultAsync(x => x.SubsidyApprovalId == id, ct);
            }

            if (s is not null)
            {
                s.SupportTypeCode = Req(Text(request, "P81_SUPPORT_TYPE_CODE"), t, "SUPPORT_TYPE_CODE");
                s.ApprovalNo = approvalNo;
                s.ApprovalDate = approvalDate;
                s.ExpiryDate = expiry;
                s.InstallmentSupportNpv = npv;
                s.DownpaymentSupport = dps;
                s.TotalSupport = total;
                s.IsDedicatedAccount = (Text(request, "P81_DEDICATED_ACCOUNT_FLAG") ?? "N") == "Y";
                s.ApprovalStatus = Req(status, t, "APPROVAL_STATUS");
                s.FundNotes = notes;
            }

            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == appId, ct);
            if (a is not null)
            {
                a.EligibilityCertNo = approvalNo;
                a.EligibilityCertDate = approvalDate;
                a.EligibilityCertExpiryDate = expiry;
                a.TotalSubsidyAmount = total;
            }

            await db.SaveChangesAsync(ct);
            if (status == "REJECTED")
                await control.ChangeStatusCheckedAsync(appId ?? 0, "REJECTED", "OPERATIONS", user.UserName, notes ?? "Fund subsidy rejected", ct);
            else
                await control.ChangeStatusCheckedAsync(appId ?? 0, "SUBSIDY_PENDING", "OPERATIONS", user.UserName, "Fund subsidy request/approval recorded", ct);
            return ("Fund subsidy information saved.", s is null ? null : new Dictionary<string, string?> { ["P81_SUBSIDY_APPROVAL_ID"] = Id(s.SubsidyApprovalId) });
        }, ct);
    }
}
