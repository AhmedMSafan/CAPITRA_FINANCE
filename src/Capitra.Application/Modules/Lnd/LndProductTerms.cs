using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// The rules of a Retail Lending product version (design deck, agreed with Ahmed 4 Nov 2026): the choices each field may take,
/// the checks before a version can be approved, and the version in force on a day. Late fees (Ahmed, 4 Nov 2026): no fee while the
/// instalment is paid in full within the grace days; once they pass, the fee is charged from day 1; no cap.
/// </summary>
public static class LndProductTerms
{
    public static class BusinessLines
    {
        public const string Microfinance = "MICROFINANCE";
        public const string ConsumerFinance = "CONSUMER_FINANCE";
    }

    public static class Statuses
    {
        public const string Draft = "DRAFT";
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
    }

    /// <summary>The business lines of Retail Lending and the licence (parameter scope) of each.</summary>
    public static readonly IReadOnlyDictionary<string, string> Licences = new Dictionary<string, string>
    {
        [BusinessLines.Microfinance] = LndParameterCatalog.Microfinance,
        [BusinessLines.ConsumerFinance] = LndParameterCatalog.ConsumerFinance,
    };

    public static readonly IReadOnlyList<CrmOptionDto> Frequencies = [new("WEEKLY", "Weekly"), new("FORTNIGHTLY", "Every two weeks"), new("MONTHLY", "Monthly")];

    public static readonly IReadOnlyList<CrmOptionDto> InterestMethods =
        [new("DECLINING", "Declining balance (equal instalments)"), new("FLAT", "Flat (on the original amount)")];

    public static readonly IReadOnlyList<CrmOptionDto> DayCounts = [new("ACT_365", "Actual / 365"), new("ACT_360", "Actual / 360"), new("THIRTY_360", "30 / 360")];

    public static readonly IReadOnlyList<CrmOptionDto> LateFeeTypes =
        [new("NONE", "No late fee"), new("FLAT", "Fixed amount per late instalment"), new("PCT_ONCE", "Percentage, once"), new("PCT_DAILY", "Percentage per day")];

    public static readonly IReadOnlyList<CrmOptionDto> LateFeeBases = [new("OVERDUE_INSTALMENT", "Overdue instalment"), new("OVERDUE_PRINCIPAL", "Overdue principal")];

    /// <summary>What a payment can be applied to, in the agreed default order (Ahmed, 4 Nov 2026).</summary>
    public static readonly IReadOnlyList<CrmOptionDto> PaymentParts =
        [new("LATE_FEE", "Late fee"), new("CHARGES", "Charges"), new("INSURANCE", "Insurance"), new("INTEREST", "Interest"), new("PRINCIPAL", "Principal")];

    /// <summary>What a part prepayment changes (design deck: a product setting).</summary>
    public static readonly IReadOnlyList<CrmOptionDto> PrepaymentRecalcs =
        [new("REDUCE_INSTALMENT", "Lower instalment, same term"), new("REDUCE_TERM", "Shorter term, same instalment")];

    public static string DefaultPaymentOrder => string.Join(",", PaymentParts.Select(x => x.Value));

    public static IReadOnlyList<string> Parts(string? order) =>
        [.. (order ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUpperInvariant())];

    /// <summary>A new version's terms before anything is entered: the agreed payment order and no late fee.</summary>
    public static LndProductVersion Defaults(long productId) => new()
    {
        ProductId = productId, StatusCode = Statuses.Draft, MinAmount = 1_000, MaxAmount = 50_000, MinTermMonths = 3, MaxTermMonths = 36,
        RepaymentFrequencyCode = "MONTHLY", InterestMethodCode = "DECLINING", AnnualRatePct = 0, DayCountCode = "ACT_365",
        LateFeeGraceDays = 5, LateFeeTypeCode = "NONE", LateFeeBaseCode = "OVERDUE_INSTALMENT", PaymentOrder = DefaultPaymentOrder,
    };

