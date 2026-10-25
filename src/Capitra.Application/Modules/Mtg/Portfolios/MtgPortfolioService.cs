using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Portfolios;

/// <summary>The portfolio form (APEX page 202): what the user enters when registering a portfolio.</summary>
public sealed record PortfolioDto(
    long PortfolioId,
    string PortfolioNo,
    long DeveloperId,
    string DeveloperName,
    long ProjectId,
    string ProjectName,
    DateOnly RequestDate,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string CurrencyCode,
    int? TermMonths,
    string? Comments,
    decimal? CoveragePct,
    decimal? ReservedAmountPct,
    decimal? PvRate,
    decimal? LateFeeRate,
    decimal? MarginPct,
    decimal? AdminExpensePct,
    decimal? LegalFeePerUnitAmount,
    decimal? ValuationFeePerUnitAmount,
    int AccountCount,
    int RowVersion);

public sealed record SavePortfolioRequest(
    string PortfolioNo,
    long DeveloperId,
    long ProjectId,
    DateOnly? RequestDate,
    string? CurrencyCode,
    int? TermMonths,
    string? Comments,
    decimal? CoveragePct,
    decimal? ReservedAmountPct,
    decimal? PvRate,
    decimal? LateFeeRate,
    decimal? MarginPct,
    decimal? AdminExpensePct,
    decimal? LegalFeePerUnitAmount,
    decimal? ValuationFeePerUnitAmount,
    int? RowVersion) : ISaveRequest;

/// <summary>The "Financial rates &amp; fees" dialog of the portfolio overview (APEX page 203, process FIN_TERMS_SAVE).</summary>
public sealed record SaveFinancialTermsRequest(
    decimal? CoveragePct,
    decimal? MarginPct,
    decimal? PvRate,
    decimal? ReservedAmountPct,
    decimal? LateFeeRate,
    decimal? AdminExpensePct,
    decimal? LegalFeePerUnitAmount,
    decimal? ValuationFeePerUnitAmount,
    int RowVersion);

