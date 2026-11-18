using Capitra.Application.Common.Exceptions;

namespace Capitra.Application.Modules.Tls;

/// <summary>A rate of the scenario's rate history: in force from its date.</summary>
public sealed record TlsRate(DateOnly EffectiveDate, decimal InterestRatePct, int SequenceNo);

/// <summary>The terms of a scenario, as the calculator works them out (maturity, normalised codes and the rate history included).</summary>
public sealed record TlsTerms(
    decimal PrincipalAmount, DateOnly ContractDate, DateOnly MaturityDate, int TenorValue, string TenorUnit, string ScheduleMode,
    string PrincipalMethod, string InterestPaymentMethod, string DayCountMethod, DateOnly PrincipalPaymentStartDate, DateOnly InterestPaymentStartDate,
    int? PrincipalFrequencyValue, string? PrincipalFrequencyUnit, string? PrincipalDateRule,
    int? InterestFrequencyValue, string? InterestFrequencyUnit, string? InterestDateRule,
    int? CombinedFrequencyValue, string? CombinedFrequencyUnit, string? CombinedDateRule,
    IReadOnlyList<TlsRate> Rates);

/// <summary>A line of a calculated schedule.</summary>
public sealed record TlsLine(
    int InstallmentNo, DateOnly AccrualStartDate, DateOnly AccrualEndDate, DateOnly PaymentDate, decimal OpeningPrincipal, decimal PrincipalAmount,
    decimal InterestAmount, decimal TotalPaymentAmount, decimal ClosingPrincipal, decimal InterestRatePct, int AccrualDays, decimal DayCountFraction);

/// <summary>
/// The FINOVA_FINANCEING Loan Calculator engine (TOOLS > Loan Calculator, LoanScenarioService), ported as it is (Ahmed, 15 Nov 2026):
/// interest on the reducing balance, a rate history, six day counts, combined or separate principal and interest dates, equal
/// principal or equal instalments, and the month-end rules (EOM, NEXT_EOM, EOM_30, NEXT_EOM_30). Money is rounded to 2 decimals,
/// half away from zero. Nothing here reads or writes the database.
/// </summary>
public static class TlsLoanCalculator
{
    public static readonly string[] TenorUnits = ["DAY", "WEEK", "MONTH", "YEAR"];

    // EOMS: a legacy alias of NEXT_EOM, kept so scenarios saved with it still work.
    public static readonly string[] FrequencyUnits = ["DAY", "WEEK", "MONTH", "YEAR", "EOM", "NEXT_EOM", "EOM_30", "NEXT_EOM_30", "EOMS", "MATURITY"];

    public static readonly string[] DayCountMethods = ["ACT_ACT", "ACT_360", "ACT_365", "30_ACT", "30_360", "30_365"];

    public static readonly string[] PrincipalMethods = ["EQUAL_PRINCIPAL", "EQUAL_INSTALLMENT"];

    /// <summary>How a payment date is moved: kept as it is, to the month end, or to the 30th (February: its last day).</summary>
    public static readonly string[] DateRules = ["ACTUAL", "EOM", "DAY_30"];

    /// <summary>The scenario's terms from what the user entered: checked, normalised, with its maturity and rate history.</summary>
    public static TlsTerms Terms(TlsScenarioRequest r)
    {
        Validate(r);
        var mode = N(r.ScheduleMode);
        var separate = mode == "SEPARATE";
        var maturity = AddPeriod(r.ContractDate, r.TenorValue, N(r.TenorUnit), "ACTUAL");
        return new TlsTerms(
            r.PrincipalAmount, r.ContractDate, maturity, r.TenorValue, N(r.TenorUnit), mode,
            separate ? "EQUAL_PRINCIPAL" : N(r.PrincipalMethod), "REGULAR", N(r.DayCountMethod), r.PrincipalPaymentStartDate, r.InterestPaymentStartDate,
            r.PrincipalFrequencyValue, NN(r.PrincipalFrequencyUnit), separate ? "ACTUAL" : NN(r.PrincipalDateRule),
            r.InterestFrequencyValue, NN(r.InterestFrequencyUnit), separate ? "ACTUAL" : NN(r.InterestDateRule),
            r.CombinedFrequencyValue, NN(r.CombinedFrequencyUnit), NN(r.CombinedDateRule),
            Rates(r, maturity));
    }

