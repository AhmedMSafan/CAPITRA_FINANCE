using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Fct;

namespace Capitra.Application.Modules.Crm;

/// <summary>One dated amount the schedule draws on: an eligible invoice's advance or an uploaded cheque.</summary>
public sealed record CrmScheduleSource(DateOnly DueDate, decimal Amount);

/// <summary>
/// FNV_DISBURSEMENT_PROCESS_PKG.generate_repayment_schedule: the principal dates (normal schedule by frequency, invoice or cheque
/// due dates, or maturity), the interest dates (month ends, maturity, or the principal dates), and one FCT_REPAYMENTS row per date
/// with the interest accrued on the opening principal since the previous date.
/// </summary>
public static class CrmRepaymentSchedule
{
    /// <summary>normalize_code: upper case, '-', ' ' and '/' become '_'.</summary>
    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Replace('-', '_').Replace(' ', '_').Replace('/', '_').ToUpperInvariant();

    /// <summary>Oracle ADD_MONTHS: the last day of a month stays the last day of the target month.</summary>
    public static DateOnly AddMonths(DateOnly date, int months) =>
        date.Day == DateTime.DaysInMonth(date.Year, date.Month) ? LastDay(date.AddMonths(months)) : date.AddMonths(months);

    public static DateOnly LastDay(DateOnly date) => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>day_count: 30/360 counts 30-day months, every other basis the actual days.</summary>
    public static int DayCount(DateOnly start, DateOnly end, string? dayCountCode)
    {
        if (end <= start) return 0;
        if (Normalize(dayCountCode) is "THIRTY_360" or "30_360")
            return Math.Max((end.Year - start.Year) * 360 + (end.Month - start.Month) * 30 + (Math.Min(end.Day, 30) - Math.Min(start.Day, 30)), 0);
        return end.DayNumber - start.DayNumber;
    }

    /// <summary>interest_amount: principal x annual rate x days / 360 (365 or 366 for the ACTUAL_365 and ACTUAL_366 bases).</summary>
    public static decimal Interest(decimal principal, decimal annualRate, DateOnly start, DateOnly end, string? dayCountCode)
    {
        var denominator = Normalize(dayCountCode) switch
        {
            "ACTUAL_365" or "ACT_365" => 365m,
            "ACTUAL_366" or "ACT_366" => 366m,
            _ => 360m,
        };
        return Math.Round(principal * annualRate / 100 * DayCount(start, end, dayCountCode) / denominator, 4, MidpointRounding.AwayFromZero);
    }

