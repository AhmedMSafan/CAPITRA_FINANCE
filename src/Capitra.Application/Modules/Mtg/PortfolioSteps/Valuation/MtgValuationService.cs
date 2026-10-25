using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.Valuation;

/// <summary>Everything the valuation step shows for one portfolio.</summary>
public sealed record ValuationsDto(
    long PortfolioId,
    string PortfolioNo,
    string DeveloperName,
    string ProjectName,
    string ProjectCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    bool IsOpen, // valuations can be recorded and the step completed
    decimal FaceValueAmount, // of the contracts kept in the deal
    decimal OutstandingAmount, // of the contracts kept in the deal
    IReadOnlyList<ValuationDto> Valuations,
    IReadOnlyList<ValuationUnitDto> Units,
    IReadOnlyList<ValuationTypeOptionDto> ValuationTypes);

public sealed record ValuationDto(
    long ValuationId,
    string ValuationTypeCode,
    string ValuationTypeName,
    long? ProjectUnitId,
    string? UnitCode,
    string ValuerName,
    string? ValuerRegistrationNo,
    DateOnly ValuationDate,
    decimal MarketValueAmount,
    decimal? RecommendedValueAmount,
    decimal? ProjectCompletionPct,
    string? FacilitiesStatus,
    string? ApprovedBy,
    DateOnly? ApprovedDate,
    string CreatedBy,
    DateTime CreatedAt,
    int RowVersion);

/// <summary>A unit of the portfolio's project that can be valued on its own.</summary>
public sealed record ValuationUnitDto(long ProjectUnitId, string UnitCode, string? UnitTypeCode, decimal? MarketValueAmount);

public sealed record ValuationTypeOptionDto(string Code, string Name);

public sealed record SaveValuationRequest(
    string ValuationTypeCode,
    long? ProjectUnitId, // null values the whole project
    string ValuerName,
    string? ValuerRegistrationNo,
    DateOnly? ValuationDate,
    decimal? MarketValueAmount,
    decimal? RecommendedValueAmount,
    decimal? ProjectCompletionPct,
    string? FacilitiesStatus,
    string? ApprovedBy,
    DateOnly? ApprovedDate,
    int? RowVersion);

public sealed record CompleteValuationRequest(string? Comment);

public interface IMtgValuationService
{
    Task<ValuationsDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<ValuationDto> CreateAsync(long portfolioId, SaveValuationRequest request, CancellationToken cancellationToken);