    /// <summary>The checks of the terms themselves, by field. An empty result means the terms can be sent for approval.</summary>
    public static Dictionary<string, string[]> Check(LndProductVersion v)
    {
        var e = new Dictionary<string, string[]>();
        void Add(string field, string error) => e[field] = [.. (e.GetValueOrDefault(field) ?? []), error];
        static bool In(IReadOnlyList<CrmOptionDto> list, string? code) => list.Any(x => x.Value == code);

        if (v.MinAmount <= 0) Add("minAmount", "The minimum amount must be more than zero.");
        if (v.MaxAmount < v.MinAmount) Add("maxAmount", "The maximum amount cannot be less than the minimum.");
        if (v.MinTermMonths < 1) Add("minTermMonths", "The shortest term is at least 1 month.");
        if (v.MaxTermMonths < v.MinTermMonths) Add("maxTermMonths", "The longest term cannot be shorter than the shortest.");
        if (v.MaxTermMonths > 120) Add("maxTermMonths", "The longest term is at most 120 months.");
        if (!In(Frequencies, v.RepaymentFrequencyCode)) Add("repaymentFrequencyCode", "Choose how often the customer pays.");
        if (!In(InterestMethods, v.InterestMethodCode)) Add("interestMethodCode", "Choose the interest method.");
        if (v.AnnualRatePct is < 0 or > 100) Add("annualRatePct", "Enter a yearly rate from 0 to 100%.");
        if (!In(DayCounts, v.DayCountCode)) Add("dayCountCode", "Choose the day count.");
        if (v.LateFeeGraceDays is < 0 or > 90) Add("lateFeeGraceDays", "Enter grace days from 0 to 90.");
        if (!In(LateFeeTypes, v.LateFeeTypeCode)) Add("lateFeeTypeCode", "Choose the late fee type.");
        if (v.LateFeeTypeCode == "FLAT" && v.LateFeeFlatAmount is not > 0) Add("lateFeeFlatAmount", "Enter the late fee amount.");
        if (v.LateFeeTypeCode is "PCT_ONCE" or "PCT_DAILY")
        {
            if (v.LateFeeRatePct is not (> 0 and <= 100)) Add("lateFeeRatePct", "Enter the late fee rate, more than 0 and up to 100%.");
            if (!In(LateFeeBases, v.LateFeeBaseCode)) Add("lateFeeBaseCode", "Choose what the late fee is calculated on.");
        }
        var parts = Parts(v.PaymentOrder);
        if (parts.Count != PaymentParts.Count || parts.Distinct().Count() != parts.Count || parts.Any(p => !In(PaymentParts, p)))
            Add("paymentOrder", "The payment order must list late fee, charges, insurance, interest and principal, each once.");
        if (!In(PrepaymentRecalcs, v.PrepaymentRecalcCode)) Add("prepaymentRecalcCode", "Choose what a part prepayment changes.");
        if (v.PrepaymentFeePct is < 0 or > 100) Add("prepaymentFeePct", "Enter a prepayment fee from 0 to 100%.");
        if (v.EarlySettlementFeePct is < 0 or > 100) Add("earlySettlementFeePct", "Enter an early settlement fee from 0 to 100%.");
        return e;
    }

    /// <summary>
    /// The checks against the licence's parameters in force on the version's start date: a product can be stricter than the law,
    /// never looser. A parameter the product depends on that has no approved value stops the approval.
    /// </summary>
    public static IReadOnlyList<string> CheckAgainstParameters(LndProductVersion v, string licence, decimal? maxFinancing, decimal? maxCash, bool isCash)
    {
        var problems = new List<string>();
        if (licence == LndParameterCatalog.Microfinance)
        {
            if (maxFinancing is null) problems.Add("Maximum financing per customer has no approved value for microfinance. Approve it in Lending Parameters first.");
            else if (v.MaxAmount > maxFinancing) problems.Add($"The maximum amount is above the microfinance limit of EGP {maxFinancing:#,0.##}.");
        }
        if (licence == LndParameterCatalog.ConsumerFinance && isCash)
        {
            if (maxCash is null) problems.Add("Maximum cash financing per customer has no approved value. Approve it in Lending Parameters first.");
            else if (v.MaxAmount > maxCash) problems.Add($"The maximum amount is above the cash financing limit of EGP {maxCash:#,0.##}.");
        }
        return problems;
    }

    /// <summary>The version in force on a day: the approved version with the latest start date on or before it (the highest number on a tie).</summary>
    public static LndProductVersion? InForce(IEnumerable<LndProductVersion> versions, long productId, DateOnly day) =>
        versions.Where(x => x.ProductId == productId && x.StatusCode == Statuses.Approved && x.EffectiveFromDate <= day)
            .OrderByDescending(x => x.EffectiveFromDate).ThenByDescending(x => x.VersionNo).FirstOrDefault();
}
