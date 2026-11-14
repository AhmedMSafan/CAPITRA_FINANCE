using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>FNV_FINANCE_PKG: the cheque totals of a portfolio and its financial study figures.</summary>
public sealed class MtgFinancePackage(ICapitraDbContext db)
{
    /// <summary>cheque_total: the sum of the portfolio's cheques.</summary>
    public async Task<decimal> ChequeTotalAsync(long portfolioId, CancellationToken ct) =>
        await db.Set<MtgPurchasedPortfolioCheque>().Where(c => c.PortfolioId == portfolioId).SumAsync(c => (decimal?)c.ChequeAmount, ct) ?? 0;

    /// <summary>outstanding_total: the sum of the outstanding values of the portfolio's accounts.</summary>
    public async Task<decimal> OutstandingTotalAsync(long portfolioId, CancellationToken ct) =>
        await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == portfolioId).SumAsync(a => (decimal?)a.OutstandingValue, ct) ?? 0;

    /// <summary>cheque_coverage_pct: the cheques against outstanding plus expected return, in percent (null when that is 0).</summary>
    public async Task<decimal?> ChequeCoveragePctAsync(long portfolioId, CancellationToken ct)
    {
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().Where(x => x.PortfolioId == portfolioId)
            .Select(x => new { x.OutstandingValue, x.ExpectedReturnAmount }).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("ORA-01403: no data found");
        var required = p.OutstandingValue + p.ExpectedReturnAmount;
        if (required == 0)
            return null;
        return Math.Round(await ChequeTotalAsync(portfolioId, ct) / required * 100, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>recalc_financial_study: copies the portfolio's figures into its financial study (merge: update or insert).</summary>
    public async Task RecalcFinancialStudyAsync(long portfolioId, CancellationToken ct)
    {
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new InvalidOperationException("ORA-01403: no data found");
        var cheques = await ChequeTotalAsync(portfolioId, ct);
        var coverage = await ChequeCoveragePctAsync(portfolioId, ct);

        var fs = await db.Set<MtgFinancialStudy>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct);
        if (fs is null)
        {
            fs = new MtgFinancialStudy { PortfolioId = portfolioId };
            db.Add(fs);
        }

        fs.PortfolioFaceValue = p.FaceValue;
        fs.OutstandingValue = p.OutstandingValue;
        fs.ProposedPurchaseValue = p.ProposedPurchaseValue;
        fs.TermMonths = p.TermMonths;
        fs.ExpectedReturnAmount = p.ExpectedReturnAmount;
        fs.AdminExpenseAmount = p.AdminExpenseAmount;
        fs.NetReturnAmount = p.ExpectedReturnAmount - p.AdminExpenseAmount;
        fs.ChequeTotalValue = cheques;
        fs.ChequeCoveragePct = coverage;
        await db.SaveChangesAsync(ct);
    }
}
