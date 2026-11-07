using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>
/// NEW (not in Oracle): MFF_SERVICING_PKG. Pages 84, 121, 131 and 142 call it, but the package is not in the FINOVA
/// database. Designed in C# from those pages and the MFF loan tables.
/// </summary>
public sealed class MffServicingPackage(ICapitraDbContext db, IClock clock)
{
    /// <summary>
    /// NEW generate_installments: the loan's installment schedule from the first due date. The borrower's payment grows once a
    /// year (growth %, for the growth years) and the Fund pays the monthly subsidy for the subsidy months; the initial
    /// payment is the one that repays the principal at the loan rate. The last installment clears any rest.
    /// </summary>
    public async Task GenerateInstallmentsAsync(long loanId, DateOnly? firstDueDate, decimal growthPct, decimal growthYears, decimal monthlySubsidy,
        int subsidyMonths, CancellationToken ct)
    {
        var l = await db.Set<MffLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new ApexProcessException("ORA-20011: حساب التمويل غير موجود.");
        if (await db.Set<MffInstallment>().AnyAsync(x => x.LoanId == loanId, ct))
            throw new ApexProcessException("ORA-20010: يوجد جدول أقساط قائم.");
        var first = firstDueDate ?? l.DisbursementDate.AddMonths(1);
        var (initial, rows) = Schedule(l.PrincipalAmount, l.AnnualRatePct ?? 0, l.TermMonths, first, growthPct, growthYears, monthlySubsidy, subsidyMonths);
        foreach (var x in rows)
        {
            x.LoanId = loanId;
            db.Add(x);
        }

        var n = l.TermMonths;
        l.InitialInstallment = initial;
        l.OutstandingPrincipal = l.PrincipalAmount;
        l.MaturityDate ??= first.AddMonths(n - 1);
        l.NextDueDate = first;
        await db.SaveChangesAsync(ct);
    }

    private static readonly string[] Open = ["DUE", "PARTIAL", "OVERDUE"];

    /// <summary>
    /// NEW post_payment: a borrower payment, applied to the loan's open installments oldest first (each: fees, insurance,
    /// interest, then principal). The loan's outstanding principal and next due date follow, then its delinquency is refreshed.
    /// A payment above what is due is refused: prepayment goes through early settlement (page 132).
    /// </summary>
    public async Task<long> PostPaymentAsync(long loanId, DateOnly paymentDate, decimal amount, string? channel, string? referenceNo, CancellationToken ct)
    {
        var l = await db.Set<MffLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new ApexProcessException("ORA-20011: حساب التمويل غير موجود.");
        var open = await db.Set<MffInstallment>().Where(x => x.LoanId == loanId && Open.Contains(x.PaymentStatusCode))
            .OrderBy(x => x.DueDate).ThenBy(x => x.InstallmentNo).ToListAsync(ct);
        var due = open.Sum(x => Math.Max(0, (x.TotalDue ?? 0) - (x.PaidAmount ?? 0)));
        if (amount > due)
            throw new ApexProcessException("ORA-20079: Payment exceeds the amount due (" + due.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)
                + "). Use early settlement for a prepayment.");

        decimal fees = 0, insurance = 0, interest = 0, principal = 0, rest = amount;
        long? firstInstallment = null;
        foreach (var i in open)
        {
            if (rest <= 0)
                break;
            firstInstallment ??= i.InstallmentId;
            var paid = i.PaidAmount ?? 0;
            var pay = Math.Min(rest, Math.Max(0, (i.TotalDue ?? 0) - paid));
            // the part of each component still unpaid, in the order fees, insurance, interest, principal
            var take = pay;
            foreach (var (part, add) in new (decimal Part, Action<decimal> Add)[]
                     {
                         (i.FeesDue ?? 0, v => fees += v), (i.InsuranceDue ?? 0, v => insurance += v),
                         (i.InterestDue ?? 0, v => interest += v), ((i.TotalDue ?? 0) - (i.FeesDue ?? 0) - (i.InsuranceDue ?? 0) - (i.InterestDue ?? 0), v => principal += v),
                     })
            {
                var already = Math.Min(paid, part);
                paid -= already;
                var v = Math.Min(take, part - already);
                add(v);
                take -= v;
            }

            i.PaidAmount = (i.PaidAmount ?? 0) + pay;
            i.PaymentStatusCode = i.PaidAmount >= i.TotalDue ? "PAID" : "PARTIAL";
            i.PaidDate = paymentDate;
            rest -= pay;
        }

