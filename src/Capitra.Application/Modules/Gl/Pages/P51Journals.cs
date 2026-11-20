using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>J51_SEARCH: the journals that match the filters (x01 JSON), the counts per status, and with x02 = 'init' the pick lists.</summary>
public sealed class P51Search(ICapitraDbContext db, GlPostingApi api, IClock clock) : GlJournalPageProcess
{
    public override int Page => 51;

    public override string Name => "J51_SEARCH";

    public override string Permission => Permissions.Gl.JournalView;

    /// <summary>to_number(p, 'FM999999999999999999D99999'), null when it is not a number.</summary>
    private static decimal? Nm(string? value) =>
        decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) ? n : null;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            return await SearchAsync(request, ct);
        }
        catch (Exception ex) when (Handled(ex) || ex is JsonException)
        {
            return new { ok = false, message = GlPostingApi.CleanMessage(ex.Message) };
        }
    }

    private async Task<object> SearchAsync(ApexRequest request, CancellationToken ct)
    {
        var me = api.CurrentUserName.ToUpperInvariant();
        var init = request.X02 == "init";
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
        var o = doc.RootElement;
        var stage = Get(o, "stage")?.ToUpperInvariant();
        var q = Text(Get(o, "q"))?.ToUpperInvariant();
        var tran = Text(Get(o, "tran"))?.ToUpperInvariant();
        var led = Get(o, "ledger") is { } ls ? (long?)decimal.Parse(ls, CultureInfo.InvariantCulture) : null;
        var src = Get(o, "source");
        var cat = Get(o, "category");
        var per = Get(o, "period");
        var from = Get(o, "from") is { } f ? DateOnly.ParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateOnly?)null;
        var to = Get(o, "to") is { } t ? DateOnly.ParseExact(t, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateOnly?)null;
        var min = Nm(Get(o, "min"));
        var max = Nm(Get(o, "max"));
        var by = Text(Get(o, "by"))?.ToUpperInvariant();
        var delim = await db.Set<GlCoaStructure>().MaxAsync(s => s.DelimiterChar, ct) ?? "-";
        var rx = SegmentRegex(o, delim);

        var totals = db.Set<GlJournalLine>().Where(l => l.IsActive).GroupBy(l => l.JournalHeaderId)
            .Select(g => new { Id = g.Key, Dr = g.Sum(l => l.AccountedDr), N = g.Count() });
        var j = from h in db.Set<GlJournalHeader>()
                join b in db.Set<GlJournalBatch>() on h.BatchId equals b.BatchId
                join s in db.Set<GlJournalSource>() on h.SourceId equals s.SourceId
                join c in db.Set<GlJournalCategory>() on h.CategoryId equals c.CategoryId into cs
                from c in cs.DefaultIfEmpty()
                join lp in db.Set<GlLedgerPeriod>() on h.LedgerPeriodId equals lp.LedgerPeriodId into lps
                from lp in lps.DefaultIfEmpty()
                join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId into ps
                from p in ps.DefaultIfEmpty()
                join tt in totals on h.JournalHeaderId equals tt.Id into ts
                from tt in ts.DefaultIfEmpty()
                select new
                {
                    h,
                    b.BatchNo,
                    b.SourceBatchRef,
                    s.SourceCode,
                    CategoryCode = (string?)c.CategoryCode,
                    PeriodCode = (string?)p.PeriodCode,
                    Dr = tt == null ? 0 : tt.Dr,
                    N = tt == null ? 0 : tt.N,
                    Stage = h.JournalStatus == "CANCELLED" ? "CANCELLED"
                        : h.JournalStatus == "REVERSED" || h.PostingStatus == "REVERSED" ? "REVERSED"
                        : h.PostingStatus == "POSTED" ? "POSTED"
                        : h.JournalStatus == "ERROR" || h.PostingStatus == "POSTING_ERROR" ? "ERROR"
                        : h.ApprovalStatus == "SUBMITTED" || h.ApprovalStatus == "IN_REVIEW" ? "SUBMITTED"
                        : h.ApprovalStatus == "APPROVED" || h.ApprovalStatus == "NOT_REQUIRED" ? "APPROVED"
                        : h.ApprovalStatus == "REJECTED" || h.ApprovalStatus == "RETURNED" ? "REJECTED"
                        : "DRAFT",
                };

