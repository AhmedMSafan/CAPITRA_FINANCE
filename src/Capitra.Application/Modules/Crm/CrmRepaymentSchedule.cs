using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Fct;

namespace Capitra.Application.Modules.Crm;

/// <summary>One dated amount the schedule draws on: an eligible invoice's advance or an uploaded cheque.</summary>
public sealed record CrmScheduleSource(DateOnly DueDate, decimal Amount);

/// <summary>
/// The repayment schedule from the agreed terms (Ahmed, 2 Nov 2026), built before the customer's cheques: the cheques must then match
/// each due date's total. Two schedule types:
/// - NORMAL_SCHEDULE (regular): principal every N months from the principal start date, on the invoices' due dates (classic), or at
///   maturity; maturity is the disbursement date plus the tenor.
/// - CUSTOM_SCHEDULE (irregular): the principal payments entered on the page (date and amount), adding up to the amount financed; the
///   last date is the maturity.
/// Interest, either way: every N months from the interest start date, every N month ends (End of Month 30: the 30th, February's last
/// day; End of Month Actual: the month's real last day), on the principal dates, or at maturity. It accrues on the outstanding
/// principal for the days of each period; every row is one due date with its principal, interest and total, in 2 decimals.
/// </summary>
public static class CrmRepaymentSchedule
{
    public const string Normal = "NORMAL_SCHEDULE";
    public const string Custom = "CUSTOM_SCHEDULE";
    public const string SameMonth = "SAME_MONTH";
    public const string NextMonth = "NEXT_MONTH";

    /// <summary>normalize_code: upper case, '-', ' ' and '/' become '_'.</summary>
    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Replace('-', '_').Replace(' ', '_').Replace('/', '_').ToUpperInvariant();

    /// <summary>Oracle ADD_MONTHS: the last day of a month stays the last day of the target month.</summary>
    public static DateOnly AddMonths(DateOnly date, int months) =>
        date.Day == DateTime.DaysInMonth(date.Year, date.Month) ? LastDay(date.AddMonths(months)) : date.AddMonths(months);

    public static DateOnly LastDay(DateOnly date) => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>End of Month 30: the 30th, or the month's last day when it is shorter (February).</summary>
    public static DateOnly Day30(DateOnly date) => new(date.Year, date.Month, Math.Min(30, DateTime.DaysInMonth(date.Year, date.Month)));

    /// <summary>The principal method, older codes read as the new ones (MONTHLY, QUARTERLY, SEMI_ANNUAL, ANNUAL: every 1, 3, 6, 12 months).</summary>
    public static (string Method, int Months) PrincipalMethod(FctDisbursement d) => Normalize(d.PrincipalRepaymentMethod) switch
    {
        "MONTHLY" => ("EVERY_N_MONTHS", 1),
        "QUARTERLY" => ("EVERY_N_MONTHS", 3),
        "SEMI_ANNUAL" or "SEMIANNUAL" => ("EVERY_N_MONTHS", 6),
        "ANNUAL" => ("EVERY_N_MONTHS", 12),
        "INVOICE_DUE_DATE" or "INVOICES_DUE_DATE" => ("INVOICE_DUE_DATE", 0),
        "EVERY_N_MONTHS" => ("EVERY_N_MONTHS", d.PrincipalFrequencyMonths ?? 0),
        "END_OF_MONTH_30" => ("END_OF_MONTH_30", d.PrincipalFrequencyMonths ?? 1),
        "END_OF_MONTH_ACTUAL" => ("END_OF_MONTH_ACTUAL", d.PrincipalFrequencyMonths ?? 1),
        null or "AT_MATURITY" => ("AT_MATURITY", 0),
        var other => (other, 0),
    };

    /// <summary>The interest method, older codes read as the new ones (MONTHLY was the month ends, SAME_AS_PRINCIPAL the principal dates).</summary>
    public static string InterestMethod(FctDisbursement d) => Normalize(d.InterestRepaymentMethod) switch
    {
        null or "MONTHLY" => "END_OF_MONTH_ACTUAL",
        "SAME_AS_PRINCIPAL" => "WITH_PRINCIPAL",
        var other => other,
    };

