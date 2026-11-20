namespace Capitra.Application.Modules.Lnd;

/// <summary>One instalment of a schedule: its due date, principal and interest, and the principal before and after it.</summary>
public sealed record LndScheduleLine(int InstallmentNo, DateOnly DueDate, decimal OpeningPrincipal, decimal Principal, decimal Interest, decimal ClosingPrincipal)
{
    public decimal Total => Principal + Interest;
}

/// <summary>
/// The repayment schedule of a retail loan (design deck: one schedule engine for every lending product).
/// DECLINING: equal instalments; each period's interest is the balance times the yearly rate divided by the periods in a year.
/// FLAT: interest is the amount times the yearly rate times the years of the term, spread equally, with the principal, over the
/// instalments. Every amount is in 2 decimals; the last instalment takes the rounding, so the principal always adds up to the amount.
/// </summary>
public static class LndSchedule
{
    public static int PeriodsPerYear(string frequency) => frequency switch
    {
        "WEEKLY" => 52,
        "FORTNIGHTLY" => 26,
        "MONTHLY" => 12,
        _ => throw new ArgumentException($"Unknown repayment frequency {frequency}.", nameof(frequency)),
    };

    /// <summary>How many instalments a term in months makes: one a month, 26 a year every two weeks, 52 a year weekly.</summary>
    public static int InstallmentCount(int termMonths, string frequency) =>
        Math.Max(1, (int)Math.Round(termMonths * PeriodsPerYear(frequency) / 12m, MidpointRounding.AwayFromZero));

    /// <summary>The due date of instalment n (1-based): the first due date, then every 7 or 14 days, or the same day of each month.</summary>
    public static DateOnly DueDate(DateOnly first, int n, string frequency) => frequency switch
    {
        "WEEKLY" => first.AddDays(7 * (n - 1)),
        "FORTNIGHTLY" => first.AddDays(14 * (n - 1)),
        _ => first.AddMonths(n - 1),
    };

    /// <summary>The usual first due date: one period after the disbursement.</summary>
    public static DateOnly DefaultFirstDueDate(DateOnly disbursement, string frequency) => frequency switch
    {
        "WEEKLY" => disbursement.AddDays(7),
        "FORTNIGHTLY" => disbursement.AddDays(14),
        _ => disbursement.AddMonths(1),
    };

    public static IReadOnlyList<LndScheduleLine> Build(
        decimal amount, int termMonths, string frequency, string interestMethod, decimal annualRatePct, DateOnly firstDueDate)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be more than zero.");
        var n = InstallmentCount(termMonths, frequency);
        var lines = new List<LndScheduleLine>(n);
        var balance = amount;

        if (interestMethod == "FLAT")
        {
            var totalInterest = R(amount * annualRatePct / 100m * termMonths / 12m);
            var principalEach = R(amount / n);
            var interestEach = R(totalInterest / n);
            var interestLeft = totalInterest;
            for (var i = 1; i <= n; i++)
            {
                var last = i == n;
                var principal = last ? balance : principalEach;
                var interest = last ? interestLeft : interestEach;
                lines.Add(new LndScheduleLine(i, DueDate(firstDueDate, i, frequency), balance, principal, interest, balance - principal));
                balance -= principal;
                interestLeft -= interest;
            }
            return lines;
        }

        if (interestMethod != "DECLINING") throw new ArgumentException($"Unknown interest method {interestMethod}.", nameof(interestMethod));
        var rate = annualRatePct / 100m / PeriodsPerYear(frequency);
        var instalment = rate == 0 ? R(amount / n) : R(amount * rate / (1 - 1 / Power(1 + rate, n)));
        for (var i = 1; i <= n; i++)
        {
            var interest = R(balance * rate);
            var principal = i == n ? balance : Math.Min(balance, instalment - interest);
            lines.Add(new LndScheduleLine(i, DueDate(firstDueDate, i, frequency), balance, principal, interest, balance - principal));
            balance -= principal;
        }
        return lines;
    }

    /// <summary>
    /// A schedule of a given number of instalments (used to rebuild what is left after a part prepayment): DECLINING with equal
    /// instalments, FLAT with the interest of n periods on the amount spread equally.
    /// </summary>
    public static IReadOnlyList<LndScheduleLine> BuildCount(decimal amount, int n, string frequency, string interestMethod, decimal annualRatePct, DateOnly firstDueDate)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be more than zero.");
        if (n < 1) throw new ArgumentOutOfRangeException(nameof(n), "At least one instalment.");
        var lines = new List<LndScheduleLine>(n);
        var balance = amount;
        if (interestMethod == "FLAT")
        {
            var totalInterest = R(amount * annualRatePct / 100m * n / PeriodsPerYear(frequency));
            var principalEach = R(amount / n);
            var interestEach = R(totalInterest / n);
            var interestLeft = totalInterest;
            for (var i = 1; i <= n; i++)
            {
                var last = i == n;
                var principal = last ? balance : principalEach;
                var interest = last ? interestLeft : interestEach;
                lines.Add(new LndScheduleLine(i, DueDate(firstDueDate, i, frequency), balance, principal, interest, balance - principal));
                balance -= principal;
                interestLeft -= interest;
            }
            return lines;
        }
        var rate = annualRatePct / 100m / PeriodsPerYear(frequency);
        var instalment = rate == 0 ? R(amount / n) : R(amount * rate / (1 - 1 / Power(1 + rate, n)));
        for (var i = 1; i <= n; i++)
        {
            var interest = R(balance * rate);
            var principal = i == n ? balance : Math.Min(balance, instalment - interest);
            lines.Add(new LndScheduleLine(i, DueDate(firstDueDate, i, frequency), balance, principal, interest, balance - principal));
            balance -= principal;
        }
        return lines;
    }

    private static decimal R(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal Power(decimal value, int n)
    {
        var result = 1m;
        for (var i = 0; i < n; i++) result *= value;
        return result;
    }
}
