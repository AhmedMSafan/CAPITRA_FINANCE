using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Portfolios;

public sealed record PortfolioAccountDto(
    long PortfolioAccountId,
    long PortfolioId,
    long InvestorId,
    string InvestorCode,
    string InvestorName,
    long ProjectUnitId,
    string UnitCode,
    string? SellerContractNo,
    DateOnly? BookingDate,
    DateOnly? ContractDate,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal? InstallmentAmount,
    string? InstallmentFrequency,
    DateOnly? LastPaymentDate,
    int RegularPaymentMonths,
    decimal ArrearsAmount,
    string? AccountStatus,
    int RowVersion);

public sealed record SavePortfolioAccountRequest(
    long PortfolioId,
    long InvestorId,
    long ProjectUnitId,
    DateOnly? BookingDate,
    DateOnly? ContractDate,
    decimal? ContractAmount,
    decimal? PaidAmount,
    decimal? OutstandingAmount,
    decimal? InstallmentAmount,
    string? InstallmentFrequency,
    DateOnly? LastPaymentDate,
    int? RegularPaymentMonths,
    decimal? ArrearsAmount,
    string? AccountStatus,
    int? RowVersion) : ISaveRequest;

public interface IMtgPortfolioAccountService : ICrudService<PortfolioAccountDto, SavePortfolioAccountRequest>
{
    Task<IReadOnlyList<PortfolioAccountDto>> ListAsync(long portfolioId, CancellationToken cancellationToken);
}

