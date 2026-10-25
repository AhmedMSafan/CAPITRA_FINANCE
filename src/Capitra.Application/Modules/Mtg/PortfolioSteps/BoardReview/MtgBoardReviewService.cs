using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.BoardReview;

/// <summary>The board pack for one portfolio, and the board's decision on it.</summary>
public sealed record BoardReviewDto(
    long PortfolioId,
    string PortfolioNo,
    string DeveloperName,
    string ProjectName,
    string ProjectCode,
    string CurrencyCode,
    DateOnly RequestDate,
    DateOnly? SubmittedDate,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? RejectionReason,
    bool IsOpen, // the board decision can be recorded
    BoardTermsDto Terms,
    BoardTotalsDto Totals,
    BoardPricingDto? Pricing, // null until the financial review is saved
    BoardDueDiligenceDto DueDiligence,
    IReadOnlyList<BoardInvestorDto> Investors,
    IReadOnlyList<BoardHistoryDto> History,
    BoardDecisionDto? Decision); // null until the board has decided

public sealed record BoardTermsDto(
    decimal? CoveragePct,
    decimal? MarginPct,
    decimal? PvRate,
    decimal? ReservedAmountPct,
    decimal? LateFeeRate,
    decimal? AdminExpensePct,
    decimal? LegalFeePerUnitAmount,
    decimal? ValuationFeePerUnitAmount,
    int? TermMonths);

/// <summary>The portfolio's contracts, all of them and the ones kept in the deal, and the kept investors' cheques.</summary>
public sealed record BoardTotalsDto(
    int ContractCount,
    int InvestorCount,
    decimal FaceValueAmount,
    decimal OutstandingAmount,
    decimal PaidAmount,
    int SelectedContractCount,
    int SelectedInvestorCount,
    int SelectedUnitCount,
    decimal SelectedFaceValueAmount,
    decimal SelectedOutstandingAmount,
    decimal SelectedPaidAmount,
    int ChequeCount,
    decimal ChequeAmount);

/// <summary>The saved financial review (page 214).</summary>
public sealed record BoardPricingDto(
    string PvMethodCode,
    decimal PvRate,
    DateOnly PvBaseDate,
    int ChequeCount,
    decimal ChequesFaceAmount,
    decimal ChequesPvAmount,
    decimal MarginPct,
    decimal MarginAmount,
    decimal AdminDeductedAmount,
    decimal LegalDeductedAmount,
    decimal ValuationDeductedAmount,
    decimal TotalChargesAmount,
    decimal ProposedPurchaseAmount,
    decimal ExpectedReturnAmount);

public sealed record BoardDueDiligenceDto(
    int AmlCount,
    int AmlFlaggedCount, // any result other than CLEAR
    int InquiryCount,
    int InquiryHighRiskCount, // HIGH or CRITICAL
    BoardLegalDto? Legal,
    BoardValuationDto? Valuation); // the latest valuation

public sealed record BoardLegalDto(
    bool IsOwnershipVerified,
    bool IsBuildingLicenseVerified,
    bool IsTitleDeedVerified,
    bool IsEncumbranceFree,
    bool IsRegistrationPossible,
    bool IsMortgagePossible,
    string? LegalRecommendation,
    DateOnly? ReviewDate);

public sealed record BoardValuationDto(
    string ValuerName, DateOnly ValuationDate, decimal MarketValueAmount, decimal? RecommendedValueAmount, decimal? ProjectCompletionPct);

public sealed record BoardInvestorDto(
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    int ContractCount,
    int SelectedContractCount,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal ChequeAmount,
    bool IsIncluded);

public sealed record BoardHistoryDto(string? FromStatusName, string ToStatusName, string? Comments, string ChangedBy, DateTime ChangedAt);

public sealed record BoardDecisionDto(string BoardDecisionCode, string? BoardDecisionNo, DateOnly? BoardDecisionDate, DateOnly? ApprovedDate);

public sealed record BoardDecisionRequest(string Action, string DecisionNo, DateOnly? DecisionDate, string Comment);

