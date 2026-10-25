using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.AmlReview;

/// <summary>Everything the AML review step shows for one portfolio.</summary>
public sealed record AmlReviewDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectName,
    string ProjectCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? RejectionReason,
    bool IsOpen, // checks, deal decisions and the accept/reject decision can be recorded
    AmlSubjectDto Developer,
    IReadOnlyList<AmlAccountDto> Accounts,
    IReadOnlyList<AmlResultDto> Results,
    AmlOptionsDto Options);

/// <summary>A screened party with its latest AML result (NOT_SCREENED when it has none).</summary>
public sealed record AmlSubjectDto(long SubjectId, string Code, string Name, string AmlStatusCode);

/// <summary>One investor contract and whether it stays in the deal.</summary>
public sealed record AmlAccountDto(
    long PortfolioAccountId,
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    string? UnitCode,
    string? SellerContractNo,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string? DealDecisionCode,
    string AmlStatusCode);

public sealed record AmlResultDto(
    long AmlResultId,
    string SubjectTypeCode,
    long SubjectId,
    string SubjectName,
    string AmlTypeCode,
    DateOnly ScreeningDate,
    string? ReferenceNo,
    string AmlResultCode,
    string RiskLevelCode,
    decimal? MatchScorePct,
    string? ResultDetails,
    string CreatedBy,
    DateTime CreatedAt);

public sealed record AmlOptionsDto(
    IReadOnlyList<string> AmlTypes,
    IReadOnlyList<string> Results,
    IReadOnlyList<string> RiskLevels);

public sealed record AddAmlResultRequest(
    string SubjectTypeCode,
    long SubjectId,
    string AmlTypeCode,
    DateOnly? ScreeningDate,
    string? ReferenceNo,
    string AmlResultCode,
    string RiskLevelCode,
    decimal? MatchScorePct,
    string? ResultDetails);

public sealed record AmlDecisionRequest(string Action, string? Comment);

