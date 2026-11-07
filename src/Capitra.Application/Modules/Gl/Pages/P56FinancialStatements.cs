using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>FS_INIT: the ledgers, the chosen ledger's periods, currencies, segments, segment values and accounts (x01 ledger id).</summary>
public sealed class P56Init(ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 120;

    public int Page => 56;

    public string Name => "FS_INIT";

    public string Permission => Permissions.Gl.JournalView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var active = db.Set<GlLedger>().Where(l => (l.LedgerStatus ?? "ACTIVE") == "ACTIVE");
        var led = Text(request.X01) is { } x ? (long)decimal.Parse(x, CultureInfo.InvariantCulture)
            : await active.OrderBy(l => l.IsPrimaryLedger ? 0 : 1).ThenBy(l => l.LedgerCode).Select(l => (long?)l.LedgerId).FirstOrDefaultAsync(ct);
        var ledger = await db.Set<GlLedger>().Where(l => l.LedgerId == led).Select(l => new { l.CoaId, l.LedgerCurrencyCode }).FirstOrDefaultAsync(ct);
        if (ledger is null)
            return new { ok = false, message = "ORA-01403: no data found" };

        // the period that covers today, else the latest one that has started
        var today = GlTime.Today(clock);
        var periods = from lp in db.Set<GlLedgerPeriod>()
                      join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                      where lp.LedgerId == led
                      select new { lp.LedgerPeriodId, p.PeriodCode, lp.PeriodStatus, p.StartDate, p.EndDate, p.PeriodNo, p.IsAdjustmentPeriod };
        var current = await periods.Where(p => !p.IsAdjustmentPeriod && p.StartDate <= today && p.EndDate >= today)
            .Select(p => (long?)p.LedgerPeriodId).FirstOrDefaultAsync(ct)
            ?? await periods.Where(p => p.StartDate <= today).OrderByDescending(p => p.StartDate).Select(p => (long?)p.LedgerPeriodId).FirstOrDefaultAsync(ct);

        var segments = await db.Set<GlCoaSegment>().Where(s => s.CoaId == ledger.CoaId).OrderBy(s => s.SegmentNum).ToListAsync(ct);
        var values = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var g in segments)
        {
            values[g.SegmentCode] = (await (from v in db.Set<GlCoaSegmentValue>()
                                            join p in db.Set<GlCoaSegmentValue>() on v.ParentValueId equals p.SegmentValueId into ps
                                            from p in ps.DefaultIfEmpty()
                                            where v.CoaSegmentId == g.CoaSegmentId
                                            orderby v.ValueCode
                                            select new { v.ValueCode, v.ValueNameEn, v.IsPostingAllowed, Parent = (string?)p.ValueCode }).Take(1000).ToListAsync(ct))
                .Select(v => new object?[] { v.ValueCode, v.ValueNameEn, Yn(v.IsPostingAllowed), v.Parent }).ToList();
        }

        var others = await db.Set<GlAccountBalance>().Where(b => b.LedgerId == led && b.CurrencyCode != ledger.LedgerCurrencyCode)
            .Select(b => b.CurrencyCode).Distinct().OrderBy(c => c).ToListAsync(ct);
        return new
        {
            ok = true,
            ledgerId = led,
            currency = ledger.LedgerCurrencyCode,
            lp = current,
            ledgers = (await active.OrderBy(l => l.LedgerCode).Select(l => new { l.LedgerId, l.LedgerCode, l.LedgerNameEn, l.LedgerCurrencyCode }).ToListAsync(ct))
                .Select(l => new object[] { l.LedgerId, l.LedgerCode, l.LedgerNameEn, l.LedgerCurrencyCode }),
            periods = (await periods.OrderByDescending(p => p.StartDate).ThenByDescending(p => p.PeriodNo).ToListAsync(ct))
                .Select(p => new object?[] { p.LedgerPeriodId, p.PeriodCode, p.PeriodStatus, Ymd(p.StartDate), Ymd(p.EndDate) }),
            currencies = others.Prepend(ledger.LedgerCurrencyCode),
            segments = segments.Select(s => new object[] { s.SegmentNum, s.SegmentCode, s.SegmentNameEn, s.SegmentType }),
            values,
            accounts = (await db.Set<GlAccount>().Where(a => a.CoaId == ledger.CoaId).OrderBy(a => a.AccountCode).ToListAsync(ct))
                .Select(a => new object?[] { a.AccountId, a.AccountCode, a.AccountNameEn, a.ParentAccountId, Yn(a.IsSummary), a.AccountType, a.FinancialStatementClass, a.CashFlowClass }),
        };
    }
}

