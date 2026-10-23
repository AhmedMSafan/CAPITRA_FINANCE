using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>
/// Every lookup served by the generic lookup API and screen, taken from the APEX App 105 reference data pages (3, 11 to 40).
/// Add one entry here when a module adds a lookup table.
/// </summary>
public static class LookupRegistry
{
    public static IReadOnlyDictionary<string, LookupDefinition> All { get; } =
        new[]
        {
            LookupDefinition.For<MtgLkpPortfolioStatus>("portfolio-status", "MTG", "Portfolio Status", "11/12"),
            LookupDefinition.For<AdmLkpDocumentGroup>("document-group", "ADM", "Document Groups", "13/14"),
            LookupDefinition.For<AdmLkpDocumentType>("document-type", "ADM", "Document Types", "15/16") with
            {
                NameMaxLength = 100,
                DisplayOrderLabel = "Sort Order",
                ShowDescription = true,
                Fields =
                [
                    new("documentGroupCode", "Document Group", LookupFieldType.Lookup, LookupName: "document-group"),
                    new("documentCategory", "Document Category", LookupFieldType.Text, MaxLength: 50),
                    new("allowedExtensions", "Allowed Extensions", LookupFieldType.Text, MaxLength: 200),
                    new("maxFileSizeMb", "Max File Size (MB)", LookupFieldType.Number, Min: 1, Max: 1024),
                    new("isMandatory", "Mandatory", LookupFieldType.Boolean),
                    new("isExpiryRequired", "Requires Expiry Date", LookupFieldType.Boolean),
                ],
            },
            LookupDefinition.For<MtgLkpInquiryType>("inquiry-type", "MTG", "Inquiry Types", "17/18"),
            LookupDefinition.For<MtgLkpDdCheckType>("dd-check-type", "MTG", "Due Diligence Check Types", "19/20") with
            {
                NameMaxLength = 300,
                Fields = [new("isMandatory", "Mandatory", LookupFieldType.Boolean)],
            },
            LookupDefinition.For<MtgLkpValuationType>("valuation-type", "MTG", "Valuation Types", "21/22"),
            LookupDefinition.For<MtgLkpApprovalStep>("approval-step", "MTG", "Approval Steps", "23/24") with
            {
                DisplayOrderLabel = "Sequence No.",
                DisplayOrderRequired = true,
            },
            LookupDefinition.For<AdmLkpDecisionType>("decision-type", "ADM", "Decision Types", "25/26") with { ShowDisplayOrder = false },
            LookupDefinition.For<AdmLkpGuaranteeType>("guarantee-type", "ADM", "Guarantee Types", "27/28") with
            {
                NameMaxLength = 300,
                Fields = [new("isMandatory", "Mandatory", LookupFieldType.Boolean)],
            },
            LookupDefinition.For<AdmLkpInsuranceType>("insurance-type", "ADM", "Insurance Types", "29/30") with { ShowDisplayOrder = false },
            LookupDefinition.For<AdmLkpRiskLevel>("risk-level", "ADM", "Risk Levels", "31/32") with
            {
                NameMaxLength = 100,
                DisplayOrderLabel = "Sort Order",
            },
            LookupDefinition.For<AdmLkpPaymentStatus>("payment-status", "ADM", "Payment Statuses", "33/34") with
            {
                NameMaxLength = 100,
                ShowDisplayOrder = false,
            },
            LookupDefinition.For<AdmLkpChequeStatus>("cheque-status", "ADM", "Cheque Statuses", "35/36") with
            {
                NameMaxLength = 100,
                ShowDisplayOrder = false,
            },
            LookupDefinition.For<AdmLkpCurrency>("currency", "ADM", "Currencies", "37/38") with
            {
                CodeMaxLength = 3,
                NameMaxLength = 100,
                ShowDisplayOrder = false,
                Fields = [new("decimalPlaces", "Decimal Places", LookupFieldType.Number, Required: true, Min: 0, Max: 6)],
            },
            LookupDefinition.For<MtgLkpUnitType>("unit-type", "MTG", "Unit Types", "39/40") with
            {
                NameMaxLength = 100,
                ShowDisplayOrder = false,
            },
        }.ToDictionary(x => x.LookupName, StringComparer.OrdinalIgnoreCase);
}
