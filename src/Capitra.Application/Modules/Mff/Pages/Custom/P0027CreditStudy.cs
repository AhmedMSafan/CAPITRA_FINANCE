using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 27, After Submit "Save Credit Study" (SAVE_STUDY).</summary>
public sealed class P0027CreditStudy(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 27;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_STUDY")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_CREDIT_STUDIES";
            var appId = Req(Long(request, "P27_APPLICATION_ID"), t, "APPLICATION_ID");
            var obligations = Number(request, "P27_MONTHLY_OBLIGATIONS_AMOUNT");
            var verified = Number(request, "P27_VERIFIED_INCOME_AMOUNT");
            var disposable = Number(request, "P27_DISPOSABLE_INCOME_AMOUNT") ?? (verified ?? 0) - (obligations ?? 0);
            db.Add(new MffCreditStudy
            {
                ApplicationId = appId,
                AnalystUser = user.UserName,
                StudyDate = Req(Date(request, "P27_STUDY_DATE"), t, "STUDY_DATE"),
                VerifiedIncomeAmount = verified,
                MonthlyObligationsAmount = obligations,
                DisposableIncomeAmount = disposable,
                ProposedFinanceAmount = Number(request, "P27_PROPOSED_FINANCE_AMOUNT"),
                ProposedTermMonths = Int(request, "P27_PROPOSED_TERM_MONTHS"),
                ProposedRatePct = Number(request, "P27_PROPOSED_RATE_PCT"),
                InstallmentIncomePct = Number(request, "P27_INSTALLMENT_INCOME_PCT"),
                LtvPct = Number(request, "P27_LTV_PCT"),
                RecommendationCode = Text(request, "P27_RECOMMENDATION_CODE"),
                Notes = Text(request, "P27_NOTES"),
            });
            await db.SaveChangesAsync(ct);
            await control.ChangeStatusCheckedAsync(appId, "CREDIT_STUDY", "CREDIT_ANALYST", user.UserName, "Credit study saved", ct);
            return ("Credit study saved.", new Dictionary<string, string?> { ["P27_DISPOSABLE_INCOME_AMOUNT"] = disposable.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        }, ct);
    }
}
