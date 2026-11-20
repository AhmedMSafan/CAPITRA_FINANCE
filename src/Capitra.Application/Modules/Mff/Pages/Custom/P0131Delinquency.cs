using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>
/// App 110 page 131, After Submit "Load Delinquency Case" (LOAD_CASE), "Refresh Delinquency" (REFRESH_DQ) and
/// "Save Collection Action" (SAVE_COLLECTION): MFF_SERVICING_PKG.refresh_delinquency (new in C#).
/// </summary>
public sealed class P0131Delinquency(ICapitraDbContext db, IApexSqlReader reader, MffServicingPackage servicing, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 131;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD_CASE" or "REFRESH_DQ" or "SAVE_COLLECTION"))
            return Task.FromResult<object>(new { ok = true });
        var loanId = Long(request, "P131_LOAN_ID");
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD_CASE")
                return (string.Empty, loanId is null ? null : await LoadAsync(reader, 131, request, ct));
            if (loanId is null)
                throw new ApexProcessException(req == "REFRESH_DQ" ? "ORA-20072: Select a loan first." : "ORA-20073: Select a loan first.");
            if (req == "REFRESH_DQ")
            {
                await servicing.RefreshDelinquencyAsync(loanId.Value, Today(clock), ct);
                return ("Delinquency position refreshed.", null);
            }

            var id = Long(request, "P131_DELINQUENCY_ID");
            if (id is null)
            {
                await servicing.RefreshDelinquencyAsync(loanId.Value, Today(clock), ct);
                id = await db.Set<MffDelinquency>().Where(x => x.LoanId == loanId).OrderByDescending(x => x.DelinquencyId)
                    .Select(x => (long?)x.DelinquencyId).FirstOrDefaultAsync(ct);
            }

            if (id is not null && await db.Set<MffDelinquency>().FirstOrDefaultAsync(x => x.DelinquencyId == id, ct) is { } d)
            {
                d.StageCode = Text(request, "P131_STAGE_CODE") ?? d.StageCode;
                d.DelinquencyStatus = Text(request, "P131_STATUS") ?? d.DelinquencyStatus;
                d.CollectionActionAr = Text(request, "P131_COLLECTION_ACTION");
                await db.SaveChangesAsync(ct);
            }

            return ("Collection action saved.", id is null ? null : new Dictionary<string, string?> { ["P131_DELINQUENCY_ID"] = Id(id.Value) });
        }, ct);
    }
}
