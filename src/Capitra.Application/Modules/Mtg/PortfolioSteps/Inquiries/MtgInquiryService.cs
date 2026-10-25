using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.Inquiries;

/// <summary>Everything the portfolio inquiries step shows for one portfolio.</summary>
public sealed record InquiriesDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectName,
    string ProjectCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    bool IsOpen, // inquiries, deal decisions and the step decision can be recorded
    IReadOnlyList<InquirySubjectDto> Subjects,
    IReadOnlyList<InquiryDto> Inquiries,
    IReadOnlyList<InquiryAccountDto> Accounts,
    IReadOnlyList<InquiryTypeOptionDto> InquiryTypes,
    IReadOnlyList<string> RiskLevels);

/// <summary>Something an inquiry can be about. Guarantees are entered by id until the guarantees screens exist.</summary>
public sealed record InquirySubjectDto(string SubjectTypeCode, long SubjectId, string Name);

public sealed record InquiryDto(
    long InquiryResultId,
    string SubjectTypeCode,
    long SubjectId,
    string SubjectName,
    string InquiryTypeCode,
    string InquiryTypeName,
    DateOnly? InquiryDate,
    string? ReferenceNo,
    string? ResultText,
    string? RiskLevelCode,
    string? ResultDetails,
    string CreatedBy,
    DateTime CreatedAt,
    int RowVersion);

public sealed record InquiryAccountDto(
    long PortfolioAccountId,
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    string? UnitCode,
    string? SellerContractNo,
    decimal ContractAmount,
    decimal OutstandingAmount,
    string? DealDecisionCode,
    int InquiryCount,
    string? LatestRiskLevelCode);

public sealed record InquiryTypeOptionDto(string Code, string Name);

public sealed record SaveInquiryRequest(
    string SubjectTypeCode,
    long? SubjectId,
    string InquiryTypeCode,
    DateOnly? InquiryDate,
    string? ReferenceNo,
    string? ResultText,
    string? RiskLevelCode,
    string? ResultDetails,
    int? RowVersion);

public sealed record InquiryDecisionRequest(string Action, string? Comment);

public interface IMtgInquiryService
{
    Task<InquiriesDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<InquiryDto> CreateAsync(long portfolioId, SaveInquiryRequest request, CancellationToken cancellationToken);

    Task<InquiryDto> UpdateAsync(long portfolioId, long inquiryResultId, SaveInquiryRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(long portfolioId, long inquiryResultId, int rowVersion, CancellationToken cancellationToken);

    Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken);

    Task<InquiriesDto> DecideAsync(long portfolioId, InquiryDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Inquiries" (APEX App 105 page 210): record every inquiry and its result, review which
/// investors stay in the deal, then complete or reject the step.
/// </summary>
public sealed class MtgInquiryService(ICapitraDbContext db) : IMtgInquiryService
{
    public const string StepName = "Portfolio inquiries";
    public const string ApproveAction = "APPROVE";
    public const string RejectAction = "REJECT";

    /// <summary>
    /// Open after the AML review is accepted, and after its own rejection so the step can be redone.
    /// The workflow engine (build step 4) will open it through an inquiry task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPurchasedPortfolio.AmlAcceptedStatus, MtgPurchasedPortfolio.InquiryRejectedStatus];

    public async Task<InquiriesDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPurchasedPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var subjects = await SubjectsAsync(portfolioId, cancellationToken);
        var inquiries = await InquiriesAsync(portfolioId, subjects, cancellationToken);

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
                UnitCode = u != null ? u.UnitCode : null, a.SellerContractNo, a.ContractAmount, a.OutstandingAmount,
                a.DealDecisionCode
            }).ToListAsync(cancellationToken);

