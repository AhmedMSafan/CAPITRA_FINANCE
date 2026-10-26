using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>FNV_PURSHASED_PORTFOLIO_PKG: the portfolio totals kept from its accounts.</summary>
public sealed class MtgPurchasedPortfolioPackage(ICapitraDbContext db, MtgFinancePackage finance)
{
    /// <summary>recalc_portfolio: face and outstanding value from the accounts, then the financial study.</summary>
    public async Task RecalcPortfolioAsync(long portfolioId, CancellationToken ct)
    {
        var accounts = db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == portfolioId);
        var contract = await accounts.SumAsync(a => (decimal?)a.ContractValue, ct) ?? 0;
        var outstanding = await accounts.SumAsync(a => (decimal?)a.OutstandingValue, ct) ?? 0;

        var portfolio = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct);
        if (portfolio is not null)
        {
            portfolio.FaceValue = contract;
            portfolio.OutstandingValue = outstanding;
            await db.SaveChangesAsync(ct);
        }

        await finance.RecalcFinancialStudyAsync(portfolioId, ct);
    }
}