#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), as the PL/SQL compared.
        if (stage is not null)
        {
            j = stage switch
            {
                "MINE" => j.Where(x => x.Stage == "SUBMITTED" && (x.h.SubmittedBy ?? x.h.CreatedBy).ToUpper() != me),
                "ERROR" => j.Where(x => x.Stage == "ERROR" || x.Stage == "REJECTED"),
                _ => j.Where(x => x.Stage == stage),
            };
        }

        if (q is not null)
        {
            j = j.Where(x => x.h.JournalNo.ToUpper().Contains(q) || x.h.JournalName.ToUpper().Contains(q) || x.BatchNo.ToUpper().Contains(q)
                             || (x.SourceBatchRef ?? string.Empty).ToUpper().Contains(q) || (x.h.ReferenceNo ?? string.Empty).ToUpper().Contains(q)
                             || (x.h.SourceDocumentNo ?? string.Empty).ToUpper().Contains(q)
                             || (x.h.Description ?? string.Empty).Substring(0, 1000).ToUpper().Contains(q));
        }

        if (tran is not null)
            j = j.Where(x => (x.SourceBatchRef ?? string.Empty).ToUpper().StartsWith(tran));
        if (led is not null)
            j = j.Where(x => x.h.LedgerId == led);
        if (src is not null)
            j = j.Where(x => x.SourceCode == src);
        if (cat is not null)
            j = j.Where(x => x.CategoryCode == cat);
        if (per is not null)
            j = j.Where(x => x.PeriodCode == per);
        if (from is not null)
            j = j.Where(x => x.h.AccountingDate >= from);
        if (to is not null)
            j = j.Where(x => x.h.AccountingDate <= to);
        if (min is not null)
            j = j.Where(x => x.Dr >= min);
        if (max is not null)
            j = j.Where(x => x.Dr <= max);
        if (by is not null)
            j = j.Where(x => x.h.CreatedBy.ToUpper().Contains(by));
#pragma warning restore CA1862
        if (rx is not null)
        {
            j = j.Where(x => (from jl in db.Set<GlJournalLine>()
                              join ac in db.Set<GlAccountCombination>() on jl.AccountCombinationId equals ac.AccountCombinationId
                              where jl.JournalHeaderId == x.h.JournalHeaderId && jl.IsActive && Regex.IsMatch(ac.ConcatenatedAccount, rx)
                              select jl).Any());
        }

        var found = await j.OrderByDescending(x => x.h.AccountingDate).ThenByDescending(x => x.h.JournalHeaderId).Take(501).ToListAsync(ct);
        var more = found.Count > 500;
        var shown = found.Take(500).ToList();

        // the last posting failure of approved journals
        var approvedIds = shown.Where(r => r.Stage == "APPROVED").Select(r => r.h.JournalHeaderId).ToList();
        var failures = (await (from a in db.Set<GlJournalActionHistory>()
                               join h in db.Set<GlJournalHeader>() on a.JournalHeaderId equals h.JournalHeaderId
                               where approvedIds.Contains(h.JournalHeaderId) && a.ActionCode == "POST_FAILED" && (h.ApprovedAt == null || a.ActionAt >= h.ApprovedAt)
                               select new { h.JournalHeaderId, a.ActionAt, a.ActionReason }).ToListAsync(ct))
            .GroupBy(a => a.JournalHeaderId).ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.ActionAt).First().ActionReason);

        var res = new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["rows"] = shown.Select(r =>
            {
                var row = new Dictionary<string, object?>
                {
                    ["id"] = r.h.JournalHeaderId,
                    ["no"] = r.h.JournalNo,
                    ["name"] = r.h.JournalName,
                    ["description"] = r.h.Description is { Length: > 200 } d ? d[..200] : r.h.Description,
                    ["batchNo"] = r.BatchNo,
                    ["tran"] = r.SourceBatchRef,
                    ["source"] = r.SourceCode,
                    ["category"] = r.CategoryCode,
                    ["date"] = Ymd(r.h.AccountingDate),
                    ["period"] = r.PeriodCode,
                    ["currency"] = r.h.CurrencyCode,
                    ["dr"] = r.Dr,
                    ["lines"] = r.N,
                    ["stage"] = r.Stage,
                    ["createdBy"] = r.h.CreatedBy,
                };
                if (failures.TryGetValue(r.h.JournalHeaderId, out var e))
                    row["postError"] = e is { Length: > 300 } ? e[..300] : e;
                row["url"] = JournalUrl(r.h.JournalHeaderId);
                return row;
            }).ToList(),
            ["more"] = more,
        };

        // counts per status (all journals)
        var stages = await (from h in db.Set<GlJournalHeader>()
                            select new
                            {
                                Stage = h.JournalStatus == "CANCELLED" ? "CANCELLED"
                                    : h.JournalStatus == "REVERSED" || h.PostingStatus == "REVERSED" ? "REVERSED"
                                    : h.PostingStatus == "POSTED" ? "POSTED"
                                    : h.JournalStatus == "ERROR" || h.PostingStatus == "POSTING_ERROR" ? "ERROR"
                                    : h.ApprovalStatus == "SUBMITTED" || h.ApprovalStatus == "IN_REVIEW" ? "SUBMITTED"
                                    : h.ApprovalStatus == "APPROVED" || h.ApprovalStatus == "NOT_REQUIRED" ? "APPROVED"
                                    : h.ApprovalStatus == "REJECTED" || h.ApprovalStatus == "RETURNED" ? "REJECTED"
                                    : "DRAFT",
                                Who = h.SubmittedBy ?? h.CreatedBy,
                            }).ToListAsync(ct);
        var kpis = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var g in stages.GroupBy(s => s.Stage).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            kpis[g.Key] = g.Count();
            if (g.Key == "SUBMITTED")
                kpis["MINE"] = g.Count(s => !string.Equals(s.Who, me, StringComparison.OrdinalIgnoreCase));
        }

        kpis["ALL"] = stages.Count;
        res["kpis"] = kpis;

        if (init)
        {
            res["newUrl"] = ApexPages.Url(120, 50);
            res["ledgerUrl"] = ApexPages.Url(120, 52);
            res["ledgers"] = await db.Set<GlLedger>().OrderBy(l => l.LedgerCode).Select(l => new { id = l.LedgerId, code = l.LedgerCode }).ToListAsync(ct);
            res["sources"] = await db.Set<GlJournalSource>().OrderBy(s => s.SourceCode).Select(s => new { code = s.SourceCode, name = s.SourceNameEn }).ToListAsync(ct);
            res["categories"] = await db.Set<GlJournalCategory>().OrderBy(c => c.CategoryCode).Select(c => new { code = c.CategoryCode, name = c.CategoryNameEn }).ToListAsync(ct);
            res["periods"] = await (from p in db.Set<GlAccountingPeriod>()
                                    join lp in db.Set<GlLedgerPeriod>() on p.PeriodId equals lp.PeriodId
                                    group p by p.PeriodCode into g
                                    orderby g.Max(p => p.StartDate) descending
                                    select g.Key).ToListAsync(ct);
            var today = GlTime.Today(clock);
            var current = await (from p in db.Set<GlAccountingPeriod>()
                                 join lp in db.Set<GlLedgerPeriod>() on p.PeriodId equals lp.PeriodId
                                 join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
                                 where p.StartDate <= today && p.EndDate >= today && !p.IsAdjustmentPeriod
                                 orderby l.IsPrimaryLedger ? 0 : 1
                                 select p.PeriodCode).FirstOrDefaultAsync(ct);
            if (current is not null)
                res["currentPeriod"] = current;
            var coa = await db.Set<GlLedger>().MinAsync(l => l.CoaId, ct);
            (res["segments"], res["values"]) = await SegmentListsAsync(db, coa, ct);
        }

        return res;
    }
}

