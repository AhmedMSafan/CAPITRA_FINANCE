using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Portfolios;

public sealed record AccountPaymentDto(
    long AccountPaymentId,
    long PortfolioAccountId,
    DateOnly DueDate,
    decimal DueAmount,
    DateOnly? PaymentDate,
    decimal PaidAmount,
    string PaymentStatusCode,
    string PaymentStatusName,
    int DaysLate,
    string? ChequeNo,
    string? BankName,
    int RowVersion);

public sealed record SaveAccountPaymentRequest(
    long PortfolioAccountId,
    DateOnly? DueDate,
    decimal? DueAmount,
    DateOnly? PaymentDate,
    decimal? PaidAmount,
    string? PaymentStatusCode,
    int? DaysLate,
    string? ChequeNo,
    string? BankName,
    int? RowVersion) : ISaveRequest;

public interface IMtgAccountPaymentService : ICrudService<AccountPaymentDto, SaveAccountPaymentRequest>
{
    Task<IReadOnlyList<AccountPaymentDto>> ListAsync(long portfolioAccountId, CancellationToken cancellationToken);
}

/// <summary>Payment history of a portfolio account, APEX pages 206 (list, newest due date first) and 207 (form).</summary>
public sealed class MtgAccountPaymentService(ICapitraDbContext db)
    : CrudService<MtgAccountPayment, AccountPaymentDto, SaveAccountPaymentRequest>(db), IMtgAccountPaymentService
{
    private const decimal MaxAmount = 9_999_999_999_999_999m;

    protected override string Title => "Payment";

    public Task<IReadOnlyList<AccountPaymentDto>> ListAsync(long portfolioAccountId, CancellationToken cancellationToken) =>
        ListAsync(x => x.PortfolioAccountId == portfolioAccountId, cancellationToken);

    protected override long IdOf(MtgAccountPayment entity) => entity.AccountPaymentId;

    protected override Expression<Func<MtgAccountPayment, bool>> HasId(long id) => x => x.AccountPaymentId == id;

    protected override IQueryable<AccountPaymentDto> Project(IQueryable<MtgAccountPayment> rows) =>
        from p in rows
        join s in Db.Set<AdmLkpPaymentStatus>() on p.PaymentStatusCode equals s.Code
        orderby p.DueDate descending
        select new AccountPaymentDto(
            p.AccountPaymentId, p.PortfolioAccountId, p.DueDate, p.DueAmount, p.PaymentDate, p.PaidAmount, p.PaymentStatusCode,
            s.NameEn, p.DaysLate, p.ChequeNo, p.BankName, p.RowVersion);

    protected override async Task ValidateAsync(
        SaveAccountPaymentRequest r, MtgAccountPayment? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        if (r.DueDate is null)
            errors.Add(nameof(r.DueDate), "Due Date is required.");
        if (r.DueAmount is null)
            errors.Add(nameof(r.DueAmount), "Due Amount is required.");
        errors.Range(nameof(r.DueAmount), "Due Amount", r.DueAmount, 0, MaxAmount);
        if (r.PaidAmount is null)
            errors.Add(nameof(r.PaidAmount), "Paid Amount is required.");
        errors.Range(nameof(r.PaidAmount), "Paid Amount", r.PaidAmount, 0, MaxAmount);
        if (r.DaysLate is null)
            errors.Add(nameof(r.DaysLate), "Days Late is required.");
        else if (r.DaysLate < 0)
            errors.Add(nameof(r.DaysLate), "Days Late cannot be negative.");
        errors.Text(nameof(r.ChequeNo), "Cheque No.", r.ChequeNo, 100);
        errors.Text(nameof(r.BankName), "Bank Name", r.BankName, 300);

        if (existing is not null && existing.PortfolioAccountId != r.PortfolioAccountId)
            errors.Add(nameof(r.PortfolioAccountId), "A payment cannot be moved to another account.");
        else if (existing is null
                 && !await Db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(x => x.PortfolioAccountId == r.PortfolioAccountId, cancellationToken))
            errors.Add(nameof(r.PortfolioAccountId), $"Portfolio account {r.PortfolioAccountId} was not found.");

        var status = MasterDataRules.Code(r.PaymentStatusCode);
        if (status is null)
            errors.Add(nameof(r.PaymentStatusCode), "Payment Status is required.");
        else if (!await IsSelectableAsync(
                     Db.Set<AdmLkpPaymentStatus>().Where(x => x.Code == status && x.IsActive),
                     existing?.PaymentStatusCode == status, cancellationToken))
            errors.Add(nameof(r.PaymentStatusCode), $"Payment status '{status}' is not an active payment status.");
    }

    protected override void Apply(MtgAccountPayment e, SaveAccountPaymentRequest r)
    {
        e.PortfolioAccountId = r.PortfolioAccountId;
        e.DueDate = r.DueDate!.Value;
        e.DueAmount = r.DueAmount!.Value;
        e.PaymentDate = r.PaymentDate;
        e.PaidAmount = r.PaidAmount!.Value;
        e.PaymentStatusCode = MasterDataRules.Code(r.PaymentStatusCode)!;
        e.DaysLate = r.DaysLate!.Value;
        e.ChequeNo = MasterDataRules.Clean(r.ChequeNo);
        e.BankName = MasterDataRules.Clean(r.BankName);
    }
}
