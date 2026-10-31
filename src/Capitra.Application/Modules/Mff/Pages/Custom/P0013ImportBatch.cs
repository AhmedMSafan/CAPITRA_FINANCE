using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 13, After Submit "Load Batch" (LOAD) and "Process Batch" (PROCESS): MFF_IMPORT_PKG.process_batch (new in C#).</summary>
public sealed class P0013ImportBatch(ICapitraDbContext db, IApexSqlReader reader, MffImportPackage import) : IApexProcess
{
    public int App => 110;

    public int Page => 13;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD" or "PROCESS"))
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD")
                return (string.Empty, await LoadAsync(reader, 13, request, ct));
            var (ok, err) = await import.ProcessBatchAsync(Long(request, "P13_BATCH_ID") ?? 0, ct);
            return ("Batch processed. Applications created: " + ok + ", errors: " + err, await LoadAsync(reader, 13, request, ct));
        }, ct);
    }
}
