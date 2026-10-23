using Capitra.Domain.Modules.Adm;

namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>
/// Every lookup served by the generic lookup API (api/adm/lookups/{lookupName}).
/// Add one line here when a module adds a lookup table.
/// </summary>
public static class LookupRegistry
{
    public static IReadOnlyDictionary<string, LookupDefinition> All { get; } =
        new[]
        {
            LookupDefinition.For<AdmLkpCurrency>("currency"),
            LookupDefinition.For<AdmLkpCountry>("country"),
            LookupDefinition.For<AdmLkpRiskLevel>("risk-level"),
        }.ToDictionary(x => x.LookupName, StringComparer.OrdinalIgnoreCase);
}