    /// <summary>The last due date: the last principal payment of a custom schedule, else the disbursement date plus the tenor.</summary>
    public static DateOnly? Maturity(FctDisbursement d, IReadOnlyList<CrmScheduleSource> custom) =>
        (Normalize(d.PrincipalRepaymentScheduleType) ?? Normal) == Custom
            ? custom.Count == 0 ? null : custom.Max(x => x.DueDate)
            : (d.TenorMonth ?? 0) > 0 ? AddMonths(d.DisbursementDate, d.TenorMonth!.Value) : null;

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

    /// <summary>
    /// The due dates of a frequency before maturity, from its start date: EVERY_N_MONTHS the start date plus N, 2N... months
    /// (start 15 Jul, every 1 month: 15 Aug, 15 Sep); END_OF_MONTH_30 and END_OF_MONTH_ACTUAL every N month ends from the first one,
    /// the start month's end (SAME_MONTH: start 15 Apr, 30 Apr) or the next month's (NEXT_MONTH, the default: 31 May).
    /// Dates on or before the disbursement date are left out.
    /// </summary>
    public static IEnumerable<DateOnly> Periodic(
        string method, int months, DateOnly start, DateOnly disbursementDate, DateOnly maturity, string? firstDueCode = null)
    {
        var firstOfMonth = start.AddDays(1 - start.Day).AddMonths(Normalize(firstDueCode) == SameMonth ? 0 : 1);
        for (var k = 0; ; k++)
        {
            var x = method switch
            {
                "EVERY_N_MONTHS" => AddMonths(start, months * (k + 1)),
                "END_OF_MONTH_30" => Day30(firstOfMonth.AddMonths(months * k)),
                _ => LastDay(firstOfMonth.AddMonths(months * k)),
            };
            if (x >= maturity) yield break;
            if (x > disbursementDate && x >= start) yield return x;
        }
    }

