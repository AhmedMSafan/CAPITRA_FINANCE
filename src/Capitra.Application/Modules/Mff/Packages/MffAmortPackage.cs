using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_AMORT_PKG: the level monthly payment of a loan and its installment schedule.</summary>
public sealed class MffAmortPackage(ICapitraDbContext db)
{
    /// <summary>LEVEL_PAYMENT: the monthly payment that repays the principal over the term at the annual rate, 2 decimals.</summary>
    public static decimal LevelPayment(decimal? principal, decimal? annualRatePct, int? termMonths)
    {
        if ((principal ?? 0) <= 0 || (termMonths ?? 0) <= 0)
            return 0;
        var r = (annualRatePct ?? 0) / 1200;
        if (r == 0)
            return Round(principal!.Value / termMonths!.Value);
        var factor = (decimal)Math.Pow((double)(1 + r), -termMonths!.Value);
        return Round(principal!.Value * r / (1 - factor));
    }

    /// <summary>BUILD_LOAN_SCHEDULE: writes the loan's installments (rebuild: replaces them) and its payment, maturity and next due date.</summary>
    public async Task BuildLoanScheduleAsync(long loanId, bool rebuild, CancellationToken ct)
    {
        var l = await db.Set<MffLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new ApexProcessException("ORA-20011: حساب التمويل غير موجود.");
        var existing = await db.Set<MffInstallment>().Where(x => x.LoanId == loanId).ToListAsync(ct);
        if (existing.Count > 0 && !rebuild)
            throw new ApexProcessException("ORA-20010: يوجد جدول أقساط قائم. استخدم P_REBUILD=Y لإعادة الإنشاء.");
        foreach (var x in existing)
            db.Remove(x);

        var payment = LevelPayment(l.PrincipalAmount, l.AnnualRatePct, l.TermMonths);
        var open = l.PrincipalAmount;
        var r = (l.AnnualRatePct ?? 0) / 1200;
        for (var i = 1; i <= l.TermMonths; i++)
        {
            var interest = Round(open * r);
            decimal principal;
            if (i == l.TermMonths)
            {
                principal = open;
                payment = Round(principal + interest);
            }
            else
            {
                principal = Round(Math.Min(open, payment - interest));
            }

            var close = Round(Math.Max(0, open - principal));
            db.Add(new MffInstallment
            {
                LoanId = loanId,
                InstallmentNo = i,
                DueDate = l.DisbursementDate.AddMonths(i),
                OpeningPrincipal = open,
                PrincipalDue = principal,
                InterestDue = interest,
                InsuranceDue = 0,
                FeesDue = 0,
                SubsidyDue = 0,
                BorrowerDue = payment,
                TotalDue = payment,
                PaidAmount = 0,
                PaymentStatusCode = "DUE",
                ClosingPrincipal = close
            });
            open = close;
        }

        l.InitialInstallment = LevelPayment(l.PrincipalAmount, l.AnnualRatePct, l.TermMonths);
        l.OutstandingPrincipal = l.PrincipalAmount;
        l.MaturityDate = l.DisbursementDate.AddMonths(l.TermMonths);
        l.NextDueDate = l.DisbursementDate.AddMonths(1);
        await db.SaveChangesAsync(ct);
    }

    private static decimal Round(decimal d) => Math.Round(d, 2, MidpointRounding.AwayFromZero);
}