/// <summary>J51_ACTION: one action (x01) on each of the journals in x02 (ids separated by commas), each with its own result.</summary>
public sealed class P51Action(ICapitraDbContext db, GlPostingApi api) : GlJournalPageProcess
{
    public override int Page => 51;

    public override string Name => "J51_ACTION";

    public override string Permission => Permissions.Gl.JournalEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => InTransaction(db, async () =>
    {
        var act = request.X01?.ToUpperInvariant();
        var txt = request.X03;
        var results = new List<object>();
        foreach (var part in (request.X02 ?? string.Empty).Split(','))
        {
            var id = Id(part) ?? 0;
            var no = await db.Set<GlJournalHeader>().Where(h => h.JournalHeaderId == id).Select(h => h.JournalNo).FirstOrDefaultAsync(ct);
            try
            {
                switch (act)
                {
                    case "SUBMIT": await api.SubmitJournalAsync(id, txt, ct); break;
                    case "APPROVE": await api.ApproveJournalAsync(id, txt, ct); break;
                    case "POST": await api.PostJournalAsync(id, ct); break;
                    case "CANCEL": await api.CancelJournalAsync(id, txt, ct); break;
                    case "REJECT": await api.RejectJournalAsync(id, txt, ct); break;
                    default: throw new ApexProcessException("Unknown action " + act);
                }

                results.Add(new { id, no = no ?? id.ToString(CultureInfo.InvariantCulture), ok = true });
            }
            catch (Exception ex) when (Handled(ex))
            {
                results.Add(new { id, no = no ?? id.ToString(CultureInfo.InvariantCulture), ok = false, message = GlPostingApi.CleanMessage(ex.Message) });
            }
        }

        return new { ok = true, results };
    }, ct);
}
