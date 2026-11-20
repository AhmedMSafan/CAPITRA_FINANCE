using System.Globalization;
using Capitra.Application.Modules.Adm.Settings;
using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>A licence a Retail Lending value applies to.</summary>
public sealed record LndScopeDefinition(string Code, string Title, string Description, string Icon);

/// <summary>
/// One Retail Lending parameter. Scopes are the licences it applies to. A legal parameter needs the FRA decision it comes from.
/// Suggested is the value found in public summaries of FRA decisions, shown to help the administrator; it is never used until
/// someone enters it and a second user approves it. Source says where the suggestion comes from.
/// </summary>
public sealed record LndParameterDefinition(
    string Code, string Group, string Label, string Help, ModuleSettingType Type, IReadOnlyList<string> Scopes, bool IsLegal,
    decimal? Min = null, decimal? Max = null, string? Unit = null,
    IReadOnlyDictionary<string, string>? Suggested = null, string? Source = null);

/// <summary>
/// Retail Lending parameters (design deck "Capitra Retail Lending", agreed with Ahmed 4 Nov 2026, decision 6: all legal figures are
/// admin settings). The suggested figures come from press summaries of FRA decisions and must be confirmed by compliance against
/// each decision before they are approved. A product may be stricter than a parameter, never looser.
/// </summary>
public static class LndParameterCatalog
{
    public const string Microfinance = "MF";
    public const string ConsumerFinance = "CF";

