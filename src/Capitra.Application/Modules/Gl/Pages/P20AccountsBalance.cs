using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>
/// App 120 page 20 "Accounts Balance": the interactive report on GL_ACCOUNT_BALANCE_SUMMARY_V, the balances of each
/// account combination per ledger period with their ledger, period, account and last posted journal.
/// </summary>
public sealed class P20AccountsBalance(ICapitraDbContext db) : IApexProcess
{
    public int App => 120;

    public int Page => 20;

    public string Name => "IR_ACCOUNTS_BALANCE";

    public string Permission => Permissions.Gl.JournalView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var rows = await (from b in db.Set<GlAccountBalance>()
                          join l in db.Set<GlLedger>() on b.LedgerId equals l.LedgerId
                          join lp in db.Set<GlLedgerPeriod>() on b.LedgerPeriodId equals lp.LedgerPeriodId
                          join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                          join c in db.Set<GlAccountCombination>() on b.AccountCombinationId equals c.AccountCombinationId
                          join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId into accs
                          from a in accs.DefaultIfEmpty()
                          join h in db.Set<GlJournalHeader>() on b.LastPostedHeaderId equals h.JournalHeaderId into hs
                          from h in hs.DefaultIfEmpty()
                          select new { b, l.LedgerCode, l.LedgerNameEn, p.PeriodCode, c.ConcatenatedAccount, c.DisplayAccount, c.AccountId, a, JournalNo = (string?)h.JournalNo })
            .AsNoTracking().ToListAsync(ct);

        var report = rows.Select(r => new ApexReport.Row(r.b.BalanceId, new Dictionary<string, object?>
        {
            ["BALANCE_ID"] = r.b.BalanceId,
            ["LEDGER_ID"] = r.b.LedgerId,
            ["LEDGER_CODE"] = r.LedgerCode,
            ["LEDGER_NAME_EN"] = r.LedgerNameEn,
            ["LEDGER_PERIOD_ID"] = r.b.LedgerPeriodId,
            ["PERIOD_CODE"] = r.PeriodCode,
            ["ACCOUNT_COMBINATION_ID"] = r.b.AccountCombinationId,
            ["CONCATENATED_ACCOUNT"] = r.ConcatenatedAccount,
            ["DISPLAY_ACCOUNT"] = r.DisplayAccount,
            ["ACCOUNT_ID"] = r.AccountId,
            ["ACCOUNT_CODE"] = r.a?.AccountCode,
            ["ACCOUNT_NAME_EN"] = r.a?.AccountNameEn,
            ["ACCOUNT_NAME_AR"] = r.a?.AccountNameAr,
            ["ACCOUNT_TYPE"] = r.a?.AccountType,
            ["FINANCIAL_STATEMENT_CLASS"] = r.a?.FinancialStatementClass,
            ["CURRENCY_CODE"] = r.b.CurrencyCode,
            ["OPENING_DR"] = r.b.OpeningDr,
            ["OPENING_CR"] = r.b.OpeningCr,
            ["OPENING_NET_DR_CR"] = r.b.OpeningDr - r.b.OpeningCr,
            ["PERIOD_DR"] = r.b.PeriodDr,
            ["PERIOD_CR"] = r.b.PeriodCr,
            ["PERIOD_NET_DR_CR"] = r.b.PeriodDr - r.b.PeriodCr,
            ["CLOSING_DR"] = r.b.ClosingDr,
            ["CLOSING_CR"] = r.b.ClosingCr,
            ["CLOSING_NET_DR_CR"] = r.b.ClosingDr - r.b.ClosingCr,
            ["LAST_POSTED_HEADER_ID"] = r.b.LastPostedHeaderId,
            ["LAST_POSTED_JOURNAL_NO"] = r.JournalNo,
            ["LAST_POSTED_AT"] = r.b.LastPostedAt,
            ["CREATED_BY"] = r.b.CreatedBy,
            ["CREATED_AT"] = r.b.CreatedAt,
            ["UPDATED_BY"] = r.b.UpdatedBy,
            ["UPDATED_AT"] = r.b.UpdatedAt,
        })).ToList();

        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
        return ApexReport.Page(report, doc.RootElement);
    }
}
