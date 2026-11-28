using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 142, After Submit "Take Current Month Snapshot" (SNAPSHOT): MFF_SERVICING_PKG.take_monthly_snapshot (new in C#).</summary>
public sealed class P0142Snapshot(ICapitraDbContext db, MffServicingPackage servicing, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 142;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SNAPSHOT")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            await servicing.TakeMonthlySnapshotAsync(MffForm.Today(clock), ct);
            return ("Current month snapshot refreshed.", null);
        }, ct);
    }
}