        var types = await db.Set<MtgLkpInquiryType>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.NameEn)
            .Select(x => new InquiryTypeOptionDto(x.Code, x.NameEn))
            .ToListAsync(cancellationToken);

        return new InquiriesDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectName, header.ProjectCode, header.PortfolioStatusCode,
            header.StatusName, OpenStatuses.Contains(header.PortfolioStatusCode), subjects, inquiries,
            [.. accounts.Select(a =>
            {
                var about = inquiries.Where(q => q.SubjectTypeCode == MtgInquiryResult.InvestorSubject && q.SubjectId == a.InvestorId).ToList();
                return new InquiryAccountDto(
                    a.PortfolioAccountId, a.InvestorId, a.InvestorCode, a.InvestorName, a.UnitCode, a.SellerContractNo,
                    a.ContractAmount, a.OutstandingAmount, a.DealDecisionCode, about.Count,
                    about.MaxBy(q => q.InquiryResultId)?.RiskLevelCode);
            })],
            types,
            MtgInquiryResult.RiskLevels);
    }

    /// <summary>APEX INQ_SAVE (new record).</summary>
    public async Task<InquiryDto> CreateAsync(long portfolioId, SaveInquiryRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = new MtgInquiryResult { PortfolioId = portfolioId };
        await ApplyAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await GetOneAsync(portfolioId, entity.InquiryResultId, cancellationToken);
    }

    /// <summary>APEX INQ_SAVE (existing record).</summary>
    public async Task<InquiryDto> UpdateAsync(long portfolioId, long inquiryResultId, SaveInquiryRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, inquiryResultId, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This inquiry was changed by someone else. Reload it and try again.");
        await ApplyAsync(entity, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await GetOneAsync(portfolioId, entity.InquiryResultId, cancellationToken);
    }

    /// <summary>APEX INQ_DELETE.</summary>
    public async Task DeleteAsync(long portfolioId, long inquiryResultId, int rowVersion, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, inquiryResultId, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw new ConflictException("This inquiry was changed by someone else. Reload it and try again.");
        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>APEX INQ_UPDATE_INVESTOR: unlike the AML review, an exclusion here cannot be undone.</summary>
    public async Task SaveDealDecisionsAsync(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        await DealDecisions.SaveAsync(db, portfolioId, request, allowReinclude: false, cancellationToken);
    }

    /// <summary>APEX INQ_COMPLETE. The comment is optional here.</summary>
    public async Task<InquiriesDto> DecideAsync(long portfolioId, InquiryDecisionRequest request, CancellationToken cancellationToken)
    {
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (ApproveAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid inquiry decision.");
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        if (comment?.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.InquiryRejectedStatus,
                comment ?? "Portfolio Inquiry rejected.", cancellationToken);
        }
        else
        {
            await PortfolioTotals.RecalculateSelectedAsync(db, portfolioId, cancellationToken);
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.InquiryAcceptedStatus,
                comment ?? "Portfolio Inquiry workflow task completed.", cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private async Task ApplyAsync(MtgInquiryResult entity, SaveInquiryRequest r, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        var subjectType = string.IsNullOrWhiteSpace(r.SubjectTypeCode) ? null : r.SubjectTypeCode.Trim().ToUpperInvariant();
        if (subjectType is null)
            errors.Add(nameof(r.SubjectTypeCode), "Subject Type is required.");
        else if (!MtgInquiryResult.SubjectTypes.Contains(subjectType))
            errors.Add(nameof(r.SubjectTypeCode), "Invalid Subject Type.");

        if (r.SubjectId is null)
            errors.Add(nameof(r.SubjectId), "Choose the subject.");
        else if (subjectType is not null && MtgInquiryResult.SubjectTypes.Contains(subjectType) && subjectType != MtgInquiryResult.GuaranteeSubject)
        {
            var subjects = await SubjectsAsync(entity.PortfolioId, cancellationToken);
            if (!subjects.Any(x => x.SubjectTypeCode == subjectType && x.SubjectId == r.SubjectId))
                errors.Add(nameof(r.SubjectId), "Selected subject does not belong to this portfolio.");
        }

        var type = string.IsNullOrWhiteSpace(r.InquiryTypeCode) ? null : r.InquiryTypeCode.Trim().ToUpperInvariant();
        if (type is null)
            errors.Add(nameof(r.InquiryTypeCode), "Inquiry Type is required.");
        else if (type != entity.InquiryTypeCode && !await db.Set<MtgLkpInquiryType>().AnyAsync(x => x.Code == type && x.IsActive, cancellationToken))
            errors.Add(nameof(r.InquiryTypeCode), "Invalid inquiry type: it is not defined in Inquiry Types.");

        var reference = errors.Text(nameof(r.ReferenceNo), "Reference no.", r.ReferenceNo, 100);
        var result = errors.Text(nameof(r.ResultText), "Result", r.ResultText, 100);
        var details = errors.Text(nameof(r.ResultDetails), "Result details", r.ResultDetails, 4000);
        var risk = string.IsNullOrWhiteSpace(r.RiskLevelCode) ? null : r.RiskLevelCode.Trim().ToUpperInvariant();
        if (risk is not null && !MtgInquiryResult.RiskLevels.Contains(risk))
            errors.Add(nameof(r.RiskLevelCode), "Invalid risk level.");
        errors.ThrowIfAny();

        entity.SubjectTypeCode = subjectType!;
        entity.SubjectId = r.SubjectId!.Value;
        entity.InquiryTypeCode = type!;
        entity.InquiryDate = r.InquiryDate;
        entity.ReferenceNo = reference;
        entity.ResultText = result;
        entity.RiskLevelCode = risk;
        entity.ResultDetails = details;
    }

    private async Task<MtgInquiryResult> FindAsync(long portfolioId, long inquiryResultId, CancellationToken cancellationToken) =>
        await db.Set<MtgInquiryResult>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId && x.InquiryResultId == inquiryResultId, cancellationToken)
        ?? throw new NotFoundException("Inquiry record not found.");

    private async Task<InquiryDto> GetOneAsync(long portfolioId, long inquiryResultId, CancellationToken cancellationToken) =>
        (await InquiriesAsync(portfolioId, await SubjectsAsync(portfolioId, cancellationToken), cancellationToken))
        .Single(x => x.InquiryResultId == inquiryResultId);

    /// <summary>The portfolio itself, its developer, project, investors and the developer's signatories.</summary>
    private async Task<IReadOnlyList<InquirySubjectDto>> SubjectsAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var p = await (
            from x in db.Set<MtgPurchasedPortfolio>()
            join d in db.Set<MtgDeveloper>() on x.DeveloperId equals d.DeveloperId
            join prj in db.Set<MtgProject>() on x.ProjectId equals prj.ProjectId
            where x.PortfolioId == portfolioId
            select new
            {
                x.PortfolioNo, x.DeveloperId, DeveloperName = d.LegalNameEn ?? d.LegalNameAr,
                x.ProjectId, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr
            }).FirstAsync(cancellationToken);

        var investors = await (
            from i in db.Set<MtgInvestor>()
            where db.Set<MtgPurchasedPortfolioAccount>().Any(a => a.PortfolioId == portfolioId && a.InvestorId == i.InvestorId)
            orderby i.InvestorCode
            select new InquirySubjectDto(MtgInquiryResult.InvestorSubject, i.InvestorId, i.InvestorCode + " - " + (i.FullNameEn ?? i.FullNameAr)))
            .ToListAsync(cancellationToken);

        var signatories = await db.Set<MtgDeveloperSignatory>()
            .Where(x => x.DeveloperId == p.DeveloperId)
            .OrderBy(x => x.SignatoryName)
            .Select(x => new InquirySubjectDto(MtgInquiryResult.SignatorySubject, x.DeveloperSignatoryId, x.SignatoryName))
            .ToListAsync(cancellationToken);

        return
        [
            new(MtgInquiryResult.PortfolioSubject, portfolioId, p.PortfolioNo),
            new(MtgInquiryResult.DeveloperSubject, p.DeveloperId, p.DeveloperName),
            new(MtgInquiryResult.ProjectSubject, p.ProjectId, p.ProjectName),
            .. investors,
            .. signatories
        ];
    }

    /// <summary>The inquiries of a portfolio, newest first.</summary>
    private async Task<IReadOnlyList<InquiryDto>> InquiriesAsync(
        long portfolioId, IReadOnlyList<InquirySubjectDto> subjects, CancellationToken cancellationToken)
    {
        var rows = await (
            from q in db.Set<MtgInquiryResult>()
            join t in db.Set<MtgLkpInquiryType>() on q.InquiryTypeCode equals t.Code into types
            from t in types.DefaultIfEmpty()
            where q.PortfolioId == portfolioId
            orderby q.InquiryResultId descending
            select new { q, TypeName = t != null ? t.NameEn : q.InquiryTypeCode })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(x => new InquiryDto(
            x.q.InquiryResultId, x.q.SubjectTypeCode, x.q.SubjectId,
            subjects.FirstOrDefault(s => s.SubjectTypeCode == x.q.SubjectTypeCode && s.SubjectId == x.q.SubjectId)?.Name
                ?? $"{x.q.SubjectTypeCode} #{x.q.SubjectId}",
            x.q.InquiryTypeCode, x.TypeName, x.q.InquiryDate, x.q.ReferenceNo, x.q.ResultText, x.q.RiskLevelCode,
            x.q.ResultDetails, x.q.CreatedBy, x.q.CreatedAt, x.q.RowVersion))];
    }
}
