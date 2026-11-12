using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// Part prepayment and full early settlement of a retail loan (design deck: "part prepayment needs a product setting: shorter term or
/// lower instalment, and any prepayment fee; full early settlement needs the quote rule: principal, interest accrued to the date,
/// unpaid dues and any fee"). Both are on today's date and use the same model:
/// <list type="bullet">
/// <item>Interest to date: the current period's interest earned from its start (the previous due date, or the payout) to today, evenly,
/// and never less than end of day accrued already.</item>
/// <item>Early settlement = dues not paid (instalments due by today, with late fees) + interest to date + principal not yet due + the
/// settlement fee on that principal. The interest of later periods is not owed. The loan closes.</item>
/// <item>Part prepayment = interest to date + the principal paid early + the prepayment fee on it, with nothing due unpaid. What is left
/// is rebuilt from today over the remaining instalments (lower instalment) or with the same instalment over fewer (shorter term); the
/// first rebuilt instalment carries only the interest from today to its due date.</item>
/// </list>
/// </summary>
public static class LndPrepayment
{
    public const string Settle = "SETTLE";
    public const string Prepay = "PREPAY";

    public sealed record Quote(
        string Kind, DateOnly Date, decimal DuesAmount, decimal InterestToDate, decimal PrincipalAmount, decimal FeeAmount, decimal TotalAmount,
        decimal PrincipalNotDue, int? CurrentInstallmentNo, IReadOnlyList<LndScheduleLine> NewSchedule, string? Problem);

    /// <summary>The instalment whose period runs today (the first one due after today) and the day its period started.</summary>
    public static (LndInstallment? Current, DateOnly Start) CurrentPeriod(IReadOnlyList<LndInstallment> schedule, DateOnly disbursement, DateOnly day)
    {
        var start = disbursement;
        foreach (var i in schedule.OrderBy(x => x.InstallmentNo))
        {
            if (i.DueDate > day) return (i, start);
            start = i.DueDate;
        }
        return (null, start);
    }

    /// <summary>The current period's interest earned by today and not paid: even over the period, at least what end of day accrued.</summary>
    public static decimal InterestToDate(LndInstallment current, DateOnly start, DateOnly day)
    {
        var days = current.DueDate.DayNumber - start.DayNumber;
        var share = day <= start || days <= 0 ? 0 : R(current.InterestDueAmount * (day.DayNumber - start.DayNumber) / days);
        return Math.Max(0, Math.Min(current.InterestDueAmount, Math.Max(share, current.InterestAccruedAmount)) - current.InterestPaidAmount);
    }

    public static decimal Dues(IEnumerable<LndInstallment> schedule, DateOnly day) =>
        schedule.Where(i => i.DueDate <= day).Sum(i => LndReceiptService.Owed(i).Values.Sum());

    public static decimal PrincipalNotDue(IEnumerable<LndInstallment> schedule, DateOnly day) =>
        schedule.Where(i => i.DueDate > day).Sum(i => i.PrincipalDueAmount - i.PrincipalPaidAmount);

    /// <summary>The quote of a settlement (principal ignored) or of a part prepayment of that principal, on the day.</summary>
    public static Quote Make(LndLoan loan, IReadOnlyList<LndInstallment> schedule, string kind, decimal? principal, DateOnly day)
    {
        var ordered = schedule.OrderBy(x => x.InstallmentNo).ToList();
        var dues = Dues(ordered, day);
        var notDue = PrincipalNotDue(ordered, day);
        var (current, start) = CurrentPeriod(ordered, loan.DisbursementDate, day);
        var toDate = current is null ? 0 : InterestToDate(current, start, day);
        Quote Fail(string problem) => new(kind, day, dues, toDate, 0, 0, 0, notDue, current?.InstallmentNo, [], problem);

        if (loan.StatusCode != "ACTIVE") return Fail("Only an active loan can be prepaid or settled.");
        if (!loan.IsPrepaymentAllowed) return Fail("The loan's product does not allow paying early.");
        if (kind == Settle)
        {
            var fee = R(notDue * loan.EarlySettlementFeePct / 100m);
            return new Quote(kind, day, dues, toDate, notDue, fee, dues + toDate + notDue + fee, notDue, current?.InstallmentNo, [], null);
        }
        if (kind != Prepay) return Fail("Choose a part prepayment or an early settlement.");
        if (current is null) return Fail("No instalment is left to prepay. Use early settlement.");
        if (dues > 0) return Fail($"{dues:#,0.00} is due and not paid. Receive it first, or settle the loan.");
        var p = R(principal ?? 0);
        if (p <= 0) return Fail("Enter the principal to pay early.");
        if (p >= notDue) return Fail($"That is all the principal left ({notDue:#,0.00}). Use early settlement.");
        var prepayFee = R(p * loan.PrepaymentFeePct / 100m);
        var rest = ordered.Where(i => i.DueDate > day).ToList();
        var lines = Rebuild(loan, rest, notDue - p, day, start);
        return new Quote(kind, day, 0, toDate, p, prepayFee, toDate + p + prepayFee, notDue, current.InstallmentNo, lines, null);
    }

    /// <summary>
    /// The instalments left after a part prepayment, from the current one: the same count and a lower instalment, or the same
    /// instalment and fewer. The first one's interest is only for the days from today to its due date.
    /// </summary>
    public static IReadOnlyList<LndScheduleLine> Rebuild(LndLoan loan, IReadOnlyList<LndInstallment> rest, decimal balance, DateOnly day, DateOnly start)
    {
        var n = rest.Count;
        var first = rest[0].DueDate;
        var count = n;
        if (loan.PrepaymentRecalcCode == "REDUCE_TERM")
        {
            if (loan.InterestMethodCode == "FLAT")
            {
                var each = rest[0].PrincipalDueAmount - rest[0].PrincipalPaidAmount;
                count = each <= 0 ? n : Math.Clamp((int)Math.Ceiling(balance / each), 1, n);
            }
            else
            {
                var target = loan.InstallmentAmount;
                count = 1;
                while (count < n && LndSchedule.BuildCount(balance, count, loan.RepaymentFrequencyCode, loan.InterestMethodCode, loan.AnnualRatePct, first)[0].Total > target)
                    count++;
            }
        }
        var lines = LndSchedule.BuildCount(balance, count, loan.RepaymentFrequencyCode, loan.InterestMethodCode, loan.AnnualRatePct, first).ToList();
        var period = first.DayNumber - start.DayNumber;
        if (period > 0 && day > start)
            lines[0] = lines[0] with { Interest = R(lines[0].Interest * (first.DayNumber - day.DayNumber) / period) };
        return lines;
    }

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
