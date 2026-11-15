using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Col;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndWsCustomerDto(
    long ClientId, string ClientNumber, string ClientName, string ClientType, string? PhoneNumber, string? Email, bool IsActive,
    int ActiveLoans, decimal OutstandingPrincipal);

public sealed record LndWsLoanDto(
    long LoanId, string LoanNumber, string ProductCode, string ProductName, string LicenceCode, string StatusCode, decimal PrincipalAmount,
    decimal PrincipalOutstanding, decimal OverdueAmount, int DaysLate, bool IsNonAccrual, DateOnly? NextDueDate, decimal NextDueAmount,
    DateOnly DisbursementDate, DateOnly MaturityDate);

public sealed record LndWsPaymentDto(
    long RepaymentTxnId, string RepaymentNumber, string LoanNumber, string KindCode, DateOnly ValueDate, string Method, decimal ReceiptAmount,
    decimal AllocatedAmount, string StatusCode);

public sealed record LndWsSummaryDto(int ActiveLoans, decimal OutstandingPrincipal, decimal OverdueAmount, int MaxDaysLate, decimal CreditBalance);

public sealed record LndWorkspaceDto(
    long ClientId, string ClientNumber, string ClientName, string ClientNameAr, string ClientType, string? NationalId, string? PhoneNumber,
    string? MobileNumber, string? Email, string? RelationshipManager, DateOnly Today, LndWsSummaryDto Summary,
    IReadOnlyList<LndWsLoanDto> Loans, IReadOnlyList<LndWsPaymentDto> Payments);

/// <summary>
/// The Lending Workspace (Ahmed, 15 Nov 2026: "start lending the same way" as the Factoring Workspace): the customers, and for one
/// customer everything of microfinance and consumer finance in one place: the summary, the loans and the payments. Loans are opened,
/// paid out and paid on their own page; a new loan starts here with the customer filled in.
/// </summary>
public sealed class LndWorkspaceService(ICapitraDbContext db, IClock clock, LndReceiptService receipts)
{
    public async Task<IReadOnlyList<LndWsCustomerDto>> ListCustomersAsync(CancellationToken cancellationToken)
    {
        var loans = await (from l in db.Set<LndLoan>().AsNoTracking()
                           where l.StatusCode == St.Active
                           join i in db.Set<LndInstallment>().AsNoTracking() on l.LoanId equals i.LoanId
                           group i by new { l.ClientId, l.LoanId } into g
                           select new { g.Key.ClientId, Outstanding = g.Sum(i => i.PrincipalDueAmount - i.PrincipalPaidAmount) })
            .ToListAsync(cancellationToken);
        var byClient = loans.GroupBy(x => x.ClientId).ToDictionary(g => g.Key, g => (Count: g.Count(), Outstanding: g.Sum(x => x.Outstanding)));
        var clients = await db.Set<CrmClient>().AsNoTracking().OrderBy(x => x.ClientNumber)
            .Select(x => new { x.ClientId, x.ClientNumber, x.ClientName, x.ClientType, x.PhoneNumber, x.MobileNumber, x.Email, x.IsActive })
            .ToListAsync(cancellationToken);
        return [.. clients.Select(c =>
        {
            var s = byClient.GetValueOrDefault(c.ClientId);
            return new LndWsCustomerDto(c.ClientId, c.ClientNumber, c.ClientName, c.ClientType, c.MobileNumber ?? c.PhoneNumber, c.Email, c.IsActive, s.Count, s.Outstanding);
        })];
    }

    public async Task<LndWorkspaceDto> GetAsync(long clientId, CancellationToken cancellationToken)
    {
        var c = await db.Set<CrmClient>().AsNoTracking().SingleOrDefaultAsync(x => x.ClientId == clientId, cancellationToken)
            ?? throw new NotFoundException($"Customer {clientId} was not found.");
        var today = GlTime.Today(clock);
        var loans = await (from l in db.Set<LndLoan>().AsNoTracking()
                           where l.ClientId == clientId && l.StatusCode != St.Cancelled
                           join p in db.Set<AdmProduct>().AsNoTracking() on l.ProductId equals p.ProductId
                           orderby l.LoanId descending
                           select new { l, p.ProductCode, p.ProductNameEn }).ToListAsync(cancellationToken);
        var ids = loans.Select(x => x.l.LoanId).ToList();
        var schedules = (await db.Set<LndInstallment>().AsNoTracking().Where(x => ids.Contains(x.LoanId)).ToListAsync(cancellationToken)).ToLookup(x => x.LoanId);

        var rows = loans.Select(x =>
        {
            var s = schedules[x.l.LoanId].OrderBy(i => i.InstallmentNo).ToList();
            var open = x.l.StatusCode == St.Active;
            var next = open ? s.FirstOrDefault(i => i.DueDate > today && i.StatusCode != "PAID") : null;
            return new LndWsLoanDto(x.l.LoanId, x.l.LoanNumber, x.ProductCode, x.ProductNameEn, x.l.LicenceCode, x.l.StatusCode, x.l.PrincipalAmount,
                open ? s.Sum(i => i.PrincipalDueAmount - i.PrincipalPaidAmount) : 0,
                open ? LndPrepayment.Dues(s, today) : 0,
                open ? LndEod.DaysLate(s, today) : 0, x.l.IsNonAccrual, next?.DueDate,
                next is null ? 0 : LndReceiptService.Owed(next).Values.Sum(), x.l.DisbursementDate, x.l.MaturityDate);
        }).ToList();

        var events = await db.Set<LndLoanEvent>().AsNoTracking()
            .Where(x => ids.Contains(x.LoanId) && x.SourceDocumentId != null
                && (x.EventCode == "RECEIPT" || x.EventCode == "RECOVERY" || x.EventCode == "PREPAYMENT" || x.EventCode == "SETTLEMENT"))
            .Select(x => new { x.LoanId, x.EventCode, TxnId = x.SourceDocumentId!.Value }).ToListAsync(cancellationToken);
        var txnIds = events.Select(x => x.TxnId).Distinct().ToList();
        var txns = await db.Set<ColRepaymentTransaction>().AsNoTracking().Where(x => txnIds.Contains(x.RepaymentTxnId)).ToDictionaryAsync(x => x.RepaymentTxnId, cancellationToken);
        var numbers = loans.ToDictionary(x => x.l.LoanId, x => x.l.LoanNumber);
        var payments = events.Where(e => txns.ContainsKey(e.TxnId)).GroupBy(e => e.TxnId).Select(g =>
        {
            var e = g.First();
            var t = txns[e.TxnId];
            return new LndWsPaymentDto(t.RepaymentTxnId, t.RepaymentNumber, numbers[e.LoanId], e.EventCode, t.ValueDate, t.RepaymentMethod, t.ReceiptAmount,
                t.AllocatedAmount, t.StatusCode);
        }).OrderByDescending(x => x.ValueDate).ThenByDescending(x => x.RepaymentTxnId).ToList();

        var active = rows.Where(x => x.StatusCode == St.Active).ToList();
        var summary = new LndWsSummaryDto(active.Count, active.Sum(x => x.PrincipalOutstanding), active.Sum(x => x.OverdueAmount),
            active.Count == 0 ? 0 : active.Max(x => x.DaysLate), await receipts.CreditAsync(clientId, cancellationToken));
        return new LndWorkspaceDto(c.ClientId, c.ClientNumber, c.ClientName, c.ClientNameAr, c.ClientType, c.NationalId, c.PhoneNumber, c.MobileNumber,
            c.Email, c.RelationshipManager, today, summary, rows, payments);
    }
}
