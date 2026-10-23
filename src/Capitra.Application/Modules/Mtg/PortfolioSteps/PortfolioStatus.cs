using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps;

/// <summary>Status moves shared by the workflow step services (APEX FNV_PURSHASED_PORTFOLIO_PKG.change_status).</summary>
internal static class PortfolioStatus
{
    /// <summary>
    /// Refuses the step's actions unless the portfolio is in one of the step's statuses. Until the workflow engine is in
    /// place (build step 4), the status stands in for "an open task of this step exists".
    /// </summary>
    public static async Task<MtgPortfolio> RequireAsync(
        ICapitraDbContext db, long portfolioId, string stepName, IReadOnlyCollection<string> allowed, CancellationToken cancellationToken)
    {
        var portfolio = await db.Set<MtgPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken)
                        ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");
        if (!allowed.Contains(portfolio.PortfolioStatusCode))
            throw new ConflictException($"{stepName} is not open for this portfolio (status {portfolio.PortfolioStatusCode}).");
        return portfolio;
    }

    /// <summary>Moves the portfolio to a new status and records it in the status history. Saves.</summary>
    public static async Task ChangeAsync(
        ICapitraDbContext db, MtgPortfolio portfolio, string newStatus, string? comments, CancellationToken cancellationToken)
    {
        if (portfolio.PortfolioStatusCode == newStatus)
            return;

        db.Add(new MtgPortfolioStatusHistory
        {
            PortfolioId = portfolio.PortfolioId,
            FromStatusCode = portfolio.PortfolioStatusCode,
            ToStatusCode = newStatus,
            Comments = comments
        });
        portfolio.PortfolioStatusCode = newStatus;
        await db.SaveChangesAsync(cancellationToken);
    }
}