    /// <summary>
    /// The rate history: the contract date's rate is always the main Interest Rate %, so there is one clear base rate; later rates
    /// within the term follow (the last one entered for a date wins).
    /// </summary>
    private static List<TlsRate> Rates(TlsScenarioRequest r, DateOnly maturity)
    {
        var supplied = (r.Rates ?? []).Where(x => x.EffectiveDate >= r.ContractDate && x.EffectiveDate <= maturity).OrderBy(x => x.EffectiveDate).ToList();
        if (supplied.Any(x => x.InterestRatePct < 0)) throw new BusinessRuleException("Interest rate cannot be negative.");
        var later = supplied.Where(x => x.EffectiveDate > r.ContractDate).GroupBy(x => x.EffectiveDate).Select(g => g.Last()).OrderBy(x => x.EffectiveDate);
        var rates = new List<TlsRate> { new(r.ContractDate, r.InterestRatePct, 1) };
        var sequence = 2;
        foreach (var x in later) rates.Add(new TlsRate(x.EffectiveDate, x.InterestRatePct, sequence++));
        return rates;
    }

    public static List<TlsLine> Generate(TlsTerms s) => s.ScheduleMode == "COMBINED" ? Combined(s) : Separate(s);

    private static List<TlsLine> Combined(TlsTerms s)
    {
        var dates = CombinedDates(s);
        if (dates.Count == 0) throw new BusinessRuleException("The combined repayment schedule contains no payment dates.");
        var result = new List<TlsLine>();
        var balance = s.PrincipalAmount;
        var accrualStart = s.ContractDate;
        var maturityOnly = N(s.CombinedFrequencyUnit ?? string.Empty) == "MATURITY";
        var equalPrincipal = !maturityOnly && s.PrincipalMethod == "EQUAL_PRINCIPAL" ? Round(s.PrincipalAmount / dates.Count) : 0m;
        var equalInstallment = !maturityOnly && s.PrincipalMethod == "EQUAL_INSTALLMENT"
            ? LevelInstallment(s.PrincipalAmount, s.ContractDate, dates, s.DayCountMethod, s.Rates) : 0m;

        for (var index = 0; index < dates.Count; index++)
        {
            var date = dates[index];
            var opening = balance;
            var interest = Interest(opening, accrualStart, date, s.DayCountMethod, s.Rates);
            var principal = maturityOnly ? opening
                : s.PrincipalMethod == "EQUAL_PRINCIPAL" ? Math.Min(equalPrincipal, opening)
                : Math.Min(Math.Max(0m, equalInstallment - interest), opening);
            if (index == dates.Count - 1) principal = opening;
            balance = opening - principal;
            result.Add(Line(s, result.Count + 1, accrualStart, date, date, opening, principal, interest, balance));
            accrualStart = date;
        }
        return result;
    }

    private static List<TlsLine> Separate(TlsTerms s)
    {
        var principalDates = PrincipalDates(s);
        var interestDates = InterestDates(s);
        // two calendars: only identical dates are merged; otherwise each principal-only or interest-only date is its own line
        var all = principalDates.Concat(interestDates).Distinct().Order().ToList();
        if (all.Count == 0) throw new BusinessRuleException("The separate repayment schedule contains no payment dates.");
        if (principalDates.Count == 0) throw new BusinessRuleException("The principal schedule contains no payment dates.");

        var result = new List<TlsLine>();
        var balance = s.PrincipalAmount;
        var lastAccrual = s.ContractDate;
        var periodStart = s.ContractDate;
        var unpaid = 0m;
        var equalPrincipal = Round(s.PrincipalAmount / principalDates.Count);

        foreach (var date in all)
        {
            var opening = balance;
            // each segment accrues on the balance actually outstanding in it
            if (date > lastAccrual && opening > 0) unpaid += Interest(opening, lastAccrual, date, s.DayCountMethod, s.Rates);
            var principalDue = principalDates.Contains(date);
            var interestDue = interestDates.Contains(date);
            var interest = interestDue ? Round(unpaid) : 0m;
            var principal = 0m;
            if (principalDue) principal = date == principalDates[^1] ? opening : Math.Min(equalPrincipal, opening);
            balance = opening - principal;
            result.Add(Line(s, result.Count + 1, interestDue ? periodStart : date, date, date, opening, principal, interest, balance));
            if (interestDue) { unpaid = 0; periodStart = date; }
            lastAccrual = date;
        }
        return result;
    }