    /// <param name="d">The processed request: its approved advance is already calculated.</param>
    /// <param name="financedCharges">The CALCULATED FINANCE_WITH_PRINCIPAL charges, added to the principal.</param>
    /// <param name="invoices">The request's ELIGIBLE invoices: due date and advance amount.</param>
    /// <param name="custom">The principal payments of a custom schedule (FCT_DISBURSEMENT_CUSTOM_SCHEDULES).</param>
    /// <param name="version">The new SCHEDULE_VERSION.</param>
    public static List<FctRepayment> Build(
        FctDisbursement d, decimal financedCharges, IReadOnlyList<CrmScheduleSource> invoices, IReadOnlyList<CrmScheduleSource> custom, int version)
    {
        var scheduleType = Normalize(d.PrincipalRepaymentScheduleType) ?? Normal;
        if (scheduleType is not (Normal or Custom)) throw new BusinessRuleException($"Schedule type {scheduleType} is not known.");
        var (principalMethod, principalMonths) = PrincipalMethod(d);
        var distribution = Normalize(d.PrincipalDistributionMethod) ?? "EQUAL_AMOUNT";
        var interestMethod = InterestMethod(d);
        var interestMonths = d.InterestFrequencyMonths ?? 1;
        var dayCountCode = Normalize(d.BaseDayCountBasis ?? "ACTUAL_360");
        var disbursementDate = d.DisbursementDate;
        var initialPrincipal = Math.Round(d.ApprovedAdvanceAmount + financedCharges, 2, MidpointRounding.AwayFromZero);
        var rate = (d.IndexRate ?? 0) + (d.SpreadRate ?? 0);

        // the principal: dates and amounts
        var principal = new SortedDictionary<DateOnly, decimal>();
        DateOnly maturity;
        if (scheduleType == Custom)
        {
            if (custom.Count == 0) throw new BusinessRuleException("Custom schedule: enter the principal payments (date and amount).");
            if (custom.Any(x => x.DueDate <= disbursementDate))
                throw new BusinessRuleException("Custom schedule: every principal payment date must be after the disbursement date.");
            if (custom.Any(x => x.Amount <= 0)) throw new BusinessRuleException("Custom schedule: every principal amount must be more than zero.");
            var total = custom.Sum(x => x.Amount);
            if (Math.Abs(total - initialPrincipal) > 0.01m)
                throw new BusinessRuleException(
                    $"Custom schedule: the principal payments total {total:#,##0.00}, not the amount financed {initialPrincipal:#,##0.00}.");
            foreach (var g in custom.GroupBy(x => x.DueDate)) principal[g.Key] = g.Sum(x => x.Amount);
            maturity = principal.Keys.Max();
        }
        else
        {
            if ((d.TenorMonth ?? 0) <= 0) throw new BusinessRuleException("Tenor Months must be greater than zero.");
            maturity = AddMonths(disbursementDate, d.TenorMonth!.Value);
            DateOnly Capped(DateOnly x) => x < maturity ? x : maturity;
            var dates = new SortedSet<DateOnly>();
            switch (principalMethod)
            {
                case "EVERY_N_MONTHS" or "END_OF_MONTH_30" or "END_OF_MONTH_ACTUAL":
                    if (principalMonths <= 0) throw new BusinessRuleException("Principal every N months: N must be a whole number of 1 or more.");
                    dates.UnionWith(Periodic(principalMethod, principalMonths, d.RepaymentStartDate ?? disbursementDate, disbursementDate, maturity,
                        d.PrincipalFirstDueCode));
                    break;
                case "INVOICE_DUE_DATE":
                    foreach (var i in invoices.Where(x => x.DueDate > disbursementDate)) dates.Add(Capped(i.DueDate));
                    break;
                case "AT_MATURITY":
                    break;
                default:
                    throw new BusinessRuleException($"Principal repayment method {principalMethod} is not available. Choose every N months or at maturity.");
            }

            dates.Add(maturity);
            var left = initialPrincipal;
            var list = dates.ToList();
            for (var n = 0; n < list.Count; n++)
            {
                var date = list[n];
                var amount = n == list.Count - 1 ? left
                    : principalMethod == "INVOICE_DUE_DATE" && distribution is "INVOICE_AMOUNT" or "INVOICES_AMOUNT"
                        ? Math.Min(left, invoices.Where(x => Capped(x.DueDate) == date).Sum(x => x.Amount))
                        : Math.Round(initialPrincipal / list.Count, 2, MidpointRounding.AwayFromZero);
                amount = Math.Min(amount, left);
                principal[date] = amount;
                left -= amount;
            }
        }

        // the interest dates
        var interestDates = new SortedSet<DateOnly>();
        var interestAnchor = d.InterestStartDate ?? disbursementDate;
        switch (interestMethod)
        {
            case "AT_MATURITY":
                break;
            case "WITH_PRINCIPAL":
                interestDates.UnionWith(principal.Keys);
                break;
            case "EVERY_N_MONTHS" or "END_OF_MONTH_30" or "END_OF_MONTH_ACTUAL":
                if (interestMonths <= 0) throw new BusinessRuleException("Interest every N months: N must be a whole number of 1 or more.");
                interestDates.UnionWith(Periodic(interestMethod, interestMonths, interestAnchor, disbursementDate, maturity, d.InterestFirstDueCode));
                break;
            default:
                throw new BusinessRuleException($"Interest repayment method {interestMethod} is not known.");
        }

        interestDates.Add(maturity);

        var eventDates = new SortedSet<DateOnly>(principal.Keys);
        eventDates.UnionWith(interestDates);
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
                interestDue = Math.Round(accrued, 2, MidpointRounding.AwayFromZero);
                accrued = 0;
            }

            var principalDue = principal.TryGetValue(date, out var p) ? Math.Min(p, opening) : 0;
            var closing = Math.Max(opening - principalDue, 0);
            rows.Add(new FctRepayment
            {
                DisbursementId = d.DisbursementId,
                InstallmentNo = installment,
                DueDate = date,
                AdjustedDueAt = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                OpeningPrincipal = opening,
                OutstandingPrincipal = opening,
                OutstandingInterest = accrued,
                TotalInstallment = principalDue + interestDue,
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
                RecalcReason = "Generated from disbursement repayment terms",
            });
            opening = closing;
            previous = date;
        }

        if (Math.Abs(opening) > 0.01m)
            throw new BusinessRuleException($"Generated schedule did not fully repay principal. Remaining amount: {opening}.");
        return rows;
    }
}