    Task<ValuationDto> UpdateAsync(long portfolioId, long valuationId, SaveValuationRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(long portfolioId, long valuationId, int rowVersion, CancellationToken cancellationToken);

    Task<ValuationsDto> CompleteAsync(long portfolioId, CompleteValuationRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Valuation" (APEX App 105 page 401): record the valuations of the project and its units,
/// then complete the step. APEX has no reject action on this step.
/// </summary>
public sealed class MtgValuationService(ICapitraDbContext db) : IMtgValuationService
{
    public const string StepName = "Valuation";

    /// <summary>
    /// Open after the legal review is accepted. The workflow engine (build step 4) will open it through a
    /// valuation task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPurchasedPortfolio.LegalReviewAcceptedStatus];

    public async Task<ValuationsDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPurchasedPortfolio>()
            join d in db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, p.ProjectId, DeveloperName = d.LegalNameEn ?? d.LegalNameAr,
                ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode,
                Face = p.SelectedFaceValueAmount ?? p.FaceValueAmount,
                Outstanding = p.SelectedOutstandingAmount ?? p.OutstandingAmount
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var units = await db.Set<MtgProjectUnit>()
            .Where(x => x.ProjectId == header.ProjectId)
            .OrderBy(x => x.UnitCode)
            .Select(x => new ValuationUnitDto(x.ProjectUnitId, x.UnitCode, x.UnitTypeCode, x.MarketValueAmount))
            .ToListAsync(cancellationToken);

        var types = await db.Set<MtgLkpValuationType>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.NameEn)
            .Select(x => new ValuationTypeOptionDto(x.Code, x.NameEn))
            .ToListAsync(cancellationToken);

        return new ValuationsDto(
            header.PortfolioId, header.PortfolioNo, header.DeveloperName, header.ProjectName, header.ProjectCode,
            header.PortfolioStatusCode, header.StatusName, OpenStatuses.Contains(header.PortfolioStatusCode),
            header.Face, header.Outstanding, await ValuationsAsync(portfolioId, null, cancellationToken), units, types);
    }

    /// <summary>APEX VAL_SAVE (new record).</summary>
    public async Task<ValuationDto> CreateAsync(long portfolioId, SaveValuationRequest request, CancellationToken cancellationToken)
    {
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = new MtgValuation { PortfolioId = portfolioId, ProjectId = portfolio.ProjectId };
        await ApplyAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (await ValuationsAsync(portfolioId, entity.ValuationId, cancellationToken)).Single();
    }

    /// <summary>APEX VAL_SAVE (existing record).</summary>
    public async Task<ValuationDto> UpdateAsync(long portfolioId, long valuationId, SaveValuationRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, valuationId, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This valuation was changed by someone else. Reload it and try again.");
        await ApplyAsync(entity, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (await ValuationsAsync(portfolioId, entity.ValuationId, cancellationToken)).Single();
    }

    /// <summary>APEX VAL_DELETE.</summary>
    public async Task DeleteAsync(long portfolioId, long valuationId, int rowVersion, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, valuationId, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw new ConflictException("This valuation was changed by someone else. Reload it and try again.");
        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>APEX VAL_COMPLETE. The comment is optional.</summary>
    public async Task<ValuationsDto> CompleteAsync(long portfolioId, CompleteValuationRequest request, CancellationToken cancellationToken)
    {
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        if (comment?.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        if (!await db.Set<MtgValuation>().AnyAsync(x => x.PortfolioId == portfolioId, cancellationToken))
            throw new ConflictException("Record at least one valuation before completing the valuation step.");

        await PortfolioStatus.ChangeAsync(db, portfolio, MtgPurchasedPortfolio.ValuationCompletedStatus,
            comment ?? "Valuation step completed.", cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private async Task ApplyAsync(MtgValuation entity, SaveValuationRequest r, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        var type = string.IsNullOrWhiteSpace(r.ValuationTypeCode) ? null : r.ValuationTypeCode.Trim().ToUpperInvariant();
        if (type is null)
            errors.Add(nameof(r.ValuationTypeCode), "Valuation type is required.");
        else if (type != entity.ValuationTypeCode && !await db.Set<MtgLkpValuationType>().AnyAsync(x => x.Code == type && x.IsActive, cancellationToken))
            errors.Add(nameof(r.ValuationTypeCode), "Invalid valuation type: it is not defined in Valuation Types.");

        if (r.ProjectUnitId is { } unitId
            && !await db.Set<MtgProjectUnit>().AnyAsync(x => x.ProjectUnitId == unitId && x.ProjectId == entity.ProjectId, cancellationToken))
            errors.Add(nameof(r.ProjectUnitId), "The selected unit does not belong to this portfolio's project.");

        var valuer = errors.Text(nameof(r.ValuerName), "Valuer name", r.ValuerName, 200, required: true);
        var registration = errors.Text(nameof(r.ValuerRegistrationNo), "Valuer registration no.", r.ValuerRegistrationNo, 100);
        if (r.ValuationDate is null)
            errors.Add(nameof(r.ValuationDate), "Valuation date is required.");
        if (r.MarketValueAmount is not > 0)
            errors.Add(nameof(r.MarketValueAmount), "Market value must be greater than zero.");
        if (r.RecommendedValueAmount < 0)
            errors.Add(nameof(r.RecommendedValueAmount), "Recommended value cannot be negative.");
        if (r.ProjectCompletionPct is < 0 or > 100)
            errors.Add(nameof(r.ProjectCompletionPct), "Project completion % must be between 0 and 100.");
        var facilities = errors.Text(nameof(r.FacilitiesStatus), "Facilities status", r.FacilitiesStatus, 4000);
        var approvedBy = errors.Text(nameof(r.ApprovedBy), "Approved by", r.ApprovedBy, 255);
        errors.ThrowIfAny();

        entity.ValuationTypeCode = type!;
        entity.ProjectUnitId = r.ProjectUnitId;
        entity.ValuerName = valuer!;
        entity.ValuerRegistrationNo = registration;
        entity.ValuationDate = r.ValuationDate!.Value;
        entity.MarketValueAmount = r.MarketValueAmount!.Value;
        entity.RecommendedValueAmount = r.RecommendedValueAmount;
        entity.ProjectCompletionPct = r.ProjectCompletionPct;
        entity.FacilitiesStatus = facilities;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedDate = r.ApprovedDate;
    }

    private async Task<MtgValuation> FindAsync(long portfolioId, long valuationId, CancellationToken cancellationToken) =>
        await db.Set<MtgValuation>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId && x.ValuationId == valuationId, cancellationToken)
        ?? throw new NotFoundException("Valuation record not found.");

    /// <summary>The portfolio's valuations, newest first; or just one of them.</summary>
    private async Task<IReadOnlyList<ValuationDto>> ValuationsAsync(long portfolioId, long? valuationId, CancellationToken cancellationToken) =>
        await (
            from v in db.Set<MtgValuation>()
            join t in db.Set<MtgLkpValuationType>() on v.ValuationTypeCode equals t.Code into types
            from t in types.DefaultIfEmpty()
            join u in db.Set<MtgProjectUnit>() on v.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            where v.PortfolioId == portfolioId && (valuationId == null || v.ValuationId == valuationId)
            orderby v.ValuationDate descending, v.ValuationId descending
            select new ValuationDto(
                v.ValuationId, v.ValuationTypeCode, t != null ? t.NameEn : v.ValuationTypeCode, v.ProjectUnitId,
                u != null ? u.UnitCode : null, v.ValuerName, v.ValuerRegistrationNo, v.ValuationDate, v.MarketValueAmount,
                v.RecommendedValueAmount, v.ProjectCompletionPct, v.FacilitiesStatus, v.ApprovedBy, v.ApprovedDate,
                v.CreatedBy, v.CreatedAt, v.RowVersion))
            .ToListAsync(cancellationToken);
}