public interface IMtgBoardReviewService
{
    Task<BoardReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<BoardReviewDto> DecideAsync(long portfolioId, BoardDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Board Review" (APEX App 105 page 215): the board reads one report of everything the earlier
/// steps found and the price the financial review worked out, then the board's resolution (number, date, comment) is
/// recorded as an approval or a rejection.
/// </summary>
public sealed class MtgBoardReviewService(ICapitraDbContext db, IClock clock) : IMtgBoardReviewService
{
    public const string StepName = "Board review";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>
    /// Open once the financial review is accepted, and after its own rejection so the decision can be recorded again.
    /// The workflow engine (build step 4) will open it through a board review task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPurchasedPortfolio.FinancialReviewAcceptedStatus, MtgPurchasedPortfolio.BoardReviewRejectedStatus];

    public async Task<BoardReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from po in db.Set<MtgPurchasedPortfolio>()
            join d in db.Set<MtgDeveloper>() on po.DeveloperId equals d.DeveloperId
            join prj in db.Set<MtgProject>() on po.ProjectId equals prj.ProjectId
            where po.PortfolioId == portfolioId
            select new { Portfolio = po, DeveloperName = d.LegalNameEn ?? d.LegalNameAr, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode })
            .AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");
        var p = header.Portfolio;

        var statusNames = await db.Set<MtgLkpPortfolioStatus>().AsNoTracking()
            .ToDictionaryAsync(x => x.Code, x => x.NameEn, cancellationToken);
        string StatusName(string code) => statusNames.GetValueOrDefault(code) ?? code;

        // A contract with no deal decision counts as kept, as in APEX.
        var accounts = await (
            from a in db.Set<MtgPurchasedPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            where a.PortfolioId == portfolioId
            select new
            {
                a.PortfolioAccountId, a.InvestorId, i.InvestorCode, InvestorName = i.FullNameEn ?? i.FullNameAr, a.ProjectUnitId,
                a.ContractAmount, a.PaidAmount, a.OutstandingAmount, IsKept = a.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision
            }).AsNoTracking().ToListAsync(cancellationToken);
        var cheques = await db.Set<MtgPurchasedPortfolioCheque>().AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId)
            .Select(x => new { x.PortfolioAccountId, x.ChequeAmount })
            .ToListAsync(cancellationToken);
        var investorOf = accounts.ToDictionary(x => x.PortfolioAccountId, x => x.InvestorId);
        var keptAccounts = accounts.Where(x => x.IsKept).Select(x => x.PortfolioAccountId).ToHashSet();
        var keptCheques = cheques.Where(x => keptAccounts.Contains(x.PortfolioAccountId)).ToList();
        var kept = accounts.Where(x => x.IsKept).ToList();

        var totals = new BoardTotalsDto(
            accounts.Count, accounts.Select(x => x.InvestorId).Distinct().Count(),
            accounts.Sum(x => x.ContractAmount), accounts.Sum(x => x.OutstandingAmount), accounts.Sum(x => x.PaidAmount),
            kept.Count, kept.Select(x => x.InvestorId).Distinct().Count(), kept.Select(x => x.ProjectUnitId).Distinct().Count(),
            kept.Sum(x => x.ContractAmount), kept.Sum(x => x.OutstandingAmount), kept.Sum(x => x.PaidAmount),
            keptCheques.Count, keptCheques.Sum(x => x.ChequeAmount));

        // Kept investors first, as the board reads them.
        var investors = accounts
            .GroupBy(x => x.InvestorId)
            .Select(g => new BoardInvestorDto(
                g.Key, g.First().InvestorCode, g.First().InvestorName, g.Count(), g.Count(x => x.IsKept),
                g.Sum(x => x.ContractAmount), g.Sum(x => x.PaidAmount), g.Sum(x => x.OutstandingAmount),
                cheques.Where(c => investorOf.GetValueOrDefault(c.PortfolioAccountId) == g.Key).Sum(c => c.ChequeAmount),
                g.Any(x => x.IsKept)))
            .OrderByDescending(x => x.SelectedContractCount).ThenBy(x => x.InvestorCode, StringComparer.Ordinal)
            .ToList();

        var review = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        var pricing = review is null ? null : new BoardPricingDto(
            review.PvMethodCode, review.PvRate, review.PvBaseDate, review.ChequeCount, review.ChequesFaceAmount, review.ChequesPvAmount,
            review.MarginPct, review.MarginAmount,
            review.IsAdminIncluded ? review.AdminFinalAmount : 0, review.IsLegalIncluded ? review.LegalFinalAmount : 0,
            review.IsValuationIncluded ? review.ValuationFinalAmount : 0,
            review.TotalChargesAmount, review.ProposedPurchaseAmount, review.ExpectedReturnAmount);

