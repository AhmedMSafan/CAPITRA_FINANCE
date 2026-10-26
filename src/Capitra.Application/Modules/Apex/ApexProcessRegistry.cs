using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Apex;

/// <summary>
/// Finds the ported process for an APEX app, page and process name, including those of wizard-built regions,
/// SQL reports and the LOVS process that fills a page's select lists.
/// </summary>
public sealed class ApexProcessRegistry(
    IEnumerable<IApexProcess> processes,
    IEnumerable<ApexRegion> regions,
    IEnumerable<ApexLov> lovs,
    IEnumerable<ApexSqlReport> sqlReports,
    IEnumerable<ApexSqlLov> sqlLovs,
    IEnumerable<IApexFormHooks> hooks,
    ICapitraDbContext db,
    IApexSqlReader reader,
    ICurrentUser user)
{
    private readonly Dictionary<(int, int, string), IApexProcess> byKey = Build(processes, regions, lovs, sqlReports, sqlLovs, hooks, db, reader, user);

    private static Dictionary<(int, int, string), IApexProcess> Build(
        IEnumerable<IApexProcess> processes, IEnumerable<ApexRegion> regions, IEnumerable<ApexLov> lovs, IEnumerable<ApexSqlReport> sqlReports,
        IEnumerable<ApexSqlLov> sqlLovs, IEnumerable<IApexFormHooks> hooks, ICapitraDbContext db, IApexSqlReader reader, ICurrentUser user)
    {
        var hookOf = hooks.ToDictionary(h => (h.App, h.Page));
        var tableLovs = lovs.ToLookup(l => (l.App, l.Page));
        var queryLovs = sqlLovs.ToLookup(l => (l.App, l.Page));
        var sqlValues = new ApexSqlLovValues(reader);
        var lovPages = tableLovs.Select(g => g.Key).Union(queryLovs.Select(g => g.Key));
        return processes
            .Concat(regions.SelectMany(r => ApexRegionProcess.For(r, db, hookOf.GetValueOrDefault((r.App, r.Page)))))
            .Concat(sqlReports.Select(r => new ApexSqlReportProcess(r, reader, user)))
            .Concat(lovPages.Select(k => new ApexLovProcess(k.App, k.Page, tableLovs[k].ToList(), queryLovs[k].ToList(), db, sqlValues)))
            .ToDictionary(p => (p.App, p.Page, p.Name.ToUpperInvariant()));
    }

    public IApexProcess? Find(int app, int page, string name) =>
        byKey.GetValueOrDefault((app, page, name.ToUpperInvariant()));
}
