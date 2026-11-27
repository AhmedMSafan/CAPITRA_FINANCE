using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// The rules of a Retail Lending product version (design deck, agreed with Ahmed 4 Nov 2026): the choices each field may take,
/// the checks before a version can be approved, and the version in force on a day. Late fees (Ahmed, 4 Nov 2026): no fee while the
/// instalment is paid in full within the grace days; once they pass, the fee is charged from day 1; no cap.
/// </summary>
/// <summary>One changed field of the lending terms: as it was and as it is.</summary>
public sealed record LndTermChangeDto(string Field, string OldValue, string NewValue);

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
        ProductId = productId, StatusCode = Statuses.Draft,
        RepaymentFrequencyCode = "MONTHLY", InterestMethodCode = "DECLINING", AnnualRatePct = 0, DayCountCode = "ACT_365",
        LateFeeGraceDays = 0, LateFeeTypeCode = "NONE", LateFeeBaseCode = "OVERDUE_INSTALMENT", PaymentOrder = DefaultPaymentOrder,
    };

    /// <summary>The checks of the terms themselves, by field. An empty result means the terms can be sent for approval.</summary>
    public static Dictionary<string, string[]> Check(LndProductVersion v)
    {
        var e = new Dictionary<string, string[]>();
        void Add(string field, string error) => e[field] = [.. (e.GetValueOrDefault(field) ?? []), error];
        static bool In(IReadOnlyList<CrmOptionDto> list, string? code) => list.Any(x => x.Value == code);

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
    /// What a change of terms changes (Ahmed, 27 Nov 2026: one set of terms, every change in the audit trail): each field that differs
    /// between the terms before and after, as the screen words them. Nothing before: every field is new.
    /// </summary>
    public static List<LndTermChangeDto> Diff(LndProductVersion? before, LndProductVersion after)
    {
        static string Label(IReadOnlyList<CrmOptionDto> list, string? code) => list.FirstOrDefault(x => x.Value == code)?.Label ?? code ?? "—";
        static string LateFee(LndProductVersion v) => v.LateFeeTypeCode switch
        {
            "NONE" => "No late fee",
            "FLAT" => $"EGP {v.LateFeeFlatAmount:#,0.##} per late instalment",
            "PCT_DAILY" => $"{v.LateFeeRatePct:0.####}% a day on {Label(LateFeeBases, v.LateFeeBaseCode).ToLowerInvariant()}",
            _ => $"{v.LateFeeRatePct:0.####}% once on {Label(LateFeeBases, v.LateFeeBaseCode).ToLowerInvariant()}",
        };
        static string Order(string? o) => string.Join(" › ", Parts(o).Select(p => Label(PaymentParts, p)));
        static string YesNo(bool b) => b ? "Yes" : "No";
        var fields = new (string Field, Func<LndProductVersion, string> Of)[]
        {
            ("Repayment", v => Label(Frequencies, v.RepaymentFrequencyCode)),
            ("Interest method", v => Label(InterestMethods, v.InterestMethodCode)),
            ("Yearly rate", v => $"{v.AnnualRatePct:0.####}%"),
            ("Day count", v => Label(DayCounts, v.DayCountCode)),
            ("Late fee", LateFee),
            ("Grace days", v => v.LateFeeTypeCode == "NONE" ? "—" : v.LateFeeGraceDays.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Payment order", v => Order(v.PaymentOrder)),
            ("Insurance required", v => YesNo(v.IsInsuranceRequired)),
            ("Paying early", v => v.IsPrepaymentAllowed ? $"Allowed · {Label(PrepaymentRecalcs, v.PrepaymentRecalcCode).ToLowerInvariant()}" : "Not allowed"),
            ("Part prepayment fee", v => $"{v.PrepaymentFeePct:0.####}%"),
            ("Early settlement fee", v => $"{v.EarlySettlementFeePct:0.####}%"),
        };
        return [.. fields.Select(f => (f.Field, Old: before is null ? "—" : f.Of(before), New: f.Of(after)))
            .Where(x => before is null || x.Old != x.New).Select(x => new LndTermChangeDto(x.Field, x.Old, x.New))];
    }

    /// <summary>The version in force on a day: the approved version with the latest start date on or before it (the highest number on a tie).</summary>
    public static LndProductVersion? InForce(IEnumerable<LndProductVersion> versions, long productId, DateOnly day) =>
        versions.Where(x => x.ProductId == productId && x.StatusCode == Statuses.Approved && x.EffectiveFromDate <= day)
            .OrderByDescending(x => x.EffectiveFromDate).ThenByDescending(x => x.VersionNo).FirstOrDefault();
}
