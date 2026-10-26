using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 23, After Submit "Add Income Source" (ADD_INCOME).</summary>
public sealed class P0023IncomeSources(ICapitraDbContext db) : IApexProcess
{
    public int App => 110;

    public int Page => 23;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "ADD_INCOME")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var partyId = Long(request, "P23_APPLICATION_PARTY_ID")
                ?? throw new ApexProcessException("ORA-20061: Applicant party record is missing.");
            const string t = "MFF_INCOME_SOURCES";
            db.Add(new MffIncomeSource
            {
                ApplicationPartyId = partyId,
                IncomeTypeCode = Req(Text(request, "P23_INCOME_TYPE_CODE"), t, "INCOME_TYPE_CODE"),
                EmployerName = Text(request, "P23_EMPLOYER_NAME"),
                MonthlyNetIncome = Req(Number(request, "P23_MONTHLY_NET_INCOME"), t, "MONTHLY_NET_INCOME"),
                AnnualNetIncome = Number(request, "P23_ANNUAL_NET_INCOME"),
                IsVerified = (Text(request, "P23_VERIFIED_FLAG") ?? "N") == "Y",
                VerificationDate = Date(request, "P23_VERIFICATION_DATE"),
                SourceReference = Text(request, "P23_SOURCE_REFERENCE"),
                Notes = Text(request, "P23_NOTES"),
            });
            await db.SaveChangesAsync(ct);

            var id = Long(request, "P23_APPLICATION_ID");
            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == id, ct);
            if (a is not null)
            {
                a.MonthlyIncomeTotal = await (from i in db.Set<MffIncomeSource>()
                                              join ap in db.Set<MffApplicationParty>() on i.ApplicationPartyId equals ap.ApplicationPartyId
                                              where ap.ApplicationId == a.ApplicationId && ap.IsIncomeIncluded
                                              select (decimal?)i.MonthlyNetIncome).SumAsync(ct) ?? 0;
                await db.SaveChangesAsync(ct);
            }

            return ("Income source added.", null);
        }, ct);
    }
}
