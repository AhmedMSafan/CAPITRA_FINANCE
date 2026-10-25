using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps;

public sealed record DealDecisionItem(long PortfolioAccountId, string DealDecisionCode);

public sealed record SaveDealDecisionsRequest(IReadOnlyList<DealDecisionItem> Items);

/// <summary>Keeping contracts in the deal or leaving them out, as the review steps do (APEX AML/INQ_UPDATE_INVESTOR).</summary>
internal static class DealDecisions
{
    /// <param name="allowReinclude">False where APEX locks an exclusion ("Excluded investors cannot be selected again.").</param>
    public static async Task SaveAsync(
        ICapitraDbContext db, long portfolioId, SaveDealDecisionsRequest request, bool allowReinclude, CancellationToken cancellationToken)
    {
        var items = request.Items ?? [];
        if (items.Count == 0)
            throw BusinessValidationException.For(nameof(request.Items), "Choose at least one contract.");
        if (items.Any(x => x.DealDecisionCode is not (MtgPurchasedPortfolioAccount.SelectedDecision or MtgPurchasedPortfolioAccount.ExcludedDecision)))
            throw BusinessValidationException.For(nameof(request.Items), "Decision must be SELECTED or EXCLUDED.");

        var ids = items.Select(x => x.PortfolioAccountId).Distinct().ToList();
        var accounts = await db.Set<MtgPurchasedPortfolioAccount>()
            .Where(x => x.PortfolioId == portfolioId && ids.Contains(x.PortfolioAccountId))
            .ToDictionaryAsync(x => x.PortfolioAccountId, cancellationToken);
        if (accounts.Count != ids.Count)
            throw new NotFoundException("Portfolio investor account not found.");

        foreach (var item in items)
        {
            var account = accounts[item.PortfolioAccountId];
            if (!allowReinclude && account.DealDecisionCode == MtgPurchasedPortfolioAccount.ExcludedDecision
                                && item.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision)
                throw new ConflictException("Excluded investors cannot be selected again.");
            account.DealDecisionCode = item.DealDecisionCode;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