    private static List<DateOnly> CombinedDates(TlsTerms s)
    {
        var unit = N(s.CombinedFrequencyUnit ?? "MONTH");
        if (unit == "MATURITY") return [s.MaturityDate];
        var dates = Dates(s.PrincipalPaymentStartDate, s.MaturityDate, s.CombinedFrequencyValue ?? 0, unit, s.CombinedDateRule ?? "ACTUAL", s.ContractDate);
        EnsureMaturity(dates, s.MaturityDate);
        return dates;
    }

    private static List<DateOnly> PrincipalDates(TlsTerms s)
    {
        var unit = N(s.PrincipalFrequencyUnit ?? "MONTH");
        if (unit == "MATURITY") return [s.MaturityDate];
        var dates = Dates(s.PrincipalPaymentStartDate, s.MaturityDate, s.PrincipalFrequencyValue ?? 0, unit, s.PrincipalDateRule ?? "ACTUAL", s.ContractDate);
        EnsureMaturity(dates, s.MaturityDate);
        return dates;
    }

    private static List<DateOnly> InterestDates(TlsTerms s)
    {
        var unit = N(s.InterestFrequencyUnit ?? "MONTH");
        if (unit == "MATURITY") return [s.MaturityDate];
        var first = s.InterestPaymentStartDate;
        var dates = Dates(first, s.MaturityDate, s.InterestFrequencyValue ?? 0, unit, s.InterestDateRule ?? "ACTUAL", s.ContractDate);
        // the interest start date is the first interest payment and is never lost
        if (first <= s.MaturityDate && !dates.Contains(first)) dates.Insert(0, first);
        EnsureMaturity(dates, s.MaturityDate);
        return [.. dates.Distinct().Order()];
    }

    private static TlsLine Line(TlsTerms s, int no, DateOnly accrualStart, DateOnly accrualEnd, DateOnly paymentDate, decimal opening, decimal principal,
        decimal interest, decimal closing) =>
        new(no, accrualStart, accrualEnd, paymentDate, Round(opening), Round(principal), Round(interest), Round(principal + interest), Round(closing),
            RateAt(s.Rates, paymentDate), AccrualDays(accrualStart, accrualEnd, s.DayCountMethod),
            accrualEnd > accrualStart ? Dcf(accrualStart, accrualEnd, s.DayCountMethod) : 0m);

    /// <summary>B(i) = B(i-1)·(1 + f(i)) − P, so B(n) = Principal·A − P·C, and the level payment P = Principal·A / C.</summary>
    private static decimal LevelInstallment(decimal principal, DateOnly contractDate, List<DateOnly> dates, string method, IReadOnlyList<TlsRate> rates)
    {
        if (dates.Count == 0) throw new BusinessRuleException("At least one payment date is required.");
        decimal a = 1, c = 0;
        var start = contractDate;
        foreach (var date in dates)
        {
            var f = InterestFactor(start, date, method, rates);
            a *= 1 + f;
            c = c * (1 + f) + 1;
            start = date;
        }
        if (c <= 0) throw new BusinessRuleException("Unable to calculate equal installment amount.");
        return Round(principal * a / c);
    }

    private static decimal Interest(decimal principal, DateOnly start, DateOnly end, string method, IReadOnlyList<TlsRate> rates) =>
        end <= start || principal <= 0 ? 0 : Round(principal * InterestFactor(start, end, method, rates));

    private static decimal InterestFactor(DateOnly start, DateOnly end, string method, IReadOnlyList<TlsRate> rates)
    {
        if (end <= start) return 0;
        decimal factor = 0;
        var cursor = start;
        foreach (var change in rates.Where(x => x.EffectiveDate > start && x.EffectiveDate < end).OrderBy(x => x.EffectiveDate))
        {
            factor += RateAt(rates, cursor) / 100m * Dcf(cursor, change.EffectiveDate, method);
            cursor = change.EffectiveDate;
        }
        return factor + RateAt(rates, cursor) / 100m * Dcf(cursor, end, method);
    }

    private static decimal RateAt(IReadOnlyList<TlsRate> rates, DateOnly date) =>
        rates.Where(x => x.EffectiveDate <= date).OrderByDescending(x => x.EffectiveDate).Select(x => x.InterestRatePct).FirstOrDefault();

    /// <summary>The year fraction between two dates on one of <see cref="DayCountMethods"/> (0 when end is not after start).</summary>
    public static decimal YearFraction(DateOnly start, DateOnly end, string method) => Dcf(start, end, method);

