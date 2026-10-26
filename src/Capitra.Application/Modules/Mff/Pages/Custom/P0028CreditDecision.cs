using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 28, After Submit "Save Credit Decision" (SAVE_DECISION).</summary>
public sealed class P0028CreditDecision(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 28;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_DECISION")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_CREDIT_DECISIONS";
            var appId = Req(Long(request, "P28_APPLICATION_ID"), t, "APPLICATION_ID");
            var code = Req(Text(request, "P28_DECISION_CODE"), t, "DECISION_CODE");
            var approved = Number(request, "P28_APPROVED_FINANCE_AMOUNT");
            var notes = Text(request, "P28_NOTES");
            var conditions = Text(request, "P28_CONDITIONS_TEXT");
            db.Add(new MffCreditDecision
            {
                ApplicationId = appId,
                DecisionCode = code,
                DecisionDate = Req(Date(request, "P28_DECISION_DATE"), t, "DECISION_DATE"),
                ApprovedFinanceAmount = approved,
                ApprovedTermMonths = Int(request, "P28_APPROVED_TERM_MONTHS"),
                ApprovedRatePct = Number(request, "P28_APPROVED_RATE_PCT"),
                ConditionsText = conditions,
                DecidedBy = user.UserName,
                CommitteeReference = Text(request, "P28_COMMITTEE_REFERENCE"),
                Notes = notes,
            });
            var next = code switch
            {
                "APPROVE" => "CREDIT_APPROVED",
                "CONDITIONAL" => "CONDITIONAL_APPROVAL",
                "REJECT" => "REJECTED",
                _ => "CREDIT_STUDY",
            };
            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == appId, ct);
            if (a is not null && code is "APPROVE" or "CONDITIONAL")
                a.ApprovedFinanceAmount = approved;
            else if (a is not null && code == "REJECT")
                a.RejectionReason = notes ?? conditions;
            await db.SaveChangesAsync(ct);
            await control.ChangeStatusCheckedAsync(appId, next, "CREDIT_APPROVER", user.UserName, "Credit decision recorded", ct);
            return ("Credit decision saved.", null);
        }, ct);
    }
}
