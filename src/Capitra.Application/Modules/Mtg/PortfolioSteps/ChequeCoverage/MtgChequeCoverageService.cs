using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.ChequeCoverage;

/// <summary>Everything the cheque coverage step shows for one portfolio.</summary>
public sealed record ChequeCoverageDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectName,
    string ProjectCode,
    string CurrencyCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string? RejectionReason,
    bool IsOpen, // cheques can be entered and the step decided
    IReadOnlyList<ChequeInvestorDto> Investors, // investors kept in the deal only
    IReadOnlyList<ChequeAccountDto> Accounts, // their contracts, for the cheque form
    IReadOnlyList<ChequeDto> Cheques,
    IReadOnlyList<ChequeStatusOptionDto> ChequeStatuses);

/// <summary>An investor kept in the deal: what they still owe and how much of it their cheques cover.</summary>
public sealed record ChequeInvestorDto(
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    int ContractCount,
    decimal OutstandingAmount,
    int ChequeCount,
    decimal ChequeAmount,
    bool IsCovered);

public sealed record ChequeAccountDto(long PortfolioAccountId, long InvestorId, string? SellerContractNo, string? UnitCode, decimal OutstandingAmount);

public sealed record ChequeDto(
    long ChequeId,
    long PortfolioAccountId,
    long InvestorId,
    string? SellerContractNo,
    string? UnitCode,
    string ChequeNo,
    string? BankName,
    DateOnly? ChequeDate,
    DateOnly DueDate,
    decimal ChequeAmount,
    bool IsEndorsedToCompany,
    string ChequeStatusCode,
    string ChequeStatusName,
    DateOnly? ClearingDate,
    string? ReturnReason,
    int RowVersion);

public sealed record ChequeStatusOptionDto(string Code, string Name);

public sealed record SaveChequeRequest(
    long PortfolioAccountId,
    string ChequeNo,
    string? BankName,
    DateOnly? ChequeDate,
    DateOnly? DueDate,
    decimal? ChequeAmount,
    bool IsEndorsedToCompany,
    string? ChequeStatusCode, // PENDING when empty
    DateOnly? ClearingDate,
    string? ReturnReason,
    int? RowVersion);

public sealed record ChequeCoverageDecisionRequest(string Action, string Comment);

public interface IMtgChequeCoverageService
{
    Task<ChequeCoverageDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<ChequeDto> CreateAsync(long portfolioId, SaveChequeRequest request, CancellationToken cancellationToken);

    Task<ChequeDto> UpdateAsync(long portfolioId, long chequeId, SaveChequeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(long portfolioId, long chequeId, int rowVersion, CancellationToken cancellationToken);

    Task<ChequeCoverageDto> DecideAsync(long portfolioId, ChequeCoverageDecisionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Cheque Coverage Review" (APEX App 105 page 213): enter the cheques of every investor kept
/// in the deal, check that they cover each investor's and the whole portfolio's outstanding balance, then accept or
/// reject the step. Only contracts kept in the deal count; a contract with no decision counts as kept, as in APEX.
/// </summary>
public sealed class MtgChequeCoverageService(ICapitraDbContext db) : IMtgChequeCoverageService
{
    public const string StepName = "Cheque coverage";
    public const string AcceptAction = "ACCEPT";
    public const string RejectAction = "REJECT";

    /// <summary>APEX compares amounts with half a piastre of tolerance.</summary>
    private const decimal Tolerance = 0.005m;

    /// <summary>
    /// Open after the coverage review is accepted, and after its own rejection so the review can be redone.
    /// The workflow engine (build step 4) will open it through a cheque coverage task instead.
    /// </summary>
    private static readonly string[] OpenStatuses = [MtgPortfolio.CoverageReviewAcceptedStatus, MtgPortfolio.ChequeCoverageRejectedStatus];

    public async Task<ChequeCoverageDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr, prj.ProjectCode, p.CurrencyCode,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode, p.RejectionReason
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var accounts = await (
            from a in db.Set<MtgPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            where a.PortfolioId == portfolioId && a.DealDecisionCode != MtgPortfolioAccount.ExcludedDecision
            orderby i.InvestorCode, a.PortfolioAccountId
            select new
            {
                a.PortfolioAccountId, a.InvestorId, i.InvestorCode, InvestorName = i.FullNameEn ?? i.FullNameAr,
                a.SellerContractNo, UnitCode = u != null ? u.UnitCode : null, a.OutstandingAmount
            }).ToListAsync(cancellationToken);

        var cheques = await ChequesAsync(portfolioId, null, cancellationToken);
        var keptAccountIds = accounts.Select(a => a.PortfolioAccountId).ToHashSet();
        var counted = cheques.Where(c => keptAccountIds.Contains(c.PortfolioAccountId)).ToList();

        var investors = accounts
            .GroupBy(a => a.InvestorId)
            .Select(g =>
            {
                var outstanding = g.Sum(x => x.OutstandingAmount);
                var theirs = counted.Where(c => c.InvestorId == g.Key).ToList();
                var covered = theirs.Sum(c => c.ChequeAmount);
                return new ChequeInvestorDto(
                    g.Key, g.First().InvestorCode, g.First().InvestorName, g.Count(), outstanding,
                    theirs.Count, covered, covered >= outstanding - Tolerance);
            })
            .ToList();

        var chequeStatuses = await db.Set<AdmLkpChequeStatus>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.NameEn)
            .Select(x => new ChequeStatusOptionDto(x.Code, x.NameEn))
            .ToListAsync(cancellationToken);

        return new ChequeCoverageDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectName, header.ProjectCode, header.CurrencyCode,
            header.PortfolioStatusCode, header.StatusName, header.RejectionReason, OpenStatuses.Contains(header.PortfolioStatusCode),
            investors,
            [.. accounts.Select(a => new ChequeAccountDto(a.PortfolioAccountId, a.InvestorId, a.SellerContractNo, a.UnitCode, a.OutstandingAmount))],
            counted,
            chequeStatuses);
    }