    private static decimal Dcf(DateOnly start, DateOnly end, string method)
    {
        if (end <= start) return 0;
        var days = (decimal)(end.DayNumber - start.DayNumber);
        return N(method) switch
        {
            "ACT_ACT" => ByYear(start, end, (a, b) => b.DayNumber - a.DayNumber),
            "ACT_360" => days / 360m,
            "ACT_365" => days / 365m,
            "30_ACT" => ByYear(start, end, ThirtyDays),
            "30_360" => ThirtyDays(start, end) / 360m,
            "30_365" => ThirtyDays(start, end) / 365m,
            _ => throw new BusinessRuleException($"Unsupported day count method: {method}"),
        };
    }

    /// <summary>ACT/ACT and 30/ACT: each calendar year's part over that year's 365 or 366 days.</summary>
    private static decimal ByYear(DateOnly start, DateOnly end, Func<DateOnly, DateOnly, decimal> days)
    {
        decimal result = 0;
        var cursor = start;
        while (cursor < end)
        {
            var nextYear = new DateOnly(cursor.Year + 1, 1, 1);
            var segmentEnd = nextYear < end ? nextYear : end;
            result += days(cursor, segmentEnd) / (DateTime.IsLeapYear(cursor.Year) ? 366m : 365m);
            cursor = segmentEnd;
        }
        return result;
    }

    private static decimal ThirtyDays(DateOnly start, DateOnly end) =>
        (end.Year - start.Year) * 360m + (end.Month - start.Month) * 30m + (Math.Min(end.Day, 30) - Math.Min(start.Day, 30));

    private static List<DateOnly> Dates(DateOnly first, DateOnly maturity, int value, string unit, string rule, DateOnly contractDate)
    {
        if (value <= 0) throw new BusinessRuleException("Frequency value must be greater than zero.");
        var u = N(unit);
        // MONTH and YEAR count from the first date, not from the previous one: a date cut short by a short month (31-Jan to 28-Feb)
        // must not stay on the 28th for the rest of the term. When the first date is itself a cut-short month end (a 31-Jan contract
        // paying first on 28-Feb), the contract's day is the day to keep (31-Mar, 30-Apr, ...), so the last date meets the maturity.
        var anchored = u is "MONTH" or "YEAR";
        var day = first.Day;
        if (anchored && first == EndOfMonth(first) && contractDate.Day > first.Day) day = contractDate.Day;
        var dates = new List<DateOnly>();
        var date = Rule(first, rule);
        var k = 0;
        while (date <= maturity)
        {
            dates.Add(date);
            k++;
            var next = anchored ? Rule(OnDay(first.AddMonths((u == "YEAR" ? 12 : 1) * value * k), day), rule) : AddPeriod(date, value, unit, rule);
            if (next <= date) throw new BusinessRuleException("Invalid frequency.");
            date = next;
        }
        return [.. dates.Distinct().Order()];
    }

    /// <summary>The given day of the date's month, or the month's last day when the month is shorter.</summary>
    private static DateOnly OnDay(DateOnly d, int day) => new(d.Year, d.Month, Math.Min(day, DateTime.DaysInMonth(d.Year, d.Month)));

    public static DateOnly AddPeriod(DateOnly date, int value, string unit, string rule)
    {
        var u = N(unit);
        switch (u)
        {
            case "MATURITY": throw new BusinessRuleException("MATURITY is a single payment date, not a repeating period.");
            // Start Current Month: with 1, a payment on 01-Oct is followed by 31-Oct; once on month end, it moves by the months asked
            case "EOM": return FromCurrentPeriod(date, value, EndOfMonth);
            // Start Next Month: with 1, a payment on 01-Oct is followed by 30-Nov (EOMS is its legacy alias)
            case "NEXT_EOM" or "EOMS": return EndOfMonth(date.AddMonths(value));
            // the 30th (February: its last day), from the current or the next month
            case "EOM_30": return FromCurrentPeriod(date, value, Day30OrMonthEnd);
            case "NEXT_EOM_30": return Day30OrMonthEnd(date.AddMonths(value));
        }
        var next = u switch
        {
            "DAY" => date.AddDays(value),
            "WEEK" => date.AddDays(7 * value),
            "MONTH" => date.AddMonths(value),
            "YEAR" => date.AddYears(value),
            _ => throw new BusinessRuleException($"Unsupported period unit: {unit}"),
        };
        return Rule(next, rule);
    }