    /// <param name="d">The processed request: its approved advance is already calculated.</param>
    /// <param name="financedCharges">The CALCULATED FINANCE_WITH_PRINCIPAL charges, added to the principal.</param>
    /// <param name="invoices">The request's ELIGIBLE invoices: due date and advance amount.</param>
    /// <param name="cheques">The request's cheques that are not CANCELLED or REPLACED: due date and amount.</param>
    /// <param name="version">The new SCHEDULE_VERSION.</param>
    public static List<FctRepayment> Build(
        FctDisbursement d, decimal financedCharges, IReadOnlyList<CrmScheduleSource> invoices, IReadOnlyList<CrmScheduleSource> cheques, int version)
    {
        if ((d.TenorMonth ?? 0) <= 0) throw new BusinessRuleException("TENOR_MONTH must be greater than zero.");

        var scheduleType = Normalize(d.PrincipalRepaymentScheduleType) ?? "NORMAL_SCHEDULE";
        var principalMethod = Normalize(d.PrincipalRepaymentMethod) ?? "AT_MATURITY";
        var distribution = Normalize(d.PrincipalDistributionMethod) ?? "EQUAL_AMOUNT";
        var interestMethod = Normalize(d.InterestRepaymentMethod) ?? "MONTHLY";
        var dayCountCode = Normalize(d.BaseDayCountBasis ?? "ACTUAL_360");

        // FNV_DISB_CUSTOM_SCHEDULE has no entry screen yet
        if (scheduleType == "CUSTOM_SCHEDULE")
            throw new BusinessRuleException("CUSTOM_SCHEDULE is not available yet: choose NORMAL_SCHEDULE.");

        var disbursementDate = d.DisbursementDate;
        var start = d.RepaymentStartDate is { } r && r > disbursementDate ? r : disbursementDate;
        var maturity = AddMonths(disbursementDate, d.TenorMonth!.Value);
        var rate = (d.IndexRate ?? 0) + (d.SpreadRate ?? 0);
        var initialPrincipal = Math.Round(d.ApprovedAdvanceAmount + financedCharges, 4, MidpointRounding.AwayFromZero);
        DateOnly Capped(DateOnly x) => x < maturity ? x : maturity;

        var principalDates = new SortedSet<DateOnly>();
        switch (principalMethod)
        {
            case "INVOICE_DUE_DATE" or "INVOICES_DUE_DATE":
                foreach (var i in invoices) principalDates.Add(Capped(i.DueDate));
                break;
            case "CHEQUE_DUE_DATE" or "CHEQUES_DUE_DATE":
                foreach (var c in cheques) principalDates.Add(Capped(c.DueDate));
                break;
            case "MONTHLY" or "QUARTERLY" or "SEMI_ANNUAL" or "SEMIANNUAL" or "ANNUAL":
                var frequency = principalMethod switch { "MONTHLY" => 1, "QUARTERLY" => 3, "SEMI_ANNUAL" or "SEMIANNUAL" => 6, _ => 12 };
                for (var x = AddMonths(start, frequency); x < maturity; x = AddMonths(x, frequency)) principalDates.Add(x);
                break;
        }

        // always settle any rounding residue at maturity
        principalDates.Add(maturity);

        var interestDates = new SortedSet<DateOnly>();
        if (interestMethod == "AT_MATURITY")
            interestDates.Add(maturity);
        else if (interestMethod is "SAME_AS_PRINCIPAL" or "WITH_PRINCIPAL")
            interestDates.UnionWith(principalDates);
        else
        {
            var x = LastDay(disbursementDate);
            if (x <= disbursementDate) x = LastDay(AddMonths(x, 1));
            for (; x < maturity; x = LastDay(AddMonths(x, 1))) interestDates.Add(x);
            interestDates.Add(maturity);
            // client runoff rule: accrued interest is also due whenever principal falls between month-end interest dates
            interestDates.UnionWith(principalDates);
        }

        var eventDates = new SortedSet<DateOnly>(principalDates);
        eventDates.UnionWith(interestDates);
        var lastPrincipal = principalDates.Max;

        var totalCheques = 0m;
        if (distribution is "CHEQUE_AMOUNT" or "CHEQUES_AMOUNT" or "CHEQUES_PRO_RATA")
        {
            totalCheques = cheques.Sum(x => x.Amount);
            if (totalCheques <= 0) throw new BusinessRuleException("Cheque-based principal distribution requires uploaded cheques.");
        }

        var rows = new List<FctRepayment>();
        var opening = initialPrincipal;
        var accrued = 0m;
        var previous = disbursementDate;
        var installment = 0;
        foreach (var date in eventDates)
        {
            installment++;
            accrued += Interest(opening, rate, previous, date, dayCountCode);
            var interestDue = 0m;
            if (interestDates.Contains(date))
            {
                interestDue = Math.Round(accrued, 4, MidpointRounding.AwayFromZero);
                accrued = 0;
            }

            var principalDue = 0m;
            if (principalDates.Contains(date))
            {
                if (date == lastPrincipal)
                    principalDue = opening;
                else if (distribution is "INVOICE_AMOUNT" or "INVOICES_AMOUNT")
                    principalDue = invoices.Where(x => Capped(x.DueDate) == date).Sum(x => x.Amount);
                else if (distribution is "CHEQUE_AMOUNT" or "CHEQUES_AMOUNT" or "CHEQUES_PRO_RATA")
                    principalDue = cheques.Where(x => Capped(x.DueDate) == date).Sum(x => x.Amount) * initialPrincipal / totalCheques;
                else
                    principalDue = opening / Math.Max(principalDates.Count(x => x >= date), 1);
                principalDue = Math.Round(Math.Min(principalDue, opening), 4, MidpointRounding.AwayFromZero);
            }

            var closing = Math.Round(Math.Max(opening - principalDue, 0), 4, MidpointRounding.AwayFromZero);
            rows.Add(new FctRepayment
            {
                DisbursementId = d.DisbursementId,
                InstallmentNo = installment,
                DueDate = date,
                AdjustedDueAt = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                OpeningPrincipal = opening,
                OutstandingPrincipal = opening,
                OutstandingInterest = accrued,
                TotalInstallment = Math.Round(principalDue + interestDue, 4, MidpointRounding.AwayFromZero),
                ClosingPrincipal = closing,
                PaidPrincipal = 0,
                PaidInterest = 0,
                PaidFee = 0,
                PaidPenalty = 0,
                PrincipalDue = principalDue,
                InterestDue = interestDue,
                PenaltyDue = 0,
                FeeDue = 0,
                InterestRate = rate,
                DayCount = DayCount(previous, date, dayCountCode),
                DayCountBasis = dayCountCode is { Length: > 10 } code ? code[..10] : dayCountCode,
                DaysOverdue = 0,
                StatusCode = "SCHEDULED",
                ScheduleVersion = version,
                IsCurrentVersion = true,
                RecalcReason = "Generated from disbursement repayment parameters",
            });
            opening = closing;
            previous = date;
        }

        if (Math.Abs(opening) > 0.01m)
            throw new BusinessRuleException($"Generated schedule did not fully repay principal. Remaining amount: {opening}.");
        return rows;
    }
}
