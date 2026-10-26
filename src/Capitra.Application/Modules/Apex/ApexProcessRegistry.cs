using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Apex;

/// <summary>Finds the ported process for an APEX app, page and process name, including those of wizard-built regions.</summary>
public sealed class ApexProcessRegistry(
    IEnumerable<IApexProcess> processes, IEnumerable<ApexRegion> regions, IEnumerable<ApexLov> lovs, ICapitraDbContext db)
{
    private readonly Dictionary<(int, int, string), IApexProcess> byKey =
        processes.Concat(regions.SelectMany(r => ApexRegionProcess.For(r, db)))
            .Concat(lovs.GroupBy(l => (l.App, l.Page)).Select(g => new ApexLovProcess(g.Key.App, g.Key.Page, g.ToList(), db)))
            .ToDictionary(p => (p.App, p.Page, p.Name.ToUpperInvariant()));

    public IApexProcess? Find(int app, int page, string name) =>
        byKey.GetValueOrDefault((app, page, name.ToUpperInvariant()));
}
