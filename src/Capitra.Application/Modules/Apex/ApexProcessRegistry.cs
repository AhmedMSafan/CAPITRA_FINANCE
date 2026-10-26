namespace Capitra.Application.Modules.Apex;

/// <summary>Finds the ported process for an APEX app, page and process name.</summary>
public sealed class ApexProcessRegistry(IEnumerable<IApexProcess> processes)
{
    private readonly Dictionary<(int, int, string), IApexProcess> byKey =
        processes.ToDictionary(p => (p.App, p.Page, p.Name.ToUpperInvariant()));

    public IApexProcess? Find(int app, int page, string name) =>
        byKey.GetValueOrDefault((app, page, name.ToUpperInvariant()));
}
