using Capitra.Domain.Modules.Lnd;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// The day's rules of Retail Lending end of day (design deck; Ahmed, 4 Nov 2026), worked out for one loan and one business day.
/// <list type="bullet">
/// <item>Accrual: each instalment's interest is earned evenly over its period (from the previous due date, or the payout, to its due
/// date); the due date takes the rounding, so the interest accrued always equals the schedule's.</item>
/// <item>Days late: the days since the due date of the oldest instalment not paid in full.</item>
/// <item>Late fee: none while an instalment is paid in full within the grace days; once they pass, it is charged from day 1 (a daily
/// fee for every day so far, then each day; a fixed or one-off fee once). No cap.</item>
/// <item>Non-accrual: at the non-accrual days the interest and late fees accrued and not paid move from income to suspense, and new
/// ones go to suspense; suspense follows what is still unpaid (a payment releases its part to income); when the loan is no longer
/// late (days late 0), or is closed, all of it goes back to income.</item>
/// <item>Write-off: a loan at or beyond the write-off days is proposed for write-off (the decision itself comes with its own step).</item>
/// </list>
/// </summary>
public static class LndEod
{
    /// <summary>What one loan's day gave, for GL and for the run's totals.</summary>
    public sealed record Result(
        decimal InterestAccrued, decimal InterestSuspended, decimal LateFeeCharged, decimal LateFeeSuspended,
        decimal InterestToSuspense, decimal InterestFromSuspense, decimal LateFeeToSuspense, decimal LateFeeFromSuspense,
        int DaysLate, bool WentNonAccrual, bool Cured, bool NewlyProposedForWriteOff)
    {
        public IReadOnlyDictionary<string, decimal> Amounts => new Dictionary<string, decimal>
        {
            ["INTEREST_ACCRUED"] = InterestAccrued, ["INTEREST_SUSPENDED"] = InterestSuspended,
            ["LATE_FEE_CHARGED"] = LateFeeCharged, ["LATE_FEE_SUSPENDED"] = LateFeeSuspended,
            ["INTEREST_TO_SUSPENSE"] = InterestToSuspense, ["INTEREST_FROM_SUSPENSE"] = InterestFromSuspense,
            ["LATE_FEE_TO_SUSPENSE"] = LateFeeToSuspense, ["LATE_FEE_FROM_SUSPENSE"] = LateFeeFromSuspense,
        };
    }

    /// <summary>
    /// Runs one business day on one loan and its schedule (changed in place): accrual, days late, late fees, non-accrual and suspense,
    /// cure, and the write-off proposal. An active loan gets the full day; a closed loan only has its last interest accrued and its
    /// suspense released.
    /// </summary>
    public static Result Process(LndLoan loan, IReadOnlyList<LndInstallment> schedule, DateOnly day, int nonAccrualDays, int writeOffDays)
    {
        decimal accrued = 0, suspended = 0, feeCharged = 0, feeSuspended = 0, toSusp = 0, fromSusp = 0, feeToSusp = 0, feeFromSusp = 0;
        bool wentNa = false, cured = false;
        var ordered = schedule.OrderBy(x => x.InstallmentNo).ToList();

        if (loan.StatusCode != "ACTIVE")
        {
            // closed: the interest not accrued yet was paid, so it is income; suspense goes back to income
            foreach (var i in ordered.Where(x => x.InterestAccruedAmount < x.InterestDueAmount))
            {
                accrued += i.InterestDueAmount - i.InterestAccruedAmount;
                i.InterestAccruedAmount = i.InterestDueAmount;
            }
            fromSusp = loan.InterestSuspenseAmount;
            feeFromSusp = loan.LateFeeSuspenseAmount;
            var wasNaClosed = loan.IsNonAccrual;
            loan.InterestSuspenseAmount = 0;
            loan.LateFeeSuspenseAmount = 0;
            loan.IsNonAccrual = false;
            loan.DaysLate = 0;
            loan.LastEodDate = day;
            return new Result(accrued, 0, 0, 0, 0, fromSusp, 0, feeFromSusp, 0, false, wasNaClosed, false);
        }

        var late = DaysLate(ordered, day);
        var wasNa = loan.IsNonAccrual;
        var na = wasNa ? late > 0 : late >= nonAccrualDays;

        if (!wasNa && na)
        {
            wentNa = true;
            toSusp = UnpaidAccrued(ordered);
            feeToSusp = UnpaidLateFee(ordered);
            loan.InterestSuspenseAmount += toSusp;
            loan.LateFeeSuspenseAmount += feeToSusp;
        }

        var (inst, interest) = DailyInterest(ordered, loan.DisbursementDate, day);
        if (inst is not null && interest > 0)
        {
            inst.InterestAccruedAmount += interest;
            if (na) { suspended = interest; loan.InterestSuspenseAmount += interest; }
            else accrued = interest;
        }

        foreach (var i in ordered)
        {
            var (fee, days) = LateFee(loan, i, day);
            i.LateFeeDays = days;
            if (fee <= 0) continue;
            i.LateFeeDueAmount += fee;
            if (na) { feeSuspended += fee; loan.LateFeeSuspenseAmount += fee; }
            else feeCharged += fee;
        }

        if (wasNa && !na)
        {
            cured = true;
            fromSusp = loan.InterestSuspenseAmount;
            feeFromSusp = loan.LateFeeSuspenseAmount;
            loan.InterestSuspenseAmount = 0;
            loan.LateFeeSuspenseAmount = 0;
        }
        else if (na)
        {
            // suspense holds what is still unpaid: what was paid since is income, and what is owed again (a receipt reversed) goes back
            var interestGap = loan.InterestSuspenseAmount - UnpaidAccrued(ordered);
            var feeGap = loan.LateFeeSuspenseAmount - UnpaidLateFee(ordered);
            if (interestGap > 0) fromSusp = interestGap; else toSusp += -interestGap;
            if (feeGap > 0) feeFromSusp = feeGap; else feeToSusp += -feeGap;
            loan.InterestSuspenseAmount -= interestGap;
            loan.LateFeeSuspenseAmount -= feeGap;
        }

        var writeOff = late >= writeOffDays;
        var proposed = writeOff && !loan.IsWriteOffProposed;
        loan.IsWriteOffProposed = writeOff;
        loan.IsNonAccrual = na;
        loan.DaysLate = late;
        loan.LastEodDate = day;
        return new Result(accrued, suspended, feeCharged, feeSuspended, toSusp, fromSusp, feeToSusp, feeFromSusp, late, wentNa, cured, proposed);
    }

