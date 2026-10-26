using Capitra.Domain.Modules.Adm;

namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>
/// Every lookup served by the generic lookup API and screen: global lists that no APEX page maintains.
/// The App 105 reference data (pages 11 to 40) are copied APEX pages.
/// Add one entry here when a module adds a lookup table.
/// </summary>
public static class LookupRegistry
{
    public static IReadOnlyDictionary<string, LookupDefinition> All { get; } =
        new[]
        {
            LookupDefinition.For<AdmLkpCountry>("country", "ADM", "Countries", "") with
            {
                CodeMaxLength = 2,
                NameMaxLength = 100,
                ShowDisplayOrder = false,
            },
            // Organisation lists used by the general ledger (APEX App 120 set them up by script).
            LookupDefinition.For<AdmLkpLegalEntity>("legal-entity", "GL", "Legal Entities", "") with
            {
                NameMaxLength = 240,
                Fields =
                [
                    new("baseCurrencyCode", "Base Currency", LookupFieldType.Text, MaxLength: 3),
                    new("countryCode", "Country", LookupFieldType.Lookup, LookupName: "country"),
                ],
            },
            LookupDefinition.For<AdmLkpBusinessLine>("business-line", "GL", "Business Lines", "") with { NameMaxLength = 240 },
        }.ToDictionary(x => x.LookupName, StringComparer.OrdinalIgnoreCase);
}