        var p = new MffPayment
        {
            LoanId = loanId,
            InstallmentId = firstInstallment,
            PaymentDate = paymentDate,
            PaymentAmount = amount,
            PaymentType = "BORROWER",
            PaymentChannel = channel,
            ReferenceNo = referenceNo,
            AllocatedPrincipal = principal,
            AllocatedInterest = interest,
            AllocatedFees = fees,
            AllocatedInsurance = insurance,
            StatusCode = "PAID",
        };
        db.Add(p);
        l.OutstandingPrincipal = Math.Max(0, (l.OutstandingPrincipal ?? l.PrincipalAmount) - principal);
        l.NextDueDate = open.Where(x => x.PaymentStatusCode != "PAID").Select(x => (DateOnly?)x.DueDate).FirstOrDefault();
        if (l.OutstandingPrincipal == 0 && l.NextDueDate is null)
            l.LoanStatus = "CLOSED";
        await db.SaveChangesAsync(ct);
        await RefreshDelinquencyAsync(loanId, Today(), ct);
        return p.PaymentId;
    }

    /// <summary>
    /// NEW refresh_delinquency: marks the installments past due as OVERDUE and sets the loan's days past due, its open
    /// delinquency case (stage by days: CURRENT, DPD_1_30, DPD_31_60, DPD_61_90, DPD_90_PLUS; a LEGAL case stays LEGAL)
    /// and its status (DELINQUENT / ACTIVE). A case with nothing overdue any more is closed.
    /// </summary>
    public async Task RefreshDelinquencyAsync(long loanId, DateOnly asOf, CancellationToken ct)
    {
        var l = await db.Set<MffLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new ApexProcessException("ORA-20011: حساب التمويل غير موجود.");
        var overdue = await db.Set<MffInstallment>().Where(x => x.LoanId == loanId && Open.Contains(x.PaymentStatusCode) && x.DueDate < asOf)
            .OrderBy(x => x.DueDate).ToListAsync(ct);
        foreach (var i in overdue.Where(i => i.PaymentStatusCode == "DUE"))
            i.PaymentStatusCode = "OVERDUE";
        var dpd = overdue.Count == 0 ? 0 : asOf.DayNumber - overdue[0].DueDate.DayNumber;
        var amount = overdue.Sum(x => Math.Max(0, (x.TotalDue ?? 0) - (x.PaidAmount ?? 0)));
        l.DelinquentDays = dpd;
        if (dpd > 0 && l.LoanStatus == "ACTIVE")
            l.LoanStatus = "DELINQUENT";
        else if (dpd == 0 && l.LoanStatus == "DELINQUENT")
            l.LoanStatus = "ACTIVE";

        var d = await db.Set<MffDelinquency>().Where(x => x.LoanId == loanId && x.DelinquencyStatus != "CLOSED")
            .OrderByDescending(x => x.DelinquencyId).FirstOrDefaultAsync(ct);
        if (dpd > 0)
        {
            if (d is null)
            {
                d = new MffDelinquency { LoanId = loanId, StartDate = overdue[0].DueDate.AddDays(1), DelinquencyStatus = "OPEN" };
                db.Add(d);
            }

            d.StageCode = d.StageCode == "LEGAL" ? "LEGAL" : Stage(dpd);
            d.DaysPastDue = dpd;
            d.OverdueAmount = amount;
            d.OverdueInstallments = overdue.Count;
        }
        else if (d is not null)
        {
            d.StageCode = "CURRENT";
            d.DaysPastDue = 0;
            d.OverdueAmount = 0;
            d.OverdueInstallments = 0;
            d.DelinquencyStatus = "CLOSED";
            d.EndDate = asOf;
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>The delinquency stage for the days past due.</summary>
    public static string Stage(int dpd) => dpd switch
    {
        <= 0 => "CURRENT",
        <= 30 => "DPD_1_30",
        <= 60 => "DPD_31_60",
        <= 90 => "DPD_61_90",
        _ => "DPD_90_PLUS",
    };

    /// <summary>
    /// NEW take_monthly_snapshot: one row per loan for the month (replaces the month's rows): status, outstanding principal,
    /// overdue amount and days, the subsidy accounts' balance, and the last payment.
    /// </summary>
    public async Task TakeMonthlySnapshotAsync(DateOnly month, CancellationToken ct)
    {
        month = new DateOnly(month.Year, month.Month, 1);
        foreach (var x in await db.Set<MffLoanMonthlySnapshot>().Where(x => x.SnapshotMonthDate == month).ToListAsync(ct))
            db.Remove(x);
        var today = Today();
        foreach (var l in await db.Set<MffLoan>().AsNoTracking().OrderBy(x => x.LoanId).ToListAsync(ct))
        {
            var overdue = await db.Set<MffInstallment>().Where(x => x.LoanId == l.LoanId && Open.Contains(x.PaymentStatusCode) && x.DueDate < today)
                .SumAsync(x => (x.TotalDue ?? 0) - (x.PaidAmount ?? 0), ct);
            var last = await db.Set<MffPayment>().Where(x => x.LoanId == l.LoanId && x.StatusCode != "REVERSED")
                .OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentId).FirstOrDefaultAsync(ct);
            db.Add(new MffLoanMonthlySnapshot
            {
                SnapshotMonthDate = month,
                LoanId = l.LoanId,
                LoanStatus = l.LoanStatus,
                OutstandingPrincipal = l.OutstandingPrincipal ?? l.PrincipalAmount,
                OverdueAmount = Math.Max(0, overdue),
                DaysPastDue = l.DelinquentDays,
                SubsidyBalance = await db.Set<MffSubsidyAccount>().Where(x => x.LoanId == l.LoanId).SumAsync(x => x.CurrentBalance, ct),
                LastPaymentDate = last?.PaymentDate,
                LastPaymentAmount = last?.PaymentAmount,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private DateOnly Today() => DateOnly.FromDateTime(Mtg.Pages.Custom.MtgAjax.CairoNow(clock));

    /// <summary>The installments (without the loan id) and the borrower's initial payment.</summary>
    public static (decimal Initial, List<MffInstallment> Rows) Schedule(decimal principalAmount, decimal rate, int n, DateOnly first, decimal growthPct,
        decimal growthYears, decimal monthlySubsidy, int subsidyMonths)
    {
        var subMonths = Math.Min(subsidyMonths, n);
        var pvSubsidy = MffCalcPackage.PresentValue(rate, subMonths, _ => monthlySubsidy);
        var factor = MffCalcPackage.PresentValue(rate, n, m => MffCalcPackage.GrowingPayment(1, growthPct, growthYears, m));
        var initial = factor > 0 ? Math.Max(0, principalAmount - pvSubsidy) / factor : 0;

        var rows = new List<MffInstallment>(n);
        var r = rate / 1200;
        var open = principalAmount;
        for (var m = 1; m <= n; m++)
        {
            var borrower = R(MffCalcPackage.GrowingPayment(initial, growthPct, growthYears, m));
            var subsidy = m <= subMonths ? monthlySubsidy : 0;
            var interest = R(open * r);
            var principal = R(Math.Min(open, borrower + subsidy - interest));
            if (m == n)
            {
                principal = open;
                borrower = R(principal + interest - subsidy);
            }

            var close = R(open - principal);
            rows.Add(new MffInstallment
            {
                InstallmentNo = m,
                DueDate = first.AddMonths(m - 1),
                OpeningPrincipal = open,
                PrincipalDue = principal,
                InterestDue = interest,
                InsuranceDue = 0,
                FeesDue = 0,
                SubsidyDue = subsidy,
                BorrowerDue = borrower,
                TotalDue = borrower + subsidy,
                PaidAmount = 0,
                PaymentStatusCode = "DUE",
                ClosingPrincipal = close,
            });
            open = close;
        }

        return (R(initial), rows);
    }

    private static decimal R(decimal d) => Math.Round(d, 2, MidpointRounding.AwayFromZero);
}