        var amlCodes = await db.Set<MtgAmlResult>().AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId).Select(x => x.AmlResultCode).ToListAsync(cancellationToken);
        var inquiryRisks = await db.Set<MtgInquiryResult>().AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId).Select(x => x.RiskLevelCode).ToListAsync(cancellationToken);
        var legal = await db.Set<MtgLegalReview>().AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId)
            .Select(x => new BoardLegalDto(
                x.IsOwnershipVerified, x.IsBuildingLicenseVerified, x.IsTitleDeedVerified, x.IsEncumbranceFree,
                x.IsRegistrationPossible, x.IsMortgagePossible, x.LegalRecommendation, x.ReviewDate))
            .FirstOrDefaultAsync(cancellationToken);
        var valuation = await db.Set<MtgValuation>().AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId)
            .OrderByDescending(x => x.ValuationDate).ThenByDescending(x => x.ValuationId)
            .Select(x => new BoardValuationDto(x.ValuerName, x.ValuationDate, x.MarketValueAmount, x.RecommendedValueAmount, x.ProjectCompletionPct))
            .FirstOrDefaultAsync(cancellationToken);
        var dueDiligence = new BoardDueDiligenceDto(
            amlCodes.Count, amlCodes.Count(x => !string.Equals(x, "CLEAR", StringComparison.OrdinalIgnoreCase)),
            inquiryRisks.Count, inquiryRisks.Count(x => x?.ToUpperInvariant() is "HIGH" or "CRITICAL"),
            legal, valuation);

        var history = (await db.Set<MtgPurchasedPortfolioStatusHistory>().AsNoTracking()
                .Where(x => x.PortfolioId == portfolioId)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.PortfolioStatusHistoryId)
                .ToListAsync(cancellationToken))
            .Select(x => new BoardHistoryDto(
                x.FromStatusCode is null ? null : StatusName(x.FromStatusCode), StatusName(x.ToStatusCode), x.Comments, x.CreatedBy, x.CreatedAt))
            .ToList();

        return new BoardReviewDto(
            p.PortfolioId, p.PortfolioNo, header.DeveloperName, header.ProjectName, header.ProjectCode, p.CurrencyCode,
            p.RequestDate, p.SubmittedDate, p.PortfolioStatusCode, StatusName(p.PortfolioStatusCode), p.RejectionReason,
            OpenStatuses.Contains(p.PortfolioStatusCode),
            new BoardTermsDto(p.CoveragePct, p.MarginPct, p.PvRate, p.ReservedAmountPct, p.LateFeeRate, p.AdminExpensePct,
                p.LegalFeePerUnitAmount, p.ValuationFeePerUnitAmount, p.TermMonths),
            totals, pricing, dueDiligence, investors, history,
            p.BoardDecisionCode is null ? null : new BoardDecisionDto(p.BoardDecisionCode, p.BoardDecisionNo, p.BoardDecisionDate, p.ApprovedDate));
    }

    /// <summary>APEX BOARD_DECISION. The resolution number, date and a comment are all required.</summary>
    public async Task<BoardReviewDto> DecideAsync(long portfolioId, BoardDecisionRequest request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            errors.Add(nameof(request.Action), "Invalid board decision.");
        var decisionNo = errors.Text(nameof(request.DecisionNo), "Board decision number", request.DecisionNo, 200, required: true);
        if (request.DecisionDate is null)
            errors.Add(nameof(request.DecisionDate), "The board decision date is required.");
        else if (request.DecisionDate > Today())
            errors.Add(nameof(request.DecisionDate), "The board decision date cannot be in the future.");
        var comment = request.Comment?.Trim();
        if (string.IsNullOrEmpty(comment))
            errors.Add(nameof(request.Comment), "A comment is required for the board decision.");
        else if (comment.Length > 4000)
            errors.Add(nameof(request.Comment), "Comment is limited to 4000 characters.");
        errors.ThrowIfAny();

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        var accepted = action == AcceptAction;
        if (accepted && !await db.Set<MtgPurchasedPortfolioFinancialReview>().AnyAsync(x => x.PortfolioId == portfolioId, cancellationToken))
            throw new ConflictException("The financial review has not been saved, so there is no price to approve.");
        portfolio.BoardDecisionCode = accepted ? MtgPurchasedPortfolio.BoardAccepted : MtgPurchasedPortfolio.BoardRejected;
        portfolio.BoardDecisionNo = decisionNo;
        portfolio.BoardDecisionDate = request.DecisionDate;
        portfolio.ApprovedDate = accepted ? Today() : null;
        portfolio.SubmittedDate ??= Today();
        portfolio.Comments = comment;
        portfolio.RejectionReason = accepted ? null : comment;
        await PortfolioStatus.ChangeAsync(
            db, portfolio, accepted ? MtgPurchasedPortfolio.BoardReviewAcceptedStatus : MtgPurchasedPortfolio.BoardReviewRejectedStatus,
            $"Board decision {decisionNo}: {comment}", cancellationToken);
        await db.SaveChangesAsync(cancellationToken); // a second rejection keeps the status but records the new resolution

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));
}
