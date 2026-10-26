using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>
/// NEW (not in Oracle): MFF_SERVICING_PKG. Pages 84, 121, 131 and 142 call it, but the package is not in the FINOVA
/// database. Designed in C# from those pages and the MFF loan tables.
/// </summary>
public sealed class MffServicingPackage(ICapitraDbContext db)
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