    private static DateOnly FromCurrentPeriod(DateOnly date, int value, Func<DateOnly, DateOnly> day)
    {
        var candidate = day(date.AddMonths(value - 1));
        return candidate <= date ? day(date.AddMonths(value)) : candidate;
    }

    private static DateOnly EndOfMonth(DateOnly d) => new(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));

    private static DateOnly Day30OrMonthEnd(DateOnly d) => new(d.Year, d.Month, Math.Min(30, DateTime.DaysInMonth(d.Year, d.Month)));

    private static DateOnly Rule(DateOnly date, string? rule) => NN(rule) switch
    {
        "EOM" => EndOfMonth(date),
        "DAY_30" => Day30OrMonthEnd(date),
        null or "ACTUAL" => date,
        _ => throw new BusinessRuleException($"Unsupported date rule: {rule}"),
    };

    private static void Validate(TlsScenarioRequest r)
    {
        if (r.PrincipalAmount <= 0) throw new BusinessRuleException("Principal amount must be greater than zero.");
        if (r.TenorValue <= 0) throw new BusinessRuleException("Tenor must be greater than zero.");
        if (!TenorUnits.Contains(N(r.TenorUnit))) throw new BusinessRuleException("Unsupported tenor unit.");
        if (r.InterestRatePct < 0) throw new BusinessRuleException("Interest rate cannot be negative.");
        if (!DayCountMethods.Contains(N(r.DayCountMethod))) throw new BusinessRuleException("Unsupported day count method.");
        var mode = N(r.ScheduleMode);
        if (mode is not ("SEPARATE" or "COMBINED")) throw new BusinessRuleException("Unsupported schedule mode.");
        var method = mode == "SEPARATE" ? "EQUAL_PRINCIPAL" : N(r.PrincipalMethod);
        if (!PrincipalMethods.Contains(method)) throw new BusinessRuleException("Unsupported principal method.");
        var maturity = AddPeriod(r.ContractDate, r.TenorValue, N(r.TenorUnit), "ACTUAL");
        if (r.PrincipalPaymentStartDate < r.ContractDate || r.InterestPaymentStartDate < r.ContractDate)
            throw new BusinessRuleException("Payment start dates cannot be before contract date.");
        if (r.PrincipalPaymentStartDate > maturity || r.InterestPaymentStartDate > maturity)
            throw new BusinessRuleException("Payment start dates cannot be after maturity date.");
        foreach (var rule in new[] { r.PrincipalDateRule, r.InterestDateRule, r.CombinedDateRule })
            if (NN(rule) is { } x && !DateRules.Contains(x)) throw new BusinessRuleException($"Unsupported date rule: {rule}");
        if (mode == "COMBINED")
        {
            Frequency("Combined repayment", r.CombinedFrequencyValue, r.CombinedFrequencyUnit, allowMaturity: true);
            return;
        }
        Frequency("Principal", r.PrincipalFrequencyValue, r.PrincipalFrequencyUnit, allowMaturity: false);
        Frequency("Interest", r.InterestFrequencyValue, r.InterestFrequencyUnit, allowMaturity: false);
    }

    private static void Frequency(string label, int? value, string? unit, bool allowMaturity)
    {
        var u = NN(unit) ?? throw new BusinessRuleException($"{label} frequency unit is required.");
        if (!FrequencyUnits.Contains(u)) throw new BusinessRuleException($"Unsupported {label.ToLowerInvariant()} frequency unit.");
        if (u == "MATURITY")
        {
            if (!allowMaturity) throw new BusinessRuleException($"{label} frequency does not support MATURITY.");
            return;
        }
        if (value is not > 0) throw new BusinessRuleException($"{label} frequency value must be greater than zero.");
    }

    private static void EnsureMaturity(List<DateOnly> dates, DateOnly maturity)
    {
        if (!dates.Contains(maturity)) dates.Add(maturity);
        dates.Sort();
    }

    public static int AccrualDays(DateOnly start, DateOnly end, string method)
    {
        if (end <= start) return 0;
        return N(method) switch
        {
            "ACT_ACT" or "ACT_360" or "ACT_365" => end.DayNumber - start.DayNumber,
            "30_ACT" or "30_360" or "30_365" => (int)ThirtyDays(start, end),
            _ => throw new BusinessRuleException($"Unsupported day count method: {method}"),
        };
    }

    private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static string N(string? v) => (v ?? string.Empty).Trim().ToUpperInvariant();

    private static string? NN(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim().ToUpperInvariant();
}
