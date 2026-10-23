using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Portfolios;

/// <summary>One card on the portfolio cases board (APEX page 201, built on FNV_V_PORTFOLIO_SUMMARY).</summary>
public sealed record PortfolioCaseDto(
    long PortfolioId,
    string PortfolioNo,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? PortfolioStatusNameAr,
    int StatusDisplayOrder,
    long DeveloperId,
    string DeveloperName,
    string ProjectName,
    DateOnly RequestDate,
    int AgeDays,
    string CurrencyCode,
    decimal FaceValueAmount,
    decimal OutstandingAmount,
    int AccountCount,
    decimal ArrearsAmount);

/// <summary>Everything the portfolio overview (APEX page 203) shows for one portfolio.</summary>
public sealed record PortfolioWorkspaceDto(
    PortfolioDto Portfolio,
    PortfolioFiguresDto Figures,
    PortfolioProjectDto Project,
    IReadOnlyList<PortfolioInvestorDto> Investors,
    IReadOnlyList<PortfolioAccountDto> Accounts,
    IReadOnlyList<PortfolioUnitDto> Units,
    IReadOnlyList<PortfolioStatusChangeDto> StatusHistory);

public sealed record PortfolioFiguresDto(
    decimal FaceValueAmount,
    decimal OutstandingAmount,
    decimal ProposedPurchaseAmount,
    decimal ExpectedReturnAmount,
    decimal PaidAmount,
    decimal ArrearsAmount,
    int AccountCount,
    int InvestorCount,
    int UnitCount,
    int AgeDays);

public sealed record PortfolioProjectDto(
    string ProjectCode,
    string? ProjectNameEn,
    string ProjectNameAr,
    string DeveloperCode,
    string DeveloperName,
    string? LocationText,
    decimal? LandArea,
    string? TitleDeedNo,
    string? BuildingLicenseNo,
    DateOnly? BuildingLicenseDate,
    decimal? ProjectCompletionPct,
    string? ProjectStatus,
    int UnitsInProject);

public sealed record PortfolioInvestorDto(
    long InvestorId,
    string InvestorCode,
    string? FullNameEn,
    string FullNameAr,
    string? NationalIdNo,
    int ContractCount,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount);

public sealed record PortfolioUnitDto(
    long PortfolioAccountId,
    string UnitCode,
    string? UnitTypeName,
    string InvestorName,
    decimal ContractAmount);

public sealed record PortfolioStatusChangeDto(
    string? FromStatusCode,
    string? FromStatusName,
    string ToStatusCode,
    string? ToStatusName,
    DateTime ChangedAt,
    string ChangedBy,
    string? Comments);

public interface IMtgPortfolioQueryService
{
    Task<IReadOnlyList<PortfolioCaseDto>> ListCasesAsync(long? developerId, CancellationToken cancellationToken);

    Task<PortfolioWorkspaceDto> GetWorkspaceAsync(long portfolioId, CancellationToken cancellationToken);
}