/// <summary>FS_RUN: gl_financial_statement_pkg.statement_rc for the page's choices (x01 JSON), up to 20,000 rows.</summary>
public sealed partial class P56Run(ICapitraDbContext db) : IApexProcess
{
    public int App => 120;

    public int Page => 56;

    public string Name => "FS_RUN";

    public string Permission => Permissions.Gl.JournalView;

    [GeneratedRegex("^[A-Z0-9_.-]{1,60}$")]
    private static partial Regex ValueCode();

    private static string? Norm(string? value) => value?.Trim().ToUpperInvariant() is { Length: > 0 } v ? v : null;

    private sealed record Line(
        GlAccountBalance B, long ComboId, string Concat, long AccountId, string AccountCode, string AccountNameEn, string? AccountNameAr,
        string AccountType, string NormalBalance, string? CashFlowClass, string FsClass);

    private sealed record Out(
        string Section, long? AccountId, string AccountCode, string AccountName, string AccountType, string? GroupCode, string? GroupName,
        long? ComboId, string? Concat, string Currency, decimal Odr, decimal Ocr, decimal Pdr, decimal Pcr, decimal Cdr, decimal Ccr, decimal Amount);

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
            var rows = await StatementAsync(doc.RootElement, ct);
            return new
            {
                ok = true,
                rows = rows.Take(20000).Select(r => new object?[]
                {
                    r.Section, r.AccountId, r.AccountCode, r.AccountName, r.AccountType, r.GroupCode, r.GroupName, r.ComboId, r.Concat, r.Currency,
                    r.Odr, r.Ocr, r.Pdr, r.Pcr, r.Cdr, r.Ccr, r.Amount,
                }),
                more = rows.Count > 20000,
            };
        }
        catch (ApexProcessException ex)
        {
            return new { ok = false, message = ex.Message };
        }
    }

    private static string? Get(JsonElement o, string key) =>
        o.TryGetProperty(key, out var v) ? v.ValueKind switch
        {
            JsonValueKind.String => v.GetString() is { Length: > 0 } s ? s : null,
            JsonValueKind.Number => v.GetRawText(),
            _ => null,
        } : null;

    private static long? Num(JsonElement o, string key) => Get(o, key) is { } v ? (long)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    private async Task<long> CoaOfLedgerAsync(long? ledgerId, CancellationToken ct) =>
        await db.Set<GlLedger>().Where(l => l.LedgerId == ledgerId).Select(l => l.CoaId).FirstOrDefaultAsync(ct)
        ?? throw new ApexProcessException("ORA-01403: no data found");

    /// <summary>segment_filter_sql: SEGMENT=V1,V2;SEGMENT=V ... keeps combinations whose segment holds a chosen value or one below it.</summary>
    private async Task<List<(int Num, HashSet<string> Values)>> SegmentFilterAsync(long? ledgerId, string? filter, CancellationToken ct)
    {
        var parts = new List<(int, HashSet<string>)>();
        if (string.IsNullOrWhiteSpace(filter))
            return parts;
        var coa = await CoaOfLedgerAsync(ledgerId, ct);
        foreach (var raw in filter.Split(';').Select(p => p.Trim()).Where(p => p.Length > 0))
        {
            var eq = raw.IndexOf('=', StringComparison.Ordinal);
            var seg = eq > 0 ? Norm(raw[..eq]) : null;
            if (eq < 0 || seg is null)
                throw new ApexProcessException($"ORA-20601: Segment filter \"{raw}\" must look like SEGMENT=VALUE.");
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), as the PL/SQL compared.
            var segment = await db.Set<GlCoaSegment>().Where(s => s.CoaId == coa && s.SegmentCode.ToUpper() == seg)
                .Select(s => new { s.SegmentNum, s.CoaSegmentId }).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException($"ORA-20602: Segment {seg} is not part of this ledger's account structure.");
#pragma warning restore CA1862
            var chosen = new List<string>();
            foreach (var v in raw[(eq + 1)..].Split(',').Select(Norm).Where(v => v is not null))
            {
                if (!ValueCode().IsMatch(v!))
                    throw new ApexProcessException($"ORA-20603: Segment value \"{v}\" is not valid.");
                chosen.Add(v!);
            }

            if (chosen.Count == 0)
                continue;

            // the chosen values and every value below them in the segment hierarchy
            var all = await db.Set<GlCoaSegmentValue>().Where(v => v.CoaSegmentId == segment.CoaSegmentId)
                .Select(v => new { v.SegmentValueId, v.ParentValueId, v.ValueCode }).ToListAsync(ct);
            var children = all.Where(v => v.ParentValueId is not null).ToLookup(v => v.ParentValueId!.Value);
            var seen = new HashSet<long>();
            var queue = new Queue<long>(all.Where(v => chosen.Contains(v.ValueCode.ToUpperInvariant())).Select(v => v.SegmentValueId));
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!seen.Add(id))
                    continue;
                foreach (var c in children[id])
                    queue.Enqueue(c.SegmentValueId);
            }

            parts.Add((segment.SegmentNum, all.Where(v => seen.Contains(v.SegmentValueId)).Select(v => v.ValueCode.ToUpperInvariant()).ToHashSet()));
        }

        return parts;
    }

    private async Task<List<Out>> StatementAsync(JsonElement o, CancellationToken ct)
    {
        var st = Norm(Get(o, "st"));
        if (st is not ("TB" or "BS" or "IS" or "CF"))
            throw new ApexProcessException("ORA-20604: Statement must be TB, BS, IS or CF.");
        var led = Num(o, "led");
        var lp = Num(o, "lp");
        var cur = Get(o, "cur");
        var ytd = Norm(Get(o, "mode") ?? "PTD") == "YTD";
        var combo = Norm(Get(o, "level") ?? "ACCOUNT") == "COMBINATION";
        var acc = Num(o, "acc");
        var from = Get(o, "from");
        var to = Get(o, "to");
        var groupCode = Norm(Get(o, "group"));

        int? gnum = null;
        long? gseg = null;
        if (!string.IsNullOrWhiteSpace(Get(o, "group")))
        {
            var coa = await CoaOfLedgerAsync(led, ct);
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), as the PL/SQL compared.
            var g = await db.Set<GlCoaSegment>().Where(s => s.CoaId == coa && s.SegmentCode.ToUpper() == groupCode)
                .Select(s => new { s.SegmentNum, s.CoaSegmentId }).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException($"ORA-20605: Segment {groupCode} is not part of this ledger's account structure.");
#pragma warning restore CA1862
            (gnum, gseg) = (g.SegmentNum, g.CoaSegmentId);
        }

        var filter = await SegmentFilterAsync(led, Get(o, "filter"), ct);
        var curUpper = cur?.ToUpperInvariant();

        var lines = await (from b in db.Set<GlAccountBalance>()
                           join c in db.Set<GlAccountCombination>() on b.AccountCombinationId equals c.AccountCombinationId
                           join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
                           where b.LedgerId == led && b.LedgerPeriodId == lp && (cur == null || b.CurrencyCode == curUpper)
                                 && (acc == null || a.AccountId == acc)
                           select new Line(b, c.AccountCombinationId, c.ConcatenatedAccount, a.AccountId, a.AccountCode, a.AccountNameEn, a.AccountNameAr,
                               a.AccountType, a.NormalBalance, a.CashFlowClass, a.FinancialStatementClass)).AsNoTracking().ToListAsync(ct);
        lines = lines.Where(l => (from == null || string.CompareOrdinal(l.AccountCode, from) >= 0) && (to == null || string.CompareOrdinal(l.AccountCode, to) <= 0)).ToList();

        // the segment values of the combinations, for the filter and the grouping
        var comboIds = lines.Select(l => l.ComboId).Distinct().ToList();
        var segs = (await db.Set<GlAccountCombinationSegment>().Where(s => comboIds.Contains(s.AccountCombinationId))
                .Select(s => new { s.AccountCombinationId, s.SegmentNum, s.SegmentValueCode }).ToListAsync(ct))
            .GroupBy(s => s.AccountCombinationId)
            .ToDictionary(g => g.Key, g => g.GroupBy(s => s.SegmentNum).ToDictionary(x => x.Key, x => x.First().SegmentValueCode));
        lines = lines.Where(l => filter.All(f => segs.TryGetValue(l.ComboId, out var m) && m.TryGetValue(f.Num, out var v) && f.Values.Contains(v.ToUpperInvariant()))).ToList();

        var groupNames = gseg is null ? new Dictionary<string, string>()
            : (await db.Set<GlCoaSegmentValue>().Where(v => v.CoaSegmentId == gseg).Select(v => new { v.ValueCode, v.ValueNameEn }).ToListAsync(ct))
                .GroupBy(v => v.ValueCode.ToUpperInvariant()).ToDictionary(g => g.Key, g => g.Max(v => v.ValueNameEn)!);
        string? GroupOf(Line l) => gnum is null ? null : segs.TryGetValue(l.ComboId, out var m) && m.TryGetValue(gnum.Value, out var v) ? v : "-";
        string? GroupName(string? code) => code is not null && groupNames.TryGetValue(code.ToUpperInvariant(), out var n) ? n : null;

        static bool Has(string? text, string part) => (text ?? string.Empty).ToUpperInvariant().Contains(part, StringComparison.Ordinal);
        bool income(Line l) => Has(l.AccountType, "REVENUE") || Has(l.AccountType, "INCOME");

        // which movement the statement reads, and how it is signed and grouped
        Func<Line, string> section;
        Func<Line, decimal> amount;
        Func<Line, bool> where;
        switch (st)
        {
            case "TB":
                section = _ => "TRIAL_BALANCE";
                amount = l => l.B.ClosingDr - l.B.ClosingCr;
                where = _ => true;
                break;
            case "BS":
                section = l => Has(l.AccountType, "ASSET") ? "ASSETS" : Has(l.AccountType, "LIAB") ? "LIABILITIES" : "EQUITY";
                amount = l => Has(l.AccountType, "ASSET") ? l.B.ClosingDr - l.B.ClosingCr : l.B.ClosingCr - l.B.ClosingDr;
                where = l => l.FsClass == "BALANCE_SHEET";
                break;
            case "IS":
                section = l => income(l) ? "REVENUE" : "EXPENSES";
                amount = l =>
                {
                    var (dr, cr) = ytd ? (l.B.ClosingDr, l.B.ClosingCr) : (l.B.PeriodDr, l.B.PeriodCr);
                    return income(l) ? cr - dr : dr - cr;
                };
                where = l => l.FsClass == "INCOME_STATEMENT";
                break;
            default:
                // the period's cash movement: cash accounts show their own change, every other account its effect on cash
                section = l => l.CashFlowClass ?? "NONE";
                amount = l => l.CashFlowClass == "CASH_EQUIVALENT" ? l.B.PeriodDr - l.B.PeriodCr : l.B.PeriodCr - l.B.PeriodDr;
                where = l => (l.CashFlowClass ?? "NONE") != "NONE";
                break;
        }

        var result = lines.Where(where)
            .GroupBy(l => (Section: section(l), l.AccountId, l.AccountCode, l.AccountNameEn, l.AccountType, l.B.CurrencyCode, Group: GroupOf(l),
                Combo: combo ? l.ComboId : (long?)null, Concat: combo ? l.Concat : null))
            .Select(g => new Out(g.Key.Section, g.Key.AccountId, g.Key.AccountCode, g.Key.AccountNameEn, g.Key.AccountType, g.Key.Group, GroupName(g.Key.Group),
                g.Key.Combo, g.Key.Concat, g.Key.CurrencyCode,
                g.Sum(l => l.B.OpeningDr), g.Sum(l => l.B.OpeningCr), g.Sum(l => l.B.PeriodDr), g.Sum(l => l.B.PeriodCr),
                g.Sum(l => l.B.ClosingDr), g.Sum(l => l.B.ClosingCr), g.Sum(amount)))
            .ToList();

        // balance sheet: the year's result that is not closed to retained earnings yet, as an equity line
        if (st == "BS")
        {
            result.AddRange(lines.Where(l => l.FsClass == "INCOME_STATEMENT")
                .GroupBy(l => (l.B.CurrencyCode, Group: GroupOf(l)))
                .Where(g => g.Sum(l => l.B.ClosingCr - l.B.ClosingDr) != 0)
                .Select(g => new Out("EQUITY", null, "~RESULT", "Current year result", "EQUITY", g.Key.Group, GroupName(g.Key.Group), null, null, g.Key.CurrencyCode,
                    0, 0, 0, 0, g.Sum(l => l.B.ClosingDr), g.Sum(l => l.B.ClosingCr), g.Sum(l => l.B.ClosingCr - l.B.ClosingDr))));
        }

        return result.OrderBy(r => r.Section, StringComparer.Ordinal).ThenBy(r => r.AccountCode, StringComparer.Ordinal)
            .ThenBy(r => r.GroupCode ?? "￿", StringComparer.Ordinal).ToList();
    }
}