public interface IMtgPortfolioService : ICrudService<PortfolioDto, SavePortfolioRequest>
{
    Task<PortfolioDto> SaveFinancialTermsAsync(long portfolioId, SaveFinancialTermsRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Create, change and delete a portfolio (APEX page 202 processes Create Portfolio, Update Portfolio and
/// Delete Draft Portfolio, and FNV_PURSHASED_PORTFOLIO_PKG.create_portfolio), plus the rates and fees of page 203.
/// </summary>
public sealed class MtgPortfolioService(ICapitraDbContext db)
    : CrudService<MtgPurchasedPortfolio, PortfolioDto, SavePortfolioRequest>(db), IMtgPortfolioService
{
    private const decimal MaxAmount = 9_999_999_999_999_999m;
    private const decimal MaxPct = 999.999999m;

    protected override string Title => "Portfolio";

    protected override long IdOf(MtgPurchasedPortfolio entity) => entity.PortfolioId;

    protected override Expression<Func<MtgPurchasedPortfolio, bool>> HasId(long id) => x => x.PortfolioId == id;

    protected override IQueryable<PortfolioDto> Project(IQueryable<MtgPurchasedPortfolio> rows) =>
        from p in rows
        join d in Db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
        join j in Db.Set<MtgProject>() on p.ProjectId equals j.ProjectId
        join s in Db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code
        select new PortfolioDto(
            p.PortfolioId, p.PortfolioNo, p.DeveloperId, d.LegalNameEn ?? d.LegalNameAr, p.ProjectId,
            j.ProjectNameEn ?? j.ProjectNameAr, p.RequestDate, p.PortfolioStatusCode, s.NameEn, p.CurrencyCode, p.TermMonths,
            p.Comments, p.CoveragePct, p.ReservedAmountPct, p.PvRate, p.LateFeeRate, p.MarginPct, p.AdminExpensePct,
            p.LegalFeePerUnitAmount, p.ValuationFeePerUnitAmount,
            Db.Set<MtgPurchasedPortfolioAccount>().Count(a => a.PortfolioId == p.PortfolioId), p.RowVersion);

    protected override async Task ValidateAsync(
        SavePortfolioRequest r, MtgPurchasedPortfolio? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        var no = errors.Text(nameof(r.PortfolioNo), "Portfolio No.", r.PortfolioNo, 30, required: true);
        errors.Text(nameof(r.Comments), "Comments", r.Comments, 4000);
        if (r.RequestDate is null)
            errors.Add(nameof(r.RequestDate), "Request Date is required.");

        RatesAndFees(errors, r.CoveragePct, r.ReservedAmountPct, r.PvRate, r.LateFeeRate, r.MarginPct, r.AdminExpensePct,
            r.LegalFeePerUnitAmount, r.ValuationFeePerUnitAmount);

        if (no is not null)
        {
            // APEX compares upper(PORTFOLIO_NO); ToUpper is translated to SQL upper(), string.Equals with a comparison is not.
            var upper = no.ToUpperInvariant();
#pragma warning disable CA1862
            if (await IsTakenAsync(x => x.PortfolioNo.ToUpper() == upper, existing, cancellationToken))
#pragma warning restore CA1862
                errors.Add(nameof(r.PortfolioNo), "Portfolio No. already exists.");
        }

        if (!await IsSelectableAsync(
                Db.Set<MtgDeveloper>().Where(x => x.DeveloperId == r.DeveloperId && x.IsActive),
                existing?.DeveloperId == r.DeveloperId, cancellationToken))
            errors.Add(nameof(r.DeveloperId), "Select an active developer.");

        var projectDeveloper = await Db.Set<MtgProject>()
            .Where(x => x.ProjectId == r.ProjectId && (x.IsActive || existing != null && existing.ProjectId == r.ProjectId))
            .Select(x => (long?)x.DeveloperId)
            .FirstOrDefaultAsync(cancellationToken);
        if (projectDeveloper is null)
            errors.Add(nameof(r.ProjectId), "Select an active project.");
        else if (projectDeveloper != r.DeveloperId)
            errors.Add(nameof(r.ProjectId), "Selected project does not belong to the selected developer.");

        var currency = MasterDataRules.Code(r.CurrencyCode);
        if (currency is null)
            errors.Add(nameof(r.CurrencyCode), "Currency is required.");
        else if (!await IsSelectableAsync(
                     Db.Set<AdmLkpCurrency>().Where(x => x.Code == currency && x.IsActive),
                     existing?.CurrencyCode == currency, cancellationToken))
            errors.Add(nameof(r.CurrencyCode), $"Currency '{currency}' is not an active currency.");

        if (existing is not null
            && (existing.DeveloperId != r.DeveloperId || existing.ProjectId != r.ProjectId)
            && await Db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(x => x.PortfolioId == existing.PortfolioId, cancellationToken))
            errors.Add(nameof(r.ProjectId), "Developer/Project cannot be changed after portfolio accounts exist.");

        if (existing is null && !await Db.Set<MtgLkpPortfolioStatus>().AnyAsync(x => x.Code == MtgPurchasedPortfolio.DraftStatus, cancellationToken))
            errors.Add(nameof(r.PortfolioNo), $"Portfolio status {MtgPurchasedPortfolio.DraftStatus} is missing from the reference data.");
    }

    protected override void Apply(MtgPurchasedPortfolio e, SavePortfolioRequest r)
    {
        e.PortfolioNo = r.PortfolioNo.Trim();
        e.DeveloperId = r.DeveloperId;
        e.ProjectId = r.ProjectId;
        e.RequestDate = r.RequestDate!.Value;
        e.CurrencyCode = MasterDataRules.Code(r.CurrencyCode)!;
        e.TermMonths = r.TermMonths;
        e.Comments = MasterDataRules.Clean(r.Comments);
        e.CoveragePct = r.CoveragePct;
        // APEX stores these three as 0 when left blank on page 202.
        e.ReservedAmountPct = r.ReservedAmountPct ?? 0;
        e.PvRate = r.PvRate ?? 0;
        e.LateFeeRate = r.LateFeeRate ?? 0;
        e.MarginPct = r.MarginPct;
        e.AdminExpensePct = r.AdminExpensePct;
        e.LegalFeePerUnitAmount = r.LegalFeePerUnitAmount;
        e.ValuationFeePerUnitAmount = r.ValuationFeePerUnitAmount;
    }

    protected override async Task AfterCreateAsync(MtgPurchasedPortfolio entity, CancellationToken cancellationToken)
    {
        Db.Add(new MtgPurchasedPortfolioStatusHistory
        {
            PortfolioId = entity.PortfolioId,
            FromStatusCode = null,
            ToStatusCode = MtgPurchasedPortfolio.DraftStatus,
            Comments = "Portfolio created"
        });
        await Db.SaveChangesAsync(cancellationToken);
    }

    protected override async Task BeforeDeleteAsync(MtgPurchasedPortfolio entity, CancellationToken cancellationToken)
    {
        if (entity.PortfolioStatusCode != MtgPurchasedPortfolio.DraftStatus)
            throw new ConflictException("Only draft portfolios can be deleted.");
        if (await Db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(x => x.PortfolioId == entity.PortfolioId, cancellationToken))
            throw new ConflictException("Remove portfolio accounts before deleting the portfolio.");

        await Db.Set<MtgPurchasedPortfolioStatusHistory>().Where(x => x.PortfolioId == entity.PortfolioId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<PortfolioDto> SaveFinancialTermsAsync(
        long portfolioId, SaveFinancialTermsRequest request, CancellationToken cancellationToken)
    {
        var r = request;
        var entity = await Db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken)
                     ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var errors = new ValidationErrors();
        RatesAndFees(errors, r.CoveragePct, r.ReservedAmountPct, r.PvRate, r.LateFeeRate, r.MarginPct, r.AdminExpensePct,
            r.LegalFeePerUnitAmount, r.ValuationFeePerUnitAmount);
        errors.ThrowIfAny();

        if (entity.RowVersion != r.RowVersion)
            throw new ConflictException($"Portfolio {portfolioId} was changed by another user. Reload and try again.");

        // Page 203 saves exactly what was entered, blanks included.
        entity.CoveragePct = r.CoveragePct;
        entity.MarginPct = r.MarginPct;
        entity.PvRate = r.PvRate;
        entity.ReservedAmountPct = r.ReservedAmountPct;
        entity.LateFeeRate = r.LateFeeRate;
        entity.AdminExpensePct = r.AdminExpensePct;
        entity.LegalFeePerUnitAmount = r.LegalFeePerUnitAmount;
        entity.ValuationFeePerUnitAmount = r.ValuationFeePerUnitAmount;

        try
        {
            await Db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"Portfolio {portfolioId} was changed by another user. Reload and try again.");
        }

        return await GetAsync(portfolioId, cancellationToken);
    }

    /// <summary>Rate and fee checks shared by pages 202 and 203.</summary>
    private static void RatesAndFees(
        ValidationErrors errors, decimal? coverage, decimal? reserved, decimal? pv, decimal? lateFee, decimal? margin,
        decimal? adminExpense, decimal? legalFee, decimal? valuationFee)
    {
        errors.Range(nameof(SavePortfolioRequest.CoveragePct), "Coverage Percentage", coverage, 0, 100);
        errors.Range(nameof(SavePortfolioRequest.ReservedAmountPct), "Reserved Amount Percentage", reserved, 0, 100);
        errors.Range(nameof(SavePortfolioRequest.PvRate), "Present Value Calculation Rate", pv, 0, MaxPct);
        errors.Range(nameof(SavePortfolioRequest.LateFeeRate), "Late Fees Rate", lateFee, 0, MaxPct);
        errors.Range(nameof(SavePortfolioRequest.MarginPct), "Margin Percentage", margin, 0, MaxPct);
        errors.Range(nameof(SavePortfolioRequest.AdminExpensePct), "Admin Expense %", adminExpense, 0, MaxPct);
        errors.Range(nameof(SavePortfolioRequest.LegalFeePerUnitAmount), "Legal Fees per Unit", legalFee, 0, MaxAmount);
        errors.Range(nameof(SavePortfolioRequest.ValuationFeePerUnitAmount), "Valuation Expense per Unit", valuationFee, 0, MaxAmount);
    }
}