public interface IMtgAmlReviewService
{
    Task<AmlReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<AmlResultDto> AddResultAsync(long portfolioId, AddAmlResultRequest request, CancellationToken cancellationToken);

    Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken);

    Task<AmlReviewDto> DecideAsync(long portfolioId, AmlDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "AML Review" (APEX App 105 page 209): screen the developer and every investor, decide which contracts
/// stay in the deal, then accept or reject the review.
/// </summary>
public sealed class MtgAmlReviewService(ICapitraDbContext db) : IMtgAmlReviewService
{
    public const string StepName = "AML review";
    public const string NotScreened = "NOT_SCREENED";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";

    /// <summary>
    /// The step is open once the portfolio data is loaded, and stays open after a rejection so the review can be redone.
    /// The workflow engine (build step 4) will open it through an AML review task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPurchasedPortfolio.DataLoadingCompletedStatus, MtgPurchasedPortfolio.AmlRejectedStatus];

    public async Task<AmlReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPurchasedPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join d in db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode, p.RejectionReason,
                p.DeveloperId, d.DeveloperCode, DeveloperName = d.LegalNameEn ?? d.LegalNameAr
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var results = await ResultsAsync(portfolioId, cancellationToken);
        string StatusOf(string subjectType, long subjectId) =>
            results.Where(r => r.SubjectTypeCode == subjectType && r.SubjectId == subjectId)
                .MaxBy(r => r.AmlResultId)?.AmlResultCode ?? NotScreened;

        var accounts = await (
            from a in db.Set<MtgPurchasedPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            where a.PortfolioId == portfolioId
            orderby i.InvestorCode, a.PortfolioAccountId
            select new
            {
                a.PortfolioAccountId, a.InvestorId, i.InvestorCode, InvestorName = i.FullNameEn ?? i.FullNameAr,
                UnitCode = u != null ? u.UnitCode : null, a.SellerContractNo, a.ContractAmount, a.PaidAmount,
                a.OutstandingAmount, a.DealDecisionCode
            }).ToListAsync(cancellationToken);

        return new AmlReviewDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectName, header.ProjectCode, header.PortfolioStatusCode,
            header.StatusName, header.RejectionReason, OpenStatuses.Contains(header.PortfolioStatusCode),
            new AmlSubjectDto(header.DeveloperId, header.DeveloperCode, header.DeveloperName,
                StatusOf(MtgAmlResult.DeveloperSubject, header.DeveloperId)),
            [.. accounts.Select(a => new AmlAccountDto(
                a.PortfolioAccountId, a.InvestorId, a.InvestorCode, a.InvestorName, a.UnitCode, a.SellerContractNo,
                a.ContractAmount, a.PaidAmount, a.OutstandingAmount, a.DealDecisionCode,
                StatusOf(MtgAmlResult.InvestorSubject, a.InvestorId)))],
            results,
            new AmlOptionsDto(MtgAmlResult.AmlTypes, MtgAmlResult.Results, MtgAmlResult.RiskLevels));
    }

    /// <summary>APEX AML_ADD_RESULT.</summary>
    public async Task<AmlResultDto> AddResultAsync(long portfolioId, AddAmlResultRequest request, CancellationToken cancellationToken)
    {
        var r = request;
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        var errors = new ValidationErrors();
        var subjectType = Pick(errors, nameof(r.SubjectTypeCode), "Subject type", r.SubjectTypeCode, MtgAmlResult.SubjectTypes);
        var amlType = Pick(errors, nameof(r.AmlTypeCode), "Check type", r.AmlTypeCode, MtgAmlResult.AmlTypes);
        var result = Pick(errors, nameof(r.AmlResultCode), "Result", r.AmlResultCode, MtgAmlResult.Results);
        var risk = Pick(errors, nameof(r.RiskLevelCode), "Risk level", r.RiskLevelCode, MtgAmlResult.RiskLevels);
        if (r.ScreeningDate is null)
            errors.Add(nameof(r.ScreeningDate), "Screening date is required.");
        var reference = errors.Text(nameof(r.ReferenceNo), "Reference no.", r.ReferenceNo, 100);
        var details = errors.Text(nameof(r.ResultDetails), "Result details", r.ResultDetails, 4000);
        if (r.MatchScorePct is { } score && (score < 0 || score > 100))
            errors.Add(nameof(r.MatchScorePct), "Match Score must be between 0 and 100.");

        if (subjectType is not null && !await BelongsToPortfolioAsync(portfolio, subjectType, r.SubjectId, cancellationToken))
            errors.Add(nameof(r.SubjectId), "Selected subject does not belong to this portfolio.");
        errors.ThrowIfAny();

        var entity = new MtgAmlResult
        {
            PortfolioId = portfolioId,
            SubjectTypeCode = subjectType!,
            SubjectId = r.SubjectId,
            AmlTypeCode = amlType!,
            ScreeningDate = r.ScreeningDate!.Value,
            ReferenceNo = reference,
            AmlResultCode = result!,
            RiskLevelCode = risk!,
            MatchScorePct = r.MatchScorePct,
            ResultDetails = details
        };
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return (await ResultsAsync(portfolioId, cancellationToken)).Single(x => x.AmlResultId == entity.AmlResultId);
    }

    /// <summary>APEX AML_UPDATE_INVESTOR, for one or many contracts at once.</summary>
    public async Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        await DealDecisions.SaveAsync(db, portfolioId, request, allowReinclude: true, cancellationToken);
    }

    /// <summary>APEX AML_DECISION.</summary>
    public async Task<AmlReviewDto> DecideAsync(long portfolioId, AmlDecisionRequest request, CancellationToken cancellationToken)
    {
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        if (comment is null)
            throw BusinessValidationException.For(nameof(request.Comment), "A comment is required for both AML Accept and Reject.");
        if (comment.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid AML decision.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            portfolio.RejectionReason = comment;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.AmlRejectedStatus, comment, cancellationToken);
        }
        else
        {
            await EnsureReadyToAcceptAsync(portfolio, cancellationToken);
            await PortfolioTotals.RecalculateSelectedAsync(db, portfolioId, cancellationToken);
            portfolio.RejectionReason = null;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.AmlAcceptedStatus, comment, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private async Task EnsureReadyToAcceptAsync(MtgPurchasedPortfolio portfolio, CancellationToken cancellationToken)
    {
        var results = db.Set<MtgAmlResult>().Where(x => x.PortfolioId == portfolio.PortfolioId);

        if (!await results.AnyAsync(x => x.SubjectTypeCode == MtgAmlResult.DeveloperSubject && x.SubjectId == portfolio.DeveloperId, cancellationToken))
            throw new ConflictException("AML information is required for the portfolio developer.");

        var accounts = db.Set<MtgPurchasedPortfolioAccount>().Where(x => x.PortfolioId == portfolio.PortfolioId);
        if (await accounts.AnyAsync(a => !results.Any(x => x.SubjectTypeCode == MtgAmlResult.InvestorSubject && x.SubjectId == a.InvestorId), cancellationToken))
            throw new ConflictException("AML information is missing for one or more portfolio investors.");

        if (await accounts.AnyAsync(a => a.DealDecisionCode != MtgPurchasedPortfolioAccount.SelectedDecision && a.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision, cancellationToken))
            throw new ConflictException("Every investor must be either Selected or Excluded before AML can be accepted.");
    }

    private async Task<bool> BelongsToPortfolioAsync(MtgPurchasedPortfolio portfolio, string subjectType, long subjectId, CancellationToken cancellationToken) =>
        subjectType == MtgAmlResult.DeveloperSubject
            ? portfolio.DeveloperId == subjectId
            : await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(x => x.PortfolioId == portfolio.PortfolioId && x.InvestorId == subjectId, cancellationToken);

    /// <summary>The screening log, newest first, with the name of each subject.</summary>
    private async Task<IReadOnlyList<AmlResultDto>> ResultsAsync(long portfolioId, CancellationToken cancellationToken) =>
        await (
            from x in db.Set<MtgAmlResult>()
            where x.PortfolioId == portfolioId
            let developer = db.Set<MtgDeveloper>().FirstOrDefault(d => x.SubjectTypeCode == MtgAmlResult.DeveloperSubject && d.DeveloperId == x.SubjectId)
            let investor = db.Set<MtgInvestor>().FirstOrDefault(i => x.SubjectTypeCode == MtgAmlResult.InvestorSubject && i.InvestorId == x.SubjectId)
            orderby x.AmlResultId descending
            select new AmlResultDto(
                x.AmlResultId, x.SubjectTypeCode, x.SubjectId,
                developer != null ? developer.LegalNameEn ?? developer.LegalNameAr : investor != null ? investor.FullNameEn ?? investor.FullNameAr : "-",
                x.AmlTypeCode, x.ScreeningDate, x.ReferenceNo, x.AmlResultCode, x.RiskLevelCode, x.MatchScorePct,
                x.ResultDetails, x.CreatedBy, x.CreatedAt))
            .ToListAsync(cancellationToken);

    /// <summary>One of a fixed list of codes (APEX checks them in the page process), upper-cased.</summary>
    private static string? Pick(ValidationErrors errors, string field, string label, string? value, IReadOnlyList<string> allowed)
    {
        var code = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
        if (code is null)
            errors.Add(field, $"{label} is required.");
        else if (!allowed.Contains(code))
            errors.Add(field, $"Invalid {label.ToLowerInvariant()}.");
        return code is not null && allowed.Contains(code) ? code : null;
    }
}
