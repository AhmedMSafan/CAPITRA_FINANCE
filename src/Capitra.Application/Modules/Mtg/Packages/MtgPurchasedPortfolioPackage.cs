using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>FNV_PURSHASED_PORTFOLIO_PKG: the portfolio totals kept from its accounts, and its submission for approval.</summary>
public sealed class MtgPurchasedPortfolioPackage(ICapitraDbContext db, MtgFinancePackage finance, MtgValidationPackage validation, IClock clock)
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

    /// <summary>validate_portfolio: the readiness message of FNV_VALIDATION_PKG.</summary>
    public Task<string> ValidatePortfolioAsync(long portfolioId, CancellationToken ct) => validation.ReadinessMessageAsync(portfolioId, ct);

    /// <summary>submit_portfolio: to PENDING_APPROVAL at the CREDIT step, with its status history and first approval row.</summary>
    public async Task SubmitPortfolioAsync(long portfolioId, string user, CancellationToken ct)
    {
        if (!await validation.IsReadyForSubmissionAsync(portfolioId, ct))
            throw new ApexProcessException(await validation.ReadinessMessageAsync(portfolioId, ct));

        var today = DateOnly.FromDateTime(clock.UtcNow);
        var portfolio = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct);
        if (portfolio is not null)
        {
            portfolio.PortfolioStatusCode = "PENDING_APPROVAL";
            portfolio.CurrentApprovalStep = "CREDIT";
            portfolio.SubmittedAt = clock.UtcNow.Date;
            db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
            {
                PortfolioId = portfolioId,
                FromStatus = "DUE_DILIGENCE",
                ToStatus = "PENDING_APPROVAL",
                ChangeAt = clock.UtcNow,
                ChangedBy = user,
                Comments = "Portfolio submitted",
            });
        }

        db.Set<MtgPurchasedPortfolioApproval>().Add(new MtgPurchasedPortfolioApproval
        {
            PortfolioId = portfolioId,
            StepCode = "CREDIT",
            SequenceNo = 10,
            AssignedDate = today,
        });
        await db.SaveChangesAsync(ct);
    }
}
