using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.FinancialReview;

/// <summary>Everything the financial review step shows for one portfolio.</summary>
public sealed record FinancialReviewDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectName,
    string ProjectCode,
    string CurrencyCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? RejectionReason,
    bool IsOpen, // the review can be saved and the step decided
    FinancialTerms Terms,
    FinancialReviewInputs Inputs, // as saved, or the defaults
    FinancialCalculationDto? Calculation, // today's figures for those inputs; null until the PV rate is set
    SavedFinancialReviewDto? Saved);

/// <summary>The figures as last saved, which are what accepting the step uses.</summary>
public sealed record SavedFinancialReviewDto(
    DateOnly PvBaseDate,
    int ChequeCount,
    decimal ChequesPvAmount,
    decimal TotalChargesAmount,
    decimal ProposedPurchaseAmount,
    decimal ExpectedReturnAmount,
    string SavedBy,
    DateTime SavedAt,
    int RowVersion);

public sealed record SaveFinancialReviewRequest(FinancialReviewInputs Inputs, int? RowVersion);

public sealed record FinancialReviewDecisionRequest(string Action, string Comment);

public interface IMtgFinancialReviewService
{
    Task<FinancialReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<FinancialCalculationDto> CalculateAsync(long portfolioId, FinancialReviewInputs inputs, CancellationToken cancellationToken);

    Task<FinancialReviewDto> SaveAsync(long portfolioId, SaveFinancialReviewRequest request, CancellationToken cancellationToken);

    Task<FinancialReviewDto> DecideAsync(long portfolioId, FinancialReviewDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Financial Review" (APEX App 105 page 214): price the portfolio from the cheques of the
/// investors kept in the deal, save the figures, then accept (send to the board) or reject the step.
/// </summary>
public sealed class MtgFinancialReviewService(ICapitraDbContext db, IClock clock) : IMtgFinancialReviewService
{
    public const string StepName = "Financial review";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>
    /// Open after the cheque coverage is accepted, and after its own rejection so the review can be redone.
    /// The workflow engine (build step 4) will open it through a financial review task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPurchasedPortfolio.ChequeCoverageAcceptedStatus, MtgPurchasedPortfolio.FinancialReviewRejectedStatus];

    public async Task<FinancialReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPurchasedPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode, p.CurrencyCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode, p.RejectionReason,
                Terms = new FinancialTerms(p.PvRate, p.MarginPct, p.AdminExpensePct, p.LegalFeePerUnitAmount, p.ValuationFeePerUnitAmount)
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var saved = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        var inputs = saved is null
            ? FinancialReviewInputs.Default
            : new FinancialReviewInputs(
                saved.PvMethodCode, saved.MarginBaseCode, saved.AdminBaseCode,
                saved.AdminFinalAmount == saved.AdminCalcAmount ? null : saved.AdminFinalAmount, saved.IsAdminIncluded,
                saved.LegalFinalAmount == saved.LegalCalcAmount ? null : saved.LegalFinalAmount, saved.IsLegalIncluded,
                saved.ValuationFinalAmount == saved.ValuationCalcAmount ? null : saved.ValuationFinalAmount, saved.IsValuationIncluded);

        return new FinancialReviewDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectName, header.ProjectCode, header.CurrencyCode,
            header.PortfolioStatusCode, header.StatusName, header.RejectionReason, OpenStatuses.Contains(header.PortfolioStatusCode),
            header.Terms, inputs,
            header.Terms.PvRate is null ? null : await CalculateCoreAsync(portfolioId, header.Terms, inputs, cancellationToken),
            saved is null ? null : new SavedFinancialReviewDto(
                saved.PvBaseDate, saved.ChequeCount, saved.ChequesPvAmount, saved.TotalChargesAmount, saved.ProposedPurchaseAmount,
                saved.ExpectedReturnAmount, saved.UpdatedBy ?? saved.CreatedBy, saved.UpdatedAt ?? saved.CreatedAt, saved.RowVersion));
    }