/// <summary>
/// Portfolio accounts, APEX page 205. Every save or delete recalculates the portfolio totals in the same transaction,
/// as the page's "Recalculate Portfolio" process does.
/// </summary>
public sealed class MtgPortfolioAccountService(ICapitraDbContext db)
    : CrudService<MtgPurchasedPortfolioAccount, PortfolioAccountDto, SavePortfolioAccountRequest>(db), IMtgPortfolioAccountService
{
    private const decimal MaxAmount = 9_999_999_999_999_999m;

    protected override string Title => "Portfolio account";

    public Task<IReadOnlyList<PortfolioAccountDto>> ListAsync(long portfolioId, CancellationToken cancellationToken) =>
        ListAsync(x => x.PortfolioId == portfolioId, cancellationToken);

    protected override long IdOf(MtgPurchasedPortfolioAccount entity) => entity.PortfolioAccountId;

    protected override Expression<Func<MtgPurchasedPortfolioAccount, bool>> HasId(long id) => x => x.PortfolioAccountId == id;

    protected override IQueryable<PortfolioAccountDto> Project(IQueryable<MtgPurchasedPortfolioAccount> rows) =>
        from a in rows
        join i in Db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
        join u in Db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId
        orderby i.InvestorCode, u.UnitCode
        select new PortfolioAccountDto(
            a.PortfolioAccountId, a.PortfolioId, a.InvestorId, i.InvestorCode, i.FullNameEn ?? i.FullNameAr, a.ProjectUnitId,
            u.UnitCode, a.SellerContractNo, a.BookingDate, a.ContractDate, a.ContractAmount, a.PaidAmount, a.OutstandingAmount, a.InstallmentAmount,
            a.InstallmentFrequency, a.LastPaymentDate, a.RegularPaymentMonths, a.ArrearsAmount, a.AccountStatus, a.RowVersion);

    protected override async Task ValidateAsync(
        SavePortfolioAccountRequest r, MtgPurchasedPortfolioAccount? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        errors.Text(nameof(r.InstallmentFrequency), "Installment Frequency", r.InstallmentFrequency, 30);
        errors.Text(nameof(r.AccountStatus), "Account Status", r.AccountStatus, 30);
        Amount(errors, nameof(r.ContractAmount), "Contract Value", r.ContractAmount, required: true);
        Amount(errors, nameof(r.PaidAmount), "Paid Value", r.PaidAmount, required: true);
        Amount(errors, nameof(r.OutstandingAmount), "Outstanding Value", r.OutstandingAmount, required: true);
        Amount(errors, nameof(r.ArrearsAmount), "Arrears Amount", r.ArrearsAmount, required: true);
        Amount(errors, nameof(r.InstallmentAmount), "Installment Amount", r.InstallmentAmount, required: false);
        if (r.RegularPaymentMonths is null)
            errors.Add(nameof(r.RegularPaymentMonths), "Regular Payment Months is required.");
        else if (r.RegularPaymentMonths < 0)
            errors.Add(nameof(r.RegularPaymentMonths), "Regular Payment Months cannot be negative.");

        // The portfolio is shown, not chosen, on page 205: an account never moves to another portfolio.
        if (existing is not null && existing.PortfolioId != r.PortfolioId)
            errors.Add(nameof(r.PortfolioId), "An account cannot be moved to another portfolio.");

        var projectId = await Db.Set<MtgPurchasedPortfolio>()
            .Where(x => x.PortfolioId == r.PortfolioId)
            .Select(x => (long?)x.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);
        if (projectId is null)
        {
            errors.Add(nameof(r.PortfolioId), $"Portfolio {r.PortfolioId} was not found.");
            return;
        }

        if (!await IsSelectableAsync(
                Db.Set<MtgInvestor>().Where(x => x.InvestorId == r.InvestorId && x.IsActive),
                existing?.InvestorId == r.InvestorId, cancellationToken))
            errors.Add(nameof(r.InvestorId), "Select an active investor.");

        // LOV FNV_PROJECT_UNIT_BY_PORTFOLIO: active units of the portfolio's project.
        if (!await IsSelectableAsync(
                Db.Set<MtgProjectUnit>().Where(x => x.ProjectUnitId == r.ProjectUnitId && x.ProjectId == projectId && x.IsActive),
                existing?.ProjectUnitId == r.ProjectUnitId, cancellationToken))
            errors.Add(nameof(r.ProjectUnitId), "Select an active unit of the portfolio's project.");
    }

    protected override void Apply(MtgPurchasedPortfolioAccount e, SavePortfolioAccountRequest r)
    {
        e.PortfolioId = r.PortfolioId;
        e.InvestorId = r.InvestorId;
        e.ProjectUnitId = r.ProjectUnitId;
        e.BookingDate = r.BookingDate;
        e.ContractDate = r.ContractDate;
        e.ContractAmount = r.ContractAmount!.Value;
        e.PaidAmount = r.PaidAmount!.Value;
        e.OutstandingAmount = r.OutstandingAmount!.Value;
        e.InstallmentAmount = r.InstallmentAmount;
        e.InstallmentFrequency = MasterDataRules.Clean(r.InstallmentFrequency);
        e.LastPaymentDate = r.LastPaymentDate;
        e.RegularPaymentMonths = r.RegularPaymentMonths!.Value;
        e.ArrearsAmount = r.ArrearsAmount!.Value;
        e.AccountStatus = MasterDataRules.Clean(r.AccountStatus);
    }

    protected override async Task BeforeDeleteAsync(MtgPurchasedPortfolioAccount entity, CancellationToken cancellationToken)
    {
        if (await Db.Set<MtgAccountPayment>().AnyAsync(x => x.PortfolioAccountId == entity.PortfolioAccountId, cancellationToken))
            throw new ConflictException("This account has payment history. Delete its payments first.");
    }

    protected override Task AfterChangeAsync(MtgPurchasedPortfolioAccount entity, CancellationToken cancellationToken) =>
        PortfolioTotals.RecalculateAsync(Db, entity.PortfolioId, cancellationToken);

    private static void Amount(ValidationErrors errors, string field, string label, decimal? value, bool required)
    {
        if (value is null && required)
            errors.Add(field, $"{label} is required.");
        errors.Range(field, label, value, 0, MaxAmount);
    }
}
