using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Portfolios;

/// <summary>
/// FNV_PURSHASED_PORTFOLIO_PKG.recalc_portfolio: a portfolio's face value and outstanding are the totals of its accounts.
/// APEX also refreshes the financial study here (FNV_FINANCE_PKG.recalc_financial_study); that is added with the
/// financial study screens.
/// </summary>
internal static class PortfolioTotals
{
    public static async Task RecalculateAsync(ICapitraDbContext db, long portfolioId, CancellationToken cancellationToken)
    {
        var totals = await db.Set<MtgPortfolioAccount>()
            .Where(x => x.PortfolioId == portfolioId)
            .GroupBy(x => x.PortfolioId)
            .Select(g => new { Contract = g.Sum(x => x.ContractAmount), Outstanding = g.Sum(x => x.OutstandingAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var portfolio = await db.Set<MtgPortfolio>().FirstAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        portfolio.FaceValueAmount = totals?.Contract ?? 0;
        portfolio.OutstandingAmount = totals?.Outstanding ?? 0;
        await db.SaveChangesAsync(cancellationToken);
    }
}
