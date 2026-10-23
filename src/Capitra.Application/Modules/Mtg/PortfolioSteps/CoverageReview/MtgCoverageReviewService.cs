using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.CoverageReview;

/// <summary>Everything the coverage review step shows for one portfolio.</summary>
public sealed record CoverageReviewDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectName,
    string ProjectCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? RejectionReason,
    bool IsOpen, // the rule can be applied, investors decided and the step decided
    decimal? RequiredCoveragePct, // the portfolio's coverage %, 0 to 100
    IReadOnlyList<CoverageInvestorDto> Investors);

/// <summary>One investor of the portfolio with all of their contracts added up.</summary>
public sealed record CoverageInvestorDto(
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    int ContractCount,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal? PaidPct, // paid / contract value * 100; null when the contract value is zero
    bool? MeetsCoverage, // null when there is no required % or no contract value
    string? DealDecisionCode); // SELECTED, EXCLUDED, MIXED (contracts differ) or null (not decided)

public sealed record CoverageInvestorDecisionRequest(string DealDecisionCode);

public sealed record CoverageDecisionRequest(string Action, string Comment);

public sealed record ApplyCoverageRuleResult(int ChangedInvestorCount, CoverageReviewDto Review);

public interface IMtgCoverageReviewService
{
    Task<CoverageReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<ApplyCoverageRuleResult> ApplyRuleAsync(long portfolioId, CancellationToken cancellationToken);

    Task<CoverageReviewDto> SetInvestorDecisionAsync(long portfolioId, long investorId, CoverageInvestorDecisionRequest request, CancellationToken cancellationToken);

    Task<CoverageReviewDto> DecideAsync(long portfolioId, CoverageDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Coverage Review" (APEX App 105 page 212): compare what each investor has paid with the
/// portfolio's required coverage, keep or exclude investors, then accept or reject the step.
/// </summary>
public sealed class MtgCoverageReviewService(ICapitraDbContext db) : IMtgCoverageReviewService
{
    public const string StepName = "Coverage review";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";
    public const string MixedDecision = "MIXED";

    /// <summary>
    /// Open after the valuation step is completed, and after its own rejection so the review can be redone.
    /// The workflow engine (build step 4) will open it through a coverage task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPortfolio.ValuationCompletedStatus, MtgPortfolio.CoverageReviewRejectedStatus];

    public async Task<CoverageReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode, p.RejectionReason, p.CoveragePct
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var accounts = await (
            from a in db.Set<MtgPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            where a.PortfolioId == portfolioId
            select new
            {
                a.InvestorId, i.InvestorCode, InvestorName = i.FullNameEn ?? i.FullNameAr,
                a.ContractAmount, a.PaidAmount, a.OutstandingAmount, a.DealDecisionCode
            }).ToListAsync(cancellationToken);

        var investors = accounts
            .GroupBy(a => a.InvestorId)
            .Select(g =>
            {
                var first = g.First();
                var contract = g.Sum(x => x.ContractAmount);
                var paid = g.Sum(x => x.PaidAmount);
                decimal? paidPct = contract > 0 ? Math.Round(paid / contract * 100, 2) : null;
                var decisions = g.Select(x => x.DealDecisionCode).Distinct().ToList();
                return new CoverageInvestorDto(
                    g.Key, first.InvestorCode, first.InvestorName, g.Count(), contract, paid, g.Sum(x => x.OutstandingAmount),
                    paidPct,
                    header.CoveragePct is { } required && contract > 0 ? paid / contract * 100 >= required : null,
                    decisions.Count == 1 ? decisions[0] : MixedDecision);
            })
            .OrderBy(x => x.InvestorCode)
            .ToList();

        return new CoverageReviewDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectName, header.ProjectCode, header.PortfolioStatusCode,
            header.StatusName, header.RejectionReason, OpenStatuses.Contains(header.PortfolioStatusCode), header.CoveragePct,
            investors);
    }

    /// <summary>
    /// APEX COV_APPLY: every investor whose paid share of contract value is below the required coverage is excluded,
    /// every other investor is kept. Investors with no contract value are left as they are.
    /// </summary>
    public async Task<ApplyCoverageRuleResult> ApplyRuleAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        if (portfolio.CoveragePct is not { } required)
            throw new ConflictException("Set the required coverage percentage on the portfolio first.");

        var accounts = await db.Set<MtgPortfolioAccount>().Where(x => x.PortfolioId == portfolioId).ToListAsync(cancellationToken);
        var changed = 0;
        foreach (var investor in accounts.GroupBy(x => x.InvestorId))
        {
            var contract = investor.Sum(x => x.ContractAmount);
            if (contract <= 0)
                continue;
            var decision = investor.Sum(x => x.PaidAmount) / contract * 100 < required
                ? MtgPortfolioAccount.ExcludedDecision
                : MtgPortfolioAccount.SelectedDecision;
            var toChange = investor.Where(x => x.DealDecisionCode != decision).ToList();
            toChange.ForEach(x => x.DealDecisionCode = decision);
            if (toChange.Count > 0)
                changed++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ApplyCoverageRuleResult(changed, await GetAsync(portfolioId, cancellationToken));
    }

    /// <summary>APEX COV_UPDATE_INVESTOR: keeps or excludes all of an investor's contracts.</summary>
    public async Task<CoverageReviewDto> SetInvestorDecisionAsync(
        long portfolioId, long investorId, CoverageInvestorDecisionRequest request, CancellationToken cancellationToken)
    {
        var decision = request.DealDecisionCode?.Trim().ToUpperInvariant();
        if (decision is not (MtgPortfolioAccount.SelectedDecision or MtgPortfolioAccount.ExcludedDecision))
            throw BusinessValidationException.For(nameof(request.DealDecisionCode), "Decision must be SELECTED or EXCLUDED.");

        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var accounts = await db.Set<MtgPortfolioAccount>()
            .Where(x => x.PortfolioId == portfolioId && x.InvestorId == investorId)
            .ToListAsync(cancellationToken);
        if (accounts.Count == 0)
            throw new NotFoundException("This investor has no contracts in the portfolio.");

        accounts.ForEach(x => x.DealDecisionCode = decision);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    /// <summary>APEX COV_DECISION. A comment is required for both actions.</summary>
    public async Task<CoverageReviewDto> DecideAsync(long portfolioId, CoverageDecisionRequest request, CancellationToken cancellationToken)
    {
        var comment = request.Comment?.Trim();
        if (string.IsNullOrEmpty(comment))
            throw BusinessValidationException.For(nameof(request.Comment), "A comment is required for both Accept and Reject.");
        if (comment.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid coverage review decision.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            portfolio.RejectionReason = comment;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.CoverageReviewRejectedStatus, comment, cancellationToken);
        }
        else
        {
            if (portfolio.CoveragePct is null)
                throw new ConflictException("Set the required coverage percentage on the portfolio before accepting.");

            // APEX counts a contract with no decision as kept; record that so the selected totals agree.
            var undecided = await db.Set<MtgPortfolioAccount>()
                .Where(x => x.PortfolioId == portfolioId && x.DealDecisionCode == null)
                .ToListAsync(cancellationToken);
            undecided.ForEach(x => x.DealDecisionCode = MtgPortfolioAccount.SelectedDecision);
            await db.SaveChangesAsync(cancellationToken);

            await PortfolioTotals.RecalculateSelectedAsync(db, portfolioId, cancellationToken);
            if (portfolio.SelectedAccountCount is null or 0)
                throw new ConflictException("At least one investor must stay in the deal before coverage can be accepted.");

            portfolio.RejectionReason = null;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.CoverageReviewAcceptedStatus, comment, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }
}