    /// <summary>The figures for these choices, without saving them.</summary>
    public async Task<FinancialCalculationDto> CalculateAsync(long portfolioId, FinancialReviewInputs inputs, CancellationToken cancellationToken)
    {
        var portfolio = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken)
                        ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");
        return await CalculateCoreAsync(portfolioId, TermsOf(portfolio), Validate(inputs), cancellationToken);
    }

    /// <summary>APEX FR_SAVE: works the figures out again, keeps them, and copies the price to the portfolio.</summary>
    public async Task<FinancialReviewDto> SaveAsync(long portfolioId, SaveFinancialReviewRequest request, CancellationToken cancellationToken)
    {
        var inputs = Validate(request.Inputs);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var c = await CalculateCoreAsync(portfolioId, TermsOf(portfolio), inputs, cancellationToken);

        var review = await db.Set<MtgPurchasedPortfolioFinancialReview>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        if (review is null)
        {
            review = new MtgPurchasedPortfolioFinancialReview { PortfolioId = portfolioId };
            db.Add(review);
        }
        else if (request.RowVersion != review.RowVersion)
        {
            throw new ConflictException("The financial review was changed by someone else. Reload it and try again.");
        }

        review.PvMethodCode = c.PvMethodCode;
        review.PvRate = c.PvRate;
        review.PvBaseDate = c.PvBaseDate;
        review.ChequeCount = c.ChequeCount;
        review.ChequesFaceAmount = c.ChequesFaceAmount;
        review.ChequesPvAmount = c.ChequesPvAmount;
        review.MarginBaseCode = c.MarginBaseCode;
        review.MarginPct = c.MarginPct;
        review.MarginBaseAmount = c.MarginBaseAmount;
        review.MarginAmount = c.MarginAmount;
        review.AdminBaseCode = c.AdminBaseCode;
        review.AdminExpensePct = c.AdminExpensePct;
        review.AdminBaseAmount = c.AdminBaseAmount;
        review.AdminCalcAmount = c.AdminCalcAmount;
        review.AdminFinalAmount = c.AdminFinalAmount;
        review.IsAdminIncluded = c.IsAdminIncluded;
        review.UnitCount = c.UnitCount;
        review.LegalFeePerUnitAmount = c.LegalFeePerUnitAmount;
        review.LegalCalcAmount = c.LegalCalcAmount;
        review.LegalFinalAmount = c.LegalFinalAmount;
        review.IsLegalIncluded = c.IsLegalIncluded;
        review.ValuationFeePerUnitAmount = c.ValuationFeePerUnitAmount;
        review.ValuationCalcAmount = c.ValuationCalcAmount;
        review.ValuationFinalAmount = c.ValuationFinalAmount;
        review.IsValuationIncluded = c.IsValuationIncluded;
        review.TotalChargesAmount = c.TotalChargesAmount;
        review.ProposedPurchaseAmount = c.ProposedPurchaseAmount;
        review.ExpectedReturnAmount = c.ExpectedReturnAmount;

        portfolio.ProposedPurchaseAmount = c.ProposedPurchaseAmount;
        portfolio.ExpectedReturnAmount = c.ExpectedReturnAmount;
        portfolio.AdminExpenseAmount = c.IsAdminIncluded ? c.AdminFinalAmount : 0;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    /// <summary>APEX FR_DECISION. A comment is required; accept uses the saved figures.</summary>
    public async Task<FinancialReviewDto> DecideAsync(long portfolioId, FinancialReviewDecisionRequest request, CancellationToken cancellationToken)
    {
        var comment = request.Comment?.Trim();
        if (string.IsNullOrEmpty(comment))
            throw BusinessValidationException.For(nameof(request.Comment), "A comment is required for the decision.");
        if (comment.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid financial review decision.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            portfolio.RejectionReason = comment;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.FinancialReviewRejectedStatus, comment, cancellationToken);
        }
        else
        {
            var review = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken)
                         ?? throw new ConflictException("Save the financial review before completing the step.");
            if (review.ChequeCount == 0)
                throw new ConflictException("There are no cheques to value for the included investors.");
            if (review.ProposedPurchaseAmount <= 0)
                throw new ConflictException("The proposed purchase value must be above zero.");

            portfolio.RejectionReason = null;
            portfolio.SubmittedDate = Today();
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.FinancialReviewAcceptedStatus, comment, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private async Task<FinancialCalculationDto> CalculateCoreAsync(
        long portfolioId, FinancialTerms terms, FinancialReviewInputs inputs, CancellationToken cancellationToken)
    {
        if (terms.PvRate is null)
            throw new ConflictException("Set the Present Value Calculation Rate on the portfolio first.");

        // A contract with no deal decision counts as kept, as in APEX.
        var kept = await db.Set<MtgPurchasedPortfolioAccount>()
            .Where(x => x.PortfolioId == portfolioId && x.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision)
            .Select(x => new { x.OutstandingAmount, x.ContractAmount, x.ProjectUnitId })
            .ToListAsync(cancellationToken);

        var cheques = await (
            from c in db.Set<MtgPurchasedPortfolioCheque>()
            join a in db.Set<MtgPurchasedPortfolioAccount>() on c.PortfolioAccountId equals a.PortfolioAccountId
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            where c.PortfolioId == portfolioId && a.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision
            orderby c.DueDate, c.ChequeId
            select new ChequeToValue(c.ChequeId, c.ChequeNo, i.FullNameEn ?? i.FullNameAr, c.DueDate, c.ChequeAmount))
            .ToListAsync(cancellationToken);

        return FinancialReviewCalculator.Calculate(
            inputs, terms,
            new KeptContracts(kept.Sum(x => x.OutstandingAmount), kept.Sum(x => x.ContractAmount),
                kept.Select(x => x.ProjectUnitId).Distinct().Count()),
            cheques, Today());
    }

    private static FinancialReviewInputs Validate(FinancialReviewInputs? inputs)
    {
        if (inputs is null)
            throw BusinessValidationException.For("Inputs", "The financial review choices are required.");
        var method = Code(inputs.PvMethodCode, FinancialReviewCalculator.AnnualMethod);
        var marginBase = Code(inputs.MarginBaseCode, FinancialReviewCalculator.SelectedOutstandingBase);
        var adminBase = Code(inputs.AdminBaseCode, FinancialReviewCalculator.SelectedOutstandingBase);
        if (!FinancialReviewCalculator.PvMethods.Contains(method))
            throw BusinessValidationException.For(nameof(inputs.PvMethodCode), "Unknown present value method.");
        if (!FinancialReviewCalculator.MarginBases.Contains(marginBase))
            throw BusinessValidationException.For(nameof(inputs.MarginBaseCode), "Unknown margin base.");
        if (!FinancialReviewCalculator.AdminBases.Contains(adminBase))
            throw BusinessValidationException.For(nameof(inputs.AdminBaseCode), "Unknown admin expense base.");
        if (inputs.AdminFinalAmount < 0 || inputs.LegalFinalAmount < 0 || inputs.ValuationFinalAmount < 0)
            throw BusinessValidationException.For("Charges", "Charges cannot be negative.");
        return inputs with { PvMethodCode = method, MarginBaseCode = marginBase, AdminBaseCode = adminBase };
    }

    private static string Code(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToUpperInvariant();

    private static FinancialTerms TermsOf(MtgPurchasedPortfolio p) =>
        new(p.PvRate, p.MarginPct, p.AdminExpensePct, p.LegalFeePerUnitAmount, p.ValuationFeePerUnitAmount);

    private DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));
}