    public static class Statuses
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
    }

    public static class Codes
    {
        public const string MaxFinancingAmount = "LND.MAX_FINANCING_AMOUNT";
        public const string MaxInstalmentIncomePct = "LND.MAX_INSTALMENT_INCOME_PCT";
        public const string CashMaxAmount = "LND.CASH_MAX_AMOUNT";
        public const string CashPortfolioMaxPct = "LND.CASH_PORTFOLIO_MAX_PCT";
        public const string MaxCustomerCapitalPct = "LND.MAX_CUSTOMER_CAPITAL_PCT";
        public const string GroupMinMembers = "LND.GROUP_MIN_MEMBERS";
        public const string GroupMaxMembers = "LND.GROUP_MAX_MEMBERS";
        public const string OtpAtSigning = "LND.OTP_AT_SIGNING";
        public const string OtpAtUse = "LND.OTP_AT_USE";
        public const string InsuranceMaxAge = "LND.INSURANCE_MAX_AGE";
        public const string IscoreMonthlyDays = "LND.ISCORE_MONTHLY_DAYS";
        public const string RegisteredCollectorsOnly = "LND.REGISTERED_COLLECTORS_ONLY";
        public const string NonAccrualDays = "LND.NON_ACCRUAL_DAYS";
        public const string WriteOffDays = "LND.WRITE_OFF_DAYS";
        public const string Bucket1MaxDays = "LND.BUCKET_1_MAX_DAYS";
        public const string Bucket2MaxDays = "LND.BUCKET_2_MAX_DAYS";
        public const string Bucket3MaxDays = "LND.BUCKET_3_MAX_DAYS";
        public const string Bucket4MaxDays = "LND.BUCKET_4_MAX_DAYS";
        public const string Provision1Pct = "LND.PROVISION_BUCKET_1_PCT";
        public const string Provision2Pct = "LND.PROVISION_BUCKET_2_PCT";
        public const string Provision3Pct = "LND.PROVISION_BUCKET_3_PCT";
        public const string Provision4Pct = "LND.PROVISION_BUCKET_4_PCT";
        public const string Provision5Pct = "LND.PROVISION_BUCKET_5_PCT";
    }

    public static readonly IReadOnlyList<LndScopeDefinition> Scopes =
    [
        new(Microfinance, "Microfinance", "Values for the microfinance licence.", "fa-users"),
        new(ConsumerFinance, "Consumer Finance", "Values for the consumer finance licence.", "fa-shopping-cart"),
    ];

    private static readonly string[] Both = [Microfinance, ConsumerFinance];
    private static readonly string[] MfOnly = [Microfinance];
    private static readonly string[] CfOnly = [ConsumerFinance];

    private static Dictionary<string, string> Cf(string v) => new() { [ConsumerFinance] = v };
    private static Dictionary<string, string> All(string v) => new() { [Microfinance] = v, [ConsumerFinance] = v };

    private const string Summary = "Public summary, to be confirmed against the decision by compliance";

    public static readonly IReadOnlyList<LndParameterDefinition> Parameters =
    [
        new(Codes.MaxFinancingAmount, "Financing limits", "Maximum financing per customer",
            "No microfinance application can be approved above this amount.",
            ModuleSettingType.Number, MfOnly, true, 1, 100_000_000, "EGP",
            new Dictionary<string, string> { [Microfinance] = "292000" }, $"FRA Decision 1 of 2026. {Summary}."),
        new(Codes.MaxInstalmentIncomePct, "Financing limits", "Maximum instalments to income",
            "All the customer's instalments, with the new one, cannot be more than this share of verified monthly income.",
            ModuleSettingType.Number, Both, true, 1, 100, "%", Cf("50"), $"FRA consumer finance guide. {Summary}; no public microfinance figure found."),
        new(Codes.CashMaxAmount, "Financing limits", "Maximum cash financing per customer",
            "A cash advance cannot be above this amount.",
            ModuleSettingType.Number, CfOnly, true, 1, 100_000_000, "EGP", Cf("50000"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.CashPortfolioMaxPct, "Financing limits", "Maximum cash share of the portfolio",
            "Cash financing cannot be more than this share of the consumer finance portfolio.",
            ModuleSettingType.Number, CfOnly, true, 0, 100, "%", Cf("20"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.MaxCustomerCapitalPct, "Financing limits", "Maximum exposure to one customer",
            "Financing to one customer cannot be more than this share of the company's capital.",
            ModuleSettingType.Number, CfOnly, true, 0, 100, "% of capital", Cf("10"), $"FRA consumer finance guide. {Summary}."),

        new(Codes.GroupMinMembers, "Group lending", "Minimum members in a group",
            "A group loan cannot start with fewer members.",
            ModuleSettingType.WholeNumber, MfOnly, true, 2, 100, "members", null, "No public FRA figure found; compliance to provide."),
        new(Codes.GroupMaxMembers, "Group lending", "Maximum members in a group",
            "A group loan cannot have more members.",
            ModuleSettingType.WholeNumber, MfOnly, true, 2, 100, "members", null, "No public FRA figure found; compliance to provide."),

        new(Codes.OtpAtSigning, "Customer protection", "One-time password at signing",
            "Yes: the customer confirms the contract with a code sent to their own phone line, kept as evidence.",
            ModuleSettingType.YesNo, Both, true, Suggested: All("true"), Source: $"FRA Decision 133 of 2026. {Summary}."),
        new(Codes.OtpAtUse, "Customer protection", "One-time password when the financing is used",
            "Yes: the customer confirms again with a code before the money is paid out or the merchant is settled.",
            ModuleSettingType.YesNo, Both, true, Suggested: All("true"), Source: $"FRA Decision 133 of 2026. {Summary}."),
        new(Codes.InsuranceMaxAge, "Customer protection", "Life and disability cover up to age",
            "The customer must be covered by death and disability insurance up to this age.",
            ModuleSettingType.WholeNumber, Both, true, 18, 100, "years", All("65"), $"FRA Decisions 27 and 28 of 2026. {Summary}."),

        new(Codes.IscoreMonthlyDays, "Credit reporting", "Monthly I-Score file due within",
            "The monthly data file is sent to I-Score within this many days of month end. Live events are sent as they happen.",
            ModuleSettingType.WholeNumber, Both, true, 1, 31, "days", All("5"), $"FRA Decisions 174 and 175 of 2026. {Summary}."),

        new(Codes.RegisteredCollectorsOnly, "Collections", "Registered collection firms only",
            "Yes: a collection case can only be given to a firm and collector in the agent registry. Enter it with effective date 22 Jan 2027.",
            ModuleSettingType.YesNo, Both, true, Suggested: All("true"), Source: $"FRA Decision 139 of 2026, from 22 January 2027. {Summary}."),

        new(Codes.NonAccrualDays, "Income recognition", "Stop accruing interest after",
            "From this many days late, interest goes to suspense instead of income, and income already booked moves to suspense.",
            ModuleSettingType.WholeNumber, Both, false, 1, 3650, "days late", null, "Finance policy, to be confirmed with the auditors."),
        new(Codes.WriteOffDays, "Income recognition", "Propose write-off after",
            "From this many days late, end of day lists the loan for a write-off decision through Workflow.",
            ModuleSettingType.WholeNumber, Both, false, 1, 3650, "days late", null, "Finance policy, to be confirmed with the auditors."),

        new(Codes.Bucket1MaxDays, "Provisions", "Bucket 1 ends at",
            "Bucket 1 is from 0 to this many days late.", ModuleSettingType.WholeNumber, Both, true, 0, 3650, "days late", Cf("30"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Bucket2MaxDays, "Provisions", "Bucket 2 ends at",
            "Bucket 2 runs from the day after bucket 1 to this many days late.", ModuleSettingType.WholeNumber, Both, true, 1, 3650, "days late", Cf("90"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Bucket3MaxDays, "Provisions", "Bucket 3 ends at",
            "", ModuleSettingType.WholeNumber, Both, true, 1, 3650, "days late", Cf("120"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Bucket4MaxDays, "Provisions", "Bucket 4 ends at",
            "Bucket 5 is everything later.", ModuleSettingType.WholeNumber, Both, true, 1, 3650, "days late", Cf("180"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Provision1Pct, "Provisions", "Provision rate, bucket 1",
            "Share of the balance provided for while the loan is in this bucket.", ModuleSettingType.Number, Both, true, 0, 100, "%", Cf("1"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Provision2Pct, "Provisions", "Provision rate, bucket 2",
            "", ModuleSettingType.Number, Both, true, 0, 100, "%", Cf("10"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Provision3Pct, "Provisions", "Provision rate, bucket 3",
            "", ModuleSettingType.Number, Both, true, 0, 100, "%", Cf("30"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Provision4Pct, "Provisions", "Provision rate, bucket 4",
            "", ModuleSettingType.Number, Both, true, 0, 100, "%", Cf("50"), $"FRA consumer finance guide. {Summary}."),
        new(Codes.Provision5Pct, "Provisions", "Provision rate, bucket 5",
            "", ModuleSettingType.Number, Both, true, 0, 100, "%", Cf("100"), $"FRA consumer finance guide. {Summary}."),
    ];

    public static LndParameterDefinition? Find(string? code) =>
        Parameters.FirstOrDefault(x => x.Code == code?.Trim().ToUpperInvariant());

    public static LndScopeDefinition? FindScope(string? code) =>
        Scopes.FirstOrDefault(x => x.Code == code?.Trim().ToUpperInvariant());

    /// <summary>The value in its stored form, or the reason it is not valid. The rules are those of Module Settings.</summary>
    public static (string? Value, string? Error) Normalize(LndParameterDefinition d, string? raw) =>
        ModuleSettingCatalog.Normalize(new ModuleSettingDefinition("LND", d.Code, d.Group, d.Label, d.Help, d.Type, "", d.Min, d.Max, d.Unit), raw);

    /// <summary>The value in words, as the page and the history show it.</summary>
    public static string Display(LndParameterDefinition d, string? value) => value is null ? "" : d.Type switch
    {
        ModuleSettingType.YesNo => value == "true" ? "Yes" : "No",
        _ when d.Unit is null => value,
        _ when d.Unit == "%" => $"{value}%",
        _ when d.Unit == "EGP" && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var m) =>
            $"EGP {m.ToString("#,0.##", CultureInfo.InvariantCulture)}",
        _ => $"{value} {d.Unit}",
    };

    /// <summary>
    /// The value in force on a day: the approved value with the latest effective date on or before that day; when two share the date,
    /// the one approved last. Pending, rejected and withdrawn values are never used.
    /// </summary>
    public static LndParameterValue? InForce(IEnumerable<LndParameterValue> rows, string code, string scope, DateOnly day) =>
        rows.Where(x => x.ParameterCode == code && x.ScopeCode == scope && x.StatusCode == Statuses.Approved && x.EffectiveFromDate <= day)
            .OrderByDescending(x => x.EffectiveFromDate).ThenByDescending(x => x.DecidedAt).ThenByDescending(x => x.ParameterValueId)
            .FirstOrDefault();

    /// <summary>The next approved value that starts after the day, if one is already set.</summary>
    public static LndParameterValue? Next(IEnumerable<LndParameterValue> rows, string code, string scope, DateOnly day) =>
        rows.Where(x => x.ParameterCode == code && x.ScopeCode == scope && x.StatusCode == Statuses.Approved && x.EffectiveFromDate > day)
            .OrderBy(x => x.EffectiveFromDate).ThenByDescending(x => x.DecidedAt).ThenByDescending(x => x.ParameterValueId)
            .FirstOrDefault();

    /// <summary>
    /// The go-live check for a licence on a day: each parameter of the licence with no approved value in force, and each pair of values
    /// that do not fit together (buckets must rise, the group minimum cannot pass the maximum). An empty list means the licence can lend.
    /// </summary>
    public static IReadOnlyList<string> Problems(IReadOnlyCollection<LndParameterValue> rows, string scope, DateOnly day)
    {
        var problems = new List<string>();
        var values = new Dictionary<string, decimal>();
        foreach (var d in Parameters.Where(x => x.Scopes.Contains(scope)))
        {
            var v = InForce(rows, d.Code, scope, day);
            if (v is null) problems.Add($"{d.Label}: no approved value.");
            else if (d.Type is ModuleSettingType.WholeNumber or ModuleSettingType.Number)
                values[d.Code] = decimal.Parse(v.ValueText, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        string[] buckets = [Codes.Bucket1MaxDays, Codes.Bucket2MaxDays, Codes.Bucket3MaxDays, Codes.Bucket4MaxDays];
        for (var i = 1; i < buckets.Length; i++)
            if (values.TryGetValue(buckets[i - 1], out var low) && values.TryGetValue(buckets[i], out var high) && high <= low)
                problems.Add($"Bucket {i + 1} must end later than bucket {i}.");
        if (values.TryGetValue(Codes.GroupMinMembers, out var min) && values.TryGetValue(Codes.GroupMaxMembers, out var max) && min > max)
            problems.Add("Minimum members in a group cannot be more than the maximum.");
        if (values.TryGetValue(Codes.NonAccrualDays, out var stop) && values.TryGetValue(Codes.WriteOffDays, out var writeOff) && writeOff < stop)
            problems.Add("Write-off cannot be proposed before interest stops accruing.");
        return problems;
    }
}
