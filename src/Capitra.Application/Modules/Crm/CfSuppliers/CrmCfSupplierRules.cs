using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;

namespace Capitra.Application.Modules.Crm.CfSuppliers;

/// <summary>
/// The rules of a Consumer Finance supplier application (Ahmed, 25 Nov 2026), without the database so they can be tested:
/// what it needs to be submitted, and what each workflow step needs before it can be accepted. A return (REJECT) needs only a comment.
/// </summary>
public static class CrmCfSupplierRules
{
    public const string WorkflowCode = "CF_SUPPLIER_APPROVAL";

    public const string DocumentCheck = "CFS_DOCUMENT_CHECK";
    public const string Compliance = "CFS_COMPLIANCE";
    public const string SiteVisit = "CFS_SITE_VISIT";
    public const string RiskReview = "CFS_RISK_REVIEW";
    public const string ManagerApproval = "CFS_MANAGER_APPROVAL";
    public const string CommitteeApproval = "CFS_COMMITTEE_APPROVAL";
    public const string Contract = "CFS_CONTRACT";

    public static readonly string[] Steps = [DocumentCheck, Compliance, SiteVisit, RiskReview, ManagerApproval, CommitteeApproval, Contract];

    public static readonly string[] PartyTypes = ["OWNER", "MANAGER", "SIGNATORY"];
    public static readonly string[] RiskRatings = ["LOW", "MEDIUM", "HIGH"];
    public static readonly string[] Collectors = ["SUPPLIER", "COMPANY"];
    public static readonly string[] ScreeningResults = ["CLEAR", "MATCH"];

    public const string SiteVisitReport = "SITE_VISIT_REPORT";
    public const string SignedAgreement = "SIGNED_AGREEMENT";

    public static readonly string[] DocumentTypes =
        ["COMMERCIAL_REGISTER", "TAX_CARD", "ACTIVITY_LICENCE", "BANK_LETTER", "ID_DOCUMENT", SiteVisitReport, SignedAgreement, "OTHER"];

    /// <summary>The agreement terms: categories known, percentages and days in range, dates in order.</summary>
    public static Dictionary<string, string[]> CheckTerms(CrmCfSupplierApplication a, IReadOnlyCollection<string> knownCategories)
    {
        var e = new Dictionary<string, string[]>();
        var codes = Codes(a.GoodsCategoryCodes);
        if (codes.Count == 0) e["goodsCategoryCodes"] = ["Choose at least one goods category the supplier sells."];
        else if (codes.FirstOrDefault(c => !knownCategories.Contains(c)) is { } unknown) e["goodsCategoryCodes"] = [$"Goods category {unknown} is not an active category."];
        if (a.MinDownPaymentPct is < 0 or >= 100) e["minDownPaymentPct"] = ["The minimum down payment is from 0% to less than 100%."];
        if (!Collectors.Contains(a.DownPaymentCollectorCode)) e["downPaymentCollectorCode"] = ["Choose who takes the down payment."];
        if (a.CommissionPct is < 0 or >= 100) e["commissionPct"] = ["The commission is from 0% to less than 100%."];
        if (a.PaymentDays is < 0 or > 365) e["paymentDays"] = ["Payment days are from 0 to 365."];
        if (a.QuotationValidityDays is < 1 or > 90) e["quotationValidityDays"] = ["A quotation holds its price from 1 to 90 days."];
        if (a.DeliveryDays is < 1 or > 90) e["deliveryDays"] = ["Delivery days are from 1 to 90."];
        if (a.ValidToDate is { } to && to <= a.ValidFromDate) e["validToDate"] = ["The agreement must end after it starts."];
        return e;
    }

    /// <summary>What an application needs to be submitted, besides its terms (documents can come at any time).</summary>
    public static List<string> CheckSubmit(IReadOnlyCollection<CrmCfSupplierParty> parties)
    {
        var p = new List<string>();
        if (!parties.Any(x => x.PartyTypeCode == "OWNER")) p.Add("Add the supplier's owners.");
        if (!parties.Any(x => x.PartyTypeCode == "SIGNATORY")) p.Add("Add who signs for the supplier (authorised signatory).");
        if (parties.Where(x => x.PartyTypeCode == "OWNER").Sum(x => x.OwnershipPct ?? 0) > 100) p.Add("The owners' shares add up to more than 100%.");
        return p;
    }

    /// <summary>
    /// What a step needs before it is accepted. <paramref name="documentTypes"/> are the types of the application's documents;
    /// <paramref name="bankAccountStatus"/> is the status of the account chosen to pay the supplier, or null when none is chosen.
    /// </summary>
    public static List<string> CheckAccept(
        string stepCode, CrmCfSupplierApplication a, IReadOnlyCollection<CrmCfSupplierParty> parties, IReadOnlyCollection<string> documentTypes,
        string? bankAccountStatus, bool hasRegisterAndTaxNumbers, DateOnly today)
    {
        var p = new List<string>();
        switch (stepCode)
        {
            case DocumentCheck:
                if (!hasRegisterAndTaxNumbers) p.Add("The customer record needs the commercial register number and the tax number.");
                break;
            case Compliance:
                if (!RiskRatings.Contains(a.RiskRatingCode ?? "")) p.Add("Set the supplier's risk rating.");
                if (parties.Any(x => x.ScreeningResultCode is null)) p.Add("Record the screening result of every owner, manager and signatory.");
                if (parties.Any(x => x.ScreeningResultCode == "MATCH")) p.Add("A party matched a sanctions or blacklist entry: return the application.");
                break;
            case SiteVisit:
                if (a.SiteVisitDate is null) p.Add("Enter the date of the site visit.");
                else if (a.SiteVisitDate > today) p.Add("The site visit date cannot be in the future.");
                if (!documentTypes.Contains(SiteVisitReport)) p.Add("Add the site visit report to the documents (type: Site visit report).");
                break;
            case Contract:
                if (!documentTypes.Contains(SignedAgreement)) p.Add("Add the signed supplier agreement to the documents (type: Signed agreement).");
                if (bankAccountStatus is null) p.Add("Choose the bank account the supplier is paid to.");
                else if (bankAccountStatus != CrmClientBankAccount.Verified) p.Add("The supplier's bank account must be verified first (Customer 360 > Bank Accounts).");
                break;
        }
        return p;
    }

    /// <summary>Segregation of duties, by person: the person who entered the application does none of its workflow steps.</summary>
    public static void CheckPerson(string enteredByPerson, string actorPerson)
    {
        if (string.Equals(enteredByPerson, actorPerson, StringComparison.Ordinal))
            throw new BusinessRuleException("Segregation of duties: the person who entered the supplier application cannot do its workflow steps.");
    }

    public static List<string> Codes(string? csv) =>
        [.. (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUpperInvariant()).Distinct()];
}