/// <summary>Read-only views of portfolios: the cases board (page 201) and the portfolio overview (page 203).</summary>
public sealed class MtgPortfolioQueryService(
    ICapitraDbContext db, IClock clock, IMtgPortfolioService portfolios, IMtgPortfolioAccountService accounts)
    : IMtgPortfolioQueryService
{
    /// <summary>Page 201 shows the newest 300 cases.</summary>
    private const int BoardLimit = 300;

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public async Task<IReadOnlyList<PortfolioCaseDto>> ListCasesAsync(long? developerId, CancellationToken cancellationToken)
    {
        var portfolios = db.Set<MtgPortfolio>().AsNoTracking();
        if (developerId is { } id)
            portfolios = portfolios.Where(x => x.DeveloperId == id);

        var rows = await (
            from p in portfolios
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code
            join d in db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
            join j in db.Set<MtgProject>() on p.ProjectId equals j.ProjectId
            orderby p.RequestDate descending, p.PortfolioNo descending
            select new
            {
                p.PortfolioId, p.PortfolioNo, p.PortfolioStatusCode, StatusEn = s.NameEn, StatusAr = s.NameAr, s.DisplayOrder,
                p.DeveloperId, Developer = d.LegalNameEn ?? d.LegalNameAr, Project = j.ProjectNameEn ?? j.ProjectNameAr,
                p.RequestDate, p.CurrencyCode, p.FaceValueAmount, p.OutstandingAmount,
                Accounts = db.Set<MtgPortfolioAccount>().Count(a => a.PortfolioId == p.PortfolioId),
                Arrears = db.Set<MtgPortfolioAccount>().Where(a => a.PortfolioId == p.PortfolioId).Sum(a => (decimal?)a.ArrearsAmount) ?? 0
            })
            .Take(BoardLimit)
            .ToListAsync(cancellationToken);

        var today = Today();
        return rows.Select(r => new PortfolioCaseDto(
                r.PortfolioId, r.PortfolioNo, r.PortfolioStatusCode, r.StatusEn, r.StatusAr, r.DisplayOrder, r.DeveloperId,
                r.Developer, r.Project, r.RequestDate, today.DayNumber - r.RequestDate.DayNumber, r.CurrencyCode,
                r.FaceValueAmount, r.OutstandingAmount, r.Accounts, r.Arrears))
            .ToList();
    }

    public async Task<PortfolioWorkspaceDto> GetWorkspaceAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var portfolio = await portfolios.GetAsync(portfolioId, cancellationToken);
        var entity = await db.Set<MtgPortfolio>().AsNoTracking().FirstAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        var accountRows = await accounts.ListAsync(portfolioId, cancellationToken);

        var project = await (
                from j in db.Set<MtgProject>()
                join d in db.Set<MtgDeveloper>() on j.DeveloperId equals d.DeveloperId
                where j.ProjectId == entity.ProjectId
                select new PortfolioProjectDto(
                    j.ProjectCode, j.ProjectNameEn, j.ProjectNameAr, d.DeveloperCode, d.LegalNameEn ?? d.LegalNameAr,
                    j.LocationText, j.LandArea, j.TitleDeedNo, j.BuildingLicenseNo, j.BuildingLicenseDate,
                    j.ProjectCompletionPct, j.ProjectStatus,
                    db.Set<MtgProjectUnit>().Count(u => u.ProjectId == j.ProjectId)))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Project of portfolio {portfolioId} was not found.");

        // Investors tab: one row per investor, largest outstanding first (page 203, region Portfolio Details).
        var investors = await (
                from a in db.Set<MtgPortfolioAccount>()
                join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
                where a.PortfolioId == portfolioId
                group a by new { i.InvestorId, i.InvestorCode, i.FullNameEn, i.FullNameAr, i.NationalIdNo } into g
                select new PortfolioInvestorDto(
                    g.Key.InvestorId, g.Key.InvestorCode, g.Key.FullNameEn, g.Key.FullNameAr, g.Key.NationalIdNo, g.Count(),
                    g.Sum(x => x.ContractAmount), g.Sum(x => x.PaidAmount), g.Sum(x => x.OutstandingAmount)))
            .ToListAsync(cancellationToken);
        investors = [.. investors.OrderByDescending(x => x.OutstandingAmount).ThenBy(x => x.InvestorCode, StringComparer.Ordinal)];

        var units = await (
                from a in db.Set<MtgPortfolioAccount>()
                join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId
                join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
                join t in db.Set<MtgLkpUnitType>() on u.UnitTypeCode equals t.Code into types
                from t in types.DefaultIfEmpty()
                where a.PortfolioId == portfolioId
                orderby u.UnitCode
                select new PortfolioUnitDto(
                    a.PortfolioAccountId, u.UnitCode, t == null ? null : t.NameEn, i.FullNameEn ?? i.FullNameAr, a.ContractAmount))
            .ToListAsync(cancellationToken);

        var history = await (
                from h in db.Set<MtgPortfolioStatusHistory>()
                join f in db.Set<MtgLkpPortfolioStatus>() on h.FromStatusCode equals f.Code into fromStatuses
                from f in fromStatuses.DefaultIfEmpty()
                join t in db.Set<MtgLkpPortfolioStatus>() on h.ToStatusCode equals t.Code into toStatuses
                from t in toStatuses.DefaultIfEmpty()
                where h.PortfolioId == portfolioId
                orderby h.CreatedAt descending, h.PortfolioStatusHistoryId descending
                select new PortfolioStatusChangeDto(
                    h.FromStatusCode, f == null ? null : f.NameEn, h.ToStatusCode, t == null ? null : t.NameEn, h.CreatedAt,
                    h.CreatedBy, h.Comments))
            .ToListAsync(cancellationToken);

        var figures = new PortfolioFiguresDto(
            entity.FaceValueAmount, entity.OutstandingAmount, entity.ProposedPurchaseAmount, entity.ExpectedReturnAmount,
            accountRows.Sum(x => x.PaidAmount), accountRows.Sum(x => x.ArrearsAmount), accountRows.Count,
            accountRows.Select(x => x.InvestorId).Distinct().Count(), accountRows.Select(x => x.ProjectUnitId).Distinct().Count(),
            Today().DayNumber - entity.RequestDate.DayNumber);

        return new PortfolioWorkspaceDto(portfolio, figures, project, investors, accountRows, units, history);
    }

    /// <summary>Case age counts calendar days in Cairo, the business's time zone.</summary>
    private DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));
}
