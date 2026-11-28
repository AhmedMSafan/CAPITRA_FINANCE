using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>
/// J52_SEARCH: the posted movements of the primary ledger for the chosen periods and segment values, each with the running balance,
/// the balance brought forward and the totals; with x02 = 'init' also the periods, segments and values.
/// </summary>
public sealed class P52Search(ICapitraDbContext db, IClock clock) : GlJournalPageProcess
{
    public override int Page => 52;

    public override string Name => "J52_SEARCH";

    public override string Permission => Permissions.Gl.JournalView;

    public override async Task<object> RunAsync(Modules.Apex.ApexRequest request, CancellationToken ct)
    {
        var init = request.X02 == "init";
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
        var o = doc.RootElement;
        var today = GlTime.Today(clock);
        var led = await db.Set<GlLedger>().OrderBy(l => l.IsPrimaryLedger ? 0 : 1).ThenBy(l => l.LedgerId).Select(l => (long?)l.LedgerId).FirstOrDefaultAsync(ct);
        var delim = await (from l in db.Set<GlLedger>()
                           join c in db.Set<GlCoaStructure>() on l.CoaId equals c.CoaId
                           where l.LedgerId == led
                           select c.DelimiterChar).MaxAsync(ct) ?? "-";
        var periods = from p in db.Set<GlAccountingPeriod>()
                      join lp in db.Set<GlLedgerPeriod>() on p.PeriodId equals lp.PeriodId
                      where lp.LedgerId == led
                      select p;

        // current period (today) unless chosen
        var current = await periods.Where(p => p.StartDate <= today && p.EndDate >= today && !p.IsAdjustmentPeriod).Select(p => p.PeriodCode).FirstOrDefaultAsync(ct);
        var from = Get(o, "from") ?? current;
        var to = Get(o, "to") ?? current;
        var q = Text(Get(o, "q"))?.ToUpperInvariant();
        var fs = await periods.Where(p => p.PeriodCode == from).MinAsync(p => (DateOnly?)p.StartDate, ct);
        var te = await periods.Where(p => p.PeriodCode == to).MaxAsync(p => (DateOnly?)p.EndDate, ct);
        if (fs > te)
            (fs, te) = (te, fs);
        var rx = SegmentRegex(o, delim);

        decimal bf = 0, dr = 0, cr = 0, end = 0;
        var n = 0;
        var more = false;
        var rows = new List<object>();
        if (rx is not null || q is not null)
        {
            var impacts = from i in db.Set<GlPostingBalanceImpact>()
                          join c in db.Set<GlAccountCombination>() on i.AccountCombinationId equals c.AccountCombinationId
                          join lp in db.Set<GlLedgerPeriod>() on i.LedgerPeriodId equals lp.LedgerPeriodId
                          join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                          where i.ImpactStatus == "APPLIED" && i.LedgerId == led && (rx == null || Regex.IsMatch(c.ConcatenatedAccount, rx))
                          select new { i, c.ConcatenatedAccount, p.StartDate, p.EndDate };

            // balance brought forward: every posted movement before the first chosen period
            bf = await impacts.Where(x => x.StartDate < fs).SumAsync(x => (decimal?)(x.i.AccountedDr - x.i.AccountedCr), ct) ?? 0;
            end = bf;
            var movements = await (from x in impacts
                                   join h in db.Set<GlJournalHeader>() on x.i.JournalHeaderId equals h.JournalHeaderId
                                   join b in db.Set<GlJournalBatch>() on h.BatchId equals b.BatchId
                                   join l in db.Set<GlJournalLine>() on x.i.JournalLineId equals l.JournalLineId into ls
                                   from l in ls.DefaultIfEmpty()
                                   where x.StartDate >= fs && x.EndDate <= te
                                   orderby x.i.AccountingDate, x.i.CreatedAt, x.i.BalanceImpactId
                                   select new
                                   {
                                       x.i.AccountingDate,
                                       Dr = x.i.AccountedDr,
                                       Cr = x.i.AccountedCr,
                                       x.ConcatenatedAccount,
                                       h.JournalHeaderId,
                                       h.JournalNo,
                                       h.JournalName,
                                       b.SourceBatchRef,
                                       LineDescription = (string?)l.LineDescription,
                                   }).ToListAsync(ct);
            foreach (var r in movements)
            {
                dr += r.Dr;
                cr += r.Cr;
                var after = end + r.Dr - r.Cr;
                end = after;
                if (q is null || r.JournalNo.ToUpperInvariant().Contains(q, StringComparison.Ordinal) || r.JournalName.ToUpperInvariant().Contains(q, StringComparison.Ordinal)
                    || (r.SourceBatchRef ?? string.Empty).ToUpperInvariant().Contains(q, StringComparison.Ordinal))
                {
                    n++;
                    if (n <= 2000)
                    {
                        rows.Add(new
                        {
                            date = Ymd(r.AccountingDate),
                            no = r.JournalNo,
                            name = r.JournalName,
                            tran = r.SourceBatchRef,
                            concat = r.ConcatenatedAccount,
                            descr = r.LineDescription,
                            dr = r.Dr,
                            cr = r.Cr,
                            before = after - r.Dr + r.Cr,
                            after,
                            match = q is not null && rx is not null,
                            url = JournalUrl(r.JournalHeaderId),
                        });
                    }
                    else
                    {
                        more = true;
                    }
                }
            }
        }

        var res = new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["rows"] = rows,
            ["more"] = more,
            ["from"] = from,
            ["to"] = to,
            ["broughtForward"] = bf,
            ["totalDr"] = dr,
            ["totalCr"] = cr,
            ["closing"] = end,
        };
        if (init)
        {
            if (current is not null)
                res["currentPeriod"] = current;
            res["periods"] = await periods.OrderByDescending(p => p.StartDate).ThenBy(p => p.PeriodCode).Select(p => p.PeriodCode).ToListAsync(ct);
            var coa = await db.Set<GlLedger>().Where(l => l.LedgerId == led).Select(l => l.CoaId).FirstOrDefaultAsync(ct);
            (res["segments"], res["values"]) = await SegmentListsAsync(db, coa, ct);
        }

        return res;
    }
}