    /// <summary>Interest accrued and not paid yet (interest paid before end of day accrued it counts as nothing unpaid).</summary>
    public static decimal UnpaidAccrued(IEnumerable<LndInstallment> schedule) =>
        schedule.Sum(i => Math.Max(0, i.InterestAccruedAmount - i.InterestPaidAmount));

    public static decimal UnpaidLateFee(IEnumerable<LndInstallment> schedule) => schedule.Sum(i => i.LateFeeDueAmount - i.LateFeePaidAmount);

    /// <summary>The interest earned on the day, and the instalment it belongs to; nothing before the payout or after maturity.</summary>
    public static (LndInstallment? Installment, decimal Amount) DailyInterest(IReadOnlyList<LndInstallment> schedule, DateOnly disbursementDate, DateOnly day)
    {
        var previous = disbursementDate;
        foreach (var i in schedule.OrderBy(x => x.InstallmentNo))
        {
            if (day > previous && day <= i.DueDate)
            {
                var rest = i.InterestDueAmount - i.InterestAccruedAmount;
                if (rest <= 0) return (i, 0);
                if (day == i.DueDate) return (i, rest);
                var days = i.DueDate.DayNumber - previous.DayNumber;
                return (i, Math.Min(rest, Math.Round(i.InterestDueAmount / days, 2, MidpointRounding.AwayFromZero)));
            }
            previous = i.DueDate;
        }
        return (null, 0);
    }

    public static decimal Owed(LndInstallment i, bool withLateFee = true) =>
        i.PrincipalDueAmount - i.PrincipalPaidAmount + i.InterestDueAmount - i.InterestPaidAmount + i.ChargesDueAmount - i.ChargesPaidAmount
        + i.InsuranceDueAmount - i.InsurancePaidAmount + (withLateFee ? i.LateFeeDueAmount - i.LateFeePaidAmount : 0);

    /// <summary>Days late of one instalment on the day: 0 when paid in full or not yet due.</summary>
    public static int DaysLate(LndInstallment i, DateOnly day) => Owed(i, false) > 0 && day > i.DueDate ? day.DayNumber - i.DueDate.DayNumber : 0;

    /// <summary>The loan's days late: those of its oldest instalment not paid in full.</summary>
    public static int DaysLate(IEnumerable<LndInstallment> schedule, DateOnly day) =>
        schedule.OrderBy(x => x.InstallmentNo).Select(x => DaysLate(x, day)).FirstOrDefault(x => x > 0);

    /// <summary>
    /// The late fee to charge today on one instalment, and its new count of fee days. The base of a percentage is what the instalment
    /// still owes (without late fees) or its principal still owed.
    /// </summary>
    public static (decimal Fee, int Days) LateFee(LndLoan loan, LndInstallment i, DateOnly day)
    {
        var late = DaysLate(i, day);
        if (late <= loan.LateFeeGraceDays || loan.LateFeeTypeCode == "NONE") return (0, i.LateFeeDays);
        var once = loan.LateFeeTypeCode is "FLAT" or "PCT_ONCE";
        if (once && i.LateFeeDays > 0) return (0, i.LateFeeDays);
        var baseAmount = loan.LateFeeBaseCode == "OVERDUE_PRINCIPAL" ? i.PrincipalDueAmount - i.PrincipalPaidAmount : Owed(i, false);
        var rate = (loan.LateFeeRatePct ?? 0) / 100m;
        var fee = loan.LateFeeTypeCode switch
        {
            "FLAT" => loan.LateFeeFlatAmount ?? 0,
            "PCT_ONCE" => R(baseAmount * rate),
            "PCT_DAILY" => R(baseAmount * rate) * (late - i.LateFeeDays),
            _ => 0,
        };
        return (Math.Max(0, fee), late);
    }

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