    /// <summary>APEX CHQ_SAVE (new cheque).</summary>
    public async Task<ChequeDto> CreateAsync(long portfolioId, SaveChequeRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = new MtgPortfolioCheque { PortfolioId = portfolioId };
        await ApplyAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (await ChequesAsync(portfolioId, entity.ChequeId, cancellationToken)).Single();
    }

    /// <summary>APEX CHQ_SAVE (existing cheque).</summary>
    public async Task<ChequeDto> UpdateAsync(long portfolioId, long chequeId, SaveChequeRequest request, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, chequeId, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This cheque was changed by someone else. Reload it and try again.");
        await ApplyAsync(entity, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (await ChequesAsync(portfolioId, entity.ChequeId, cancellationToken)).Single();
    }

    /// <summary>APEX CHQ_DELETE.</summary>
    public async Task DeleteAsync(long portfolioId, long chequeId, int rowVersion, CancellationToken cancellationToken)
    {
        await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);
        var entity = await FindAsync(portfolioId, chequeId, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw new ConflictException("This cheque was changed by someone else. Reload it and try again.");
        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>APEX CHQ_DECISION. A comment is required for both actions; accept checks both coverage rules.</summary>
    public async Task<ChequeCoverageDto> DecideAsync(long portfolioId, ChequeCoverageDecisionRequest request, CancellationToken cancellationToken)
    {
        var comment = request.Comment?.Trim();
        if (string.IsNullOrEmpty(comment))
            throw BusinessValidationException.For(nameof(request.Comment), "A comment is required for both Accept and Reject.");
        if (comment.Length > 4000)
            throw BusinessValidationException.For(nameof(request.Comment), "Comment is limited to 4000 characters.");
        var action = request.Action?.Trim().ToUpperInvariant();
        if (action is not (AcceptAction or RejectAction))
            throw BusinessValidationException.For(nameof(request.Action), "Invalid cheque coverage decision.");

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var portfolio = await PortfolioStatus.RequireAsync(db, portfolioId, StepName, OpenStatuses, cancellationToken);

        if (action == RejectAction)
        {
            portfolio.RejectionReason = comment;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.ChequeCoverageRejectedStatus, comment, cancellationToken);
        }
        else
        {
            var investors = (await GetAsync(portfolioId, cancellationToken)).Investors;
            if (investors.Count == 0)
                throw new ConflictException("No investors are included in this deal.");
            var shortCount = investors.Count(x => !x.IsCovered);
            if (shortCount > 0)
                throw new ConflictException($"{shortCount} investor(s) do not have cheques covering their outstanding balance.");
            var outstanding = investors.Sum(x => x.OutstandingAmount);
            var cheques = investors.Sum(x => x.ChequeAmount);
            if (cheques < outstanding - Tolerance)
                throw new ConflictException(
                    $"Total cheques ({cheques.ToString("N2", CultureInfo.InvariantCulture)}) do not cover the portfolio outstanding ({outstanding.ToString("N2", CultureInfo.InvariantCulture)}).");

            portfolio.RejectionReason = null;
            await PortfolioStatus.ChangeAsync(db, portfolio, MtgPortfolio.ChequeCoverageAcceptedStatus, comment, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(portfolioId, cancellationToken);
    }

    private async Task ApplyAsync(MtgPortfolioCheque entity, SaveChequeRequest r, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        if (!await db.Set<MtgPortfolioAccount>().AnyAsync(x => x.PortfolioAccountId == r.PortfolioAccountId
                && x.PortfolioId == entity.PortfolioId && x.DealDecisionCode != MtgPortfolioAccount.ExcludedDecision, cancellationToken))
            errors.Add(nameof(r.PortfolioAccountId), "Choose a contract of an investor who is included in the deal.");

        var chequeNo = errors.Text(nameof(r.ChequeNo), "Cheque No.", r.ChequeNo, 100, required: true);
        var bank = errors.Text(nameof(r.BankName), "Bank name", r.BankName, 300);
        if (r.ChequeAmount is not > 0)
            errors.Add(nameof(r.ChequeAmount), "Cheque Amount must be greater than zero.");
        if (r.DueDate is null)
            errors.Add(nameof(r.DueDate), "Due Date is required.");

        var status = string.IsNullOrWhiteSpace(r.ChequeStatusCode) ? MtgPortfolioCheque.PendingStatus : r.ChequeStatusCode.Trim().ToUpperInvariant();
        if (status != entity.ChequeStatusCode || entity.ChequeId == 0)
        {
            if (!await db.Set<AdmLkpChequeStatus>().AnyAsync(x => x.Code == status && x.IsActive, cancellationToken))
                errors.Add(nameof(r.ChequeStatusCode), $"Unknown cheque status {status}.");
        }

        var returnReason = errors.Text(nameof(r.ReturnReason), "Return reason", r.ReturnReason, 1000);
        errors.ThrowIfAny();

        entity.PortfolioAccountId = r.PortfolioAccountId;
        entity.ChequeNo = chequeNo!;
        entity.BankName = bank;
        entity.ChequeDate = r.ChequeDate;
        entity.DueDate = r.DueDate!.Value;
        entity.ChequeAmount = Math.Round(r.ChequeAmount!.Value, 2, MidpointRounding.AwayFromZero);
        entity.IsEndorsedToCompany = r.IsEndorsedToCompany;
        entity.ChequeStatusCode = status;
        entity.ClearingDate = r.ClearingDate;
        entity.ReturnReason = returnReason;
    }

    private async Task<MtgPortfolioCheque> FindAsync(long portfolioId, long chequeId, CancellationToken cancellationToken) =>
        await db.Set<MtgPortfolioCheque>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId && x.ChequeId == chequeId, cancellationToken)
        ?? throw new NotFoundException("This cheque was not found in the portfolio.");

    /// <summary>The portfolio's cheques by due date; or just one of them.</summary>
    private async Task<IReadOnlyList<ChequeDto>> ChequesAsync(long portfolioId, long? chequeId, CancellationToken cancellationToken) =>
        await (
            from c in db.Set<MtgPortfolioCheque>()
            join a in db.Set<MtgPortfolioAccount>() on c.PortfolioAccountId equals a.PortfolioAccountId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            join s in db.Set<AdmLkpChequeStatus>() on c.ChequeStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where c.PortfolioId == portfolioId && (chequeId == null || c.ChequeId == chequeId)
            orderby c.DueDate, c.ChequeId
            select new ChequeDto(
                c.ChequeId, c.PortfolioAccountId, a.InvestorId, a.SellerContractNo, u != null ? u.UnitCode : null,
                c.ChequeNo, c.BankName, c.ChequeDate, c.DueDate, c.ChequeAmount, c.IsEndorsedToCompany,
                c.ChequeStatusCode, s != null ? s.NameEn : c.ChequeStatusCode, c.ClearingDate, c.ReturnReason, c.RowVersion))
            .ToListAsync(cancellationToken);
}
