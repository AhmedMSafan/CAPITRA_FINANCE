using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.LegalReview;

/// <summary>Everything the legal review step shows for one portfolio.</summary>
public sealed record LegalReviewDto(
    long PortfolioId,
    string PortfolioNo,
    string DeveloperName,
    string ProjectName,
    string ProjectCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    bool IsOpen, // the checklist, deal decisions and the step decision can be recorded
    LegalReviewFormDto? Review, // null until the checklist is first saved
    IReadOnlyList<LegalReviewAccountDto> Accounts);

public sealed record LegalReviewFormDto(
    bool IsOwnershipVerified,
    bool IsBuildingLicenseVerified,
    bool IsRegistrationPossible,
    bool IsMortgagePossible,
    bool IsEncumbranceFree,
    bool IsTitleDeedVerified,
    string? LegalFindings,
    string? LegalRecommendation,
    string? ReviewedBy,
    DateOnly? ReviewDate,
    string? ApprovedBy,
    DateOnly? ApprovedDate,
    bool IsComplete,
    string SavedBy,
    DateTime SavedAt,
    int RowVersion);

public sealed record LegalReviewAccountDto(
    long PortfolioAccountId,
    string InvestorCode,
    string InvestorName,
    string? UnitCode,
    string? SellerContractNo,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string? DealDecisionCode);

public sealed record SaveLegalReviewRequest(
    bool IsOwnershipVerified,
    bool IsBuildingLicenseVerified,
    bool IsRegistrationPossible,
    bool IsMortgagePossible,
    bool IsEncumbranceFree,
    bool IsTitleDeedVerified,
    string? LegalFindings,
    string? LegalRecommendation,
    string? ReviewedBy,
    DateOnly? ReviewDate,
    string? ApprovedBy,
    DateOnly? ApprovedDate,
    int? RowVersion); // null for the first save

public sealed record LegalDecisionRequest(string Action, string? Comment);

public interface IMtgLegalReviewService
{
    Task<LegalReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<LegalReviewDto> SaveAsync(long portfolioId, SaveLegalReviewRequest request, CancellationToken cancellationToken);

    Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken);

    Task<LegalReviewDto> DecideAsync(long portfolioId, LegalDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Legal Review" (APEX App 105 page 211): complete the six legal checks with findings and
/// sign-off, review which investors stay in the deal, then accept or reject the step.
/// </summary>
public sealed class MtgLegalReviewService(ICapitraDbContext db) : IMtgLegalReviewService
{
    public const string StepName = "Legal review";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";

    /// <summary>
    /// Open after the inquiries step is completed, and after its own rejection so the review can be redone.
    /// The workflow engine (build step 4) will open it through a legal review task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPortfolio.InquiryAcceptedStatus, MtgPortfolio.LegalReviewRejectedStatus];

    public async Task<LegalReviewDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPortfolio>()
            join d in db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, DeveloperName = d.LegalNameEn ?? d.LegalNameAr,
                ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var review = await db.Set<MtgLegalReview>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);

        var accounts = await (
            from a in db.Set<MtgPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            where a.PortfolioId == portfolioId
            orderby i.InvestorCode, a.PortfolioAccountId
            select new LegalReviewAccountDto(
                a.PortfolioAccountId, i.InvestorCode, i.FullNameEn ?? i.FullNameAr, u != null ? u.UnitCode : null,
                a.SellerContractNo, a.ContractAmount, a.PaidAmount, a.OutstandingAmount, a.DealDecisionCode))
            .ToListAsync(cancellationToken);

        return new LegalReviewDto(
            header.PortfolioId, header.PortfolioNo, header.DeveloperName, header.ProjectName, header.ProjectCode,
            header.PortfolioStatusCode, header.StatusName, OpenStatuses.Contains(header.PortfolioStatusCode),
            review is null ? null : new LegalReviewFormDto(
                review.IsOwnershipVerified, review.IsBuildingLicenseVerified, review.IsRegistrationPossible,
                review.IsMortgagePossible, review.IsEncumbranceFree, review.IsTitleDeedVerified,
                review.LegalFindings, review.LegalRecommendation, review.ReviewedBy, review.ReviewDate,
                review.ApprovedBy, review.ApprovedDate, review.IsComplete,
                review.UpdatedBy ?? review.CreatedBy, review.UpdatedAt ?? review.CreatedAt, review.RowVersion),
            accounts);
    }

    /// <summary>APEX LEGAL_SAVE: the first save creates the portfolio's review, later saves change it.</summary>
    public async Task<LegalReviewDto> SaveAsync(long portfolioId, SaveLegalReviewRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        var errors = new ValidationErrors();
        var findings = errors.Text(nameof(request.LegalFindings), "Legal findings", request.LegalFindings, 4000);
        var recommendation = errors.Text(nameof(request.LegalRecommendation), "Recommendation", request.LegalRecommendation, 4000);
        var reviewedBy = errors.Text(nameof(request.ReviewedBy), "Reviewed by", request.ReviewedBy, 255);
        var approvedBy = errors.Text(nameof(request.ApprovedBy), "Approved by", request.ApprovedBy, 255);
        errors.ThrowIfAny();

        var review = await db.Set<MtgLegalReview>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        if (review is null)
        {
            review = new MtgLegalReview { PortfolioId = portfolioId };
            db.Add(review);
        }
        else if (request.RowVersion != review.RowVersion)
        {
            throw new ConflictException("The legal review was changed by someone else. Reload it and try again.");
        }

        review.IsOwnershipVerified = request.IsOwnershipVerified;
        review.IsBuildingLicenseVerified = request.IsBuildingLicenseVerified;
        review.IsRegistrationPossible = request.IsRegistrationPossible;
        review.IsMortgagePossible = request.IsMortgagePossible;
        review.IsEncumbranceFree = request.IsEncumbranceFree;
        review.IsTitleDeedVerified = request.IsTitleDeedVerified;
        review.LegalFindings = findings;
        review.LegalRecommendation = recommendation;
        review.ReviewedBy = reviewedBy;
        review.ReviewDate = request.ReviewDate;
        review.ApprovedBy = approvedBy;
        review.ApprovedDate = request.ApprovedDate;

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    /// <summary>APEX LEGAL_UPDATE_INVESTOR: contracts can be included again, as in the AML review.</summary>
    public async Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        await DealDecisions.SaveAsync(db, portfolioId, request, allowReinclude: true, cancellationToken);
    }

    /// <summary>APEX LEGAL_DECISION. The comment is optional.</summary>
    public async Task<LegalReviewDto> DecideAsync(long portfolioId, LegalDecisionRequest request, CancellationToken cancellationToken)
    {
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid Legal Review decision.");
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        if (comment?.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.LegalReviewRejectedStatus,
                comment ?? "Legal Review rejected.", cancellationToken);
        }
        else
        {
            var review = await db.Set<MtgLegalReview>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
            if (review is null || !review.IsComplete)
                throw new ConflictException("Legal Review cannot be accepted until all six legal checks are Yes and both Review Date and Approved Date are completed.");
            if (await db.Set<MtgPortfolioAccount>().AnyAsync(x => x.PortfolioId == portfolioId
                    && x.DealDecisionCode != MtgPortfolioAccount.SelectedDecision
                    && x.DealDecisionCode != MtgPortfolioAccount.ExcludedDecision, cancellationToken))
                throw new ConflictException("Every investor must be either Selected or Excluded before Legal Review can be accepted.");

            await PortfolioTotals.RecalculateSelectedAsync(db, portfolioId, cancellationToken);
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.LegalReviewAcceptedStatus,
                comment ?? "Legal Review accepted.", cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }
}
