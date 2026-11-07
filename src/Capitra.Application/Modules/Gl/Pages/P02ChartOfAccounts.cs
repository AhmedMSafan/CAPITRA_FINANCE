using System.Globalization;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>App 120 page 2 "Chart of Accounts": the three on-demand processes, ported from PL/SQL.</summary>
public abstract class P02Process : IApexProcess
{
    public int App => 120;

    public int Page => 2;

    public abstract string Name { get; }

    public virtual string Permission => Permissions.Gl.SetupView;

    public abstract Task<object> RunAsync(ApexRequest request, CancellationToken ct);

    /// <summary>The account and every account below it (start with ... connect by prior account_id = parent_account_id).</summary>
    protected static async Task<HashSet<long>> SubtreeAsync(ICapitraDbContext db, long accountId, long coaId, CancellationToken ct)
    {
        var parents = await db.Set<GlAccount>().AsNoTracking().Where(a => a.CoaId == coaId)
            .Select(a => new { a.AccountId, a.ParentAccountId }).ToListAsync(ct);
        var kids = parents.Where(a => a.ParentAccountId is not null).ToLookup(a => a.ParentAccountId!.Value, a => a.AccountId);
        var result = new HashSet<long> { accountId };
        var queue = new Queue<long>([accountId]);
        while (queue.Count > 0)
        {
            foreach (var kid in kids[queue.Dequeue()])
            {
                if (result.Add(kid))
                    queue.Enqueue(kid);
            }
        }

        return result;
    }

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>to_char(timestamp, 'DD Mon YYYY HH24:MI') in Cairo time.</summary>
    protected static string? Stamp(DateTime? utc) =>
        utc is null ? null : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), Cairo)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);
}

/// <summary>GL_TREE_LOAD: structures, segments, every account of the structure, usual classifications and pick lists.</summary>
public sealed class P02TreeLoad(ICapitraDbContext db) : P02Process
{
    public override string Name => "GL_TREE_LOAD";

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        long? coa = Text(request.X01) is { } x ? long.Parse(x, CultureInfo.InvariantCulture) : null;
        coa ??= await db.Set<GlAccount>().MinAsync(a => (long?)a.CoaId, ct);
        coa ??= await db.Set<GlCoaStructure>().MinAsync(s => (long?)s.CoaId, ct);

        var accounts = await (from a in db.Set<GlAccount>().AsNoTracking()
                              join sv in db.Set<GlCoaSegmentValue>() on a.AccountSegmentValueId equals sv.SegmentValueId into svs
                              from sv in svs.DefaultIfEmpty()
                              where a.CoaId == coa
                              orderby a.SortOrder, a.AccountCode
                              select new
                              {
                                  a,
                                  segCode = sv.ValueCode,
                                  combos = db.Set<GlAccountCombination>().Count(c => c.AccountId == a.AccountId),
                                  combosOn = db.Set<GlAccountCombination>().Count(c => c.AccountId == a.AccountId && c.IsEnabled),
                              }).ToListAsync(ct);

        var all = db.Set<GlAccount>().AsNoTracking();
        return new
        {
            ok = true,
            coaId = coa,
            structures = await db.Set<GlCoaStructure>().AsNoTracking().OrderBy(s => s.CoaCode).Select(s => new
            {
                id = s.CoaId, code = s.CoaCode, nameEn = s.CoaNameEn, delimiter = s.DelimiterChar, segCount = s.SegmentCount,
                accountSeg = s.AccountSegmentNum,
            }).ToListAsync(ct),
            segments = await db.Set<GlCoaSegment>().AsNoTracking().Where(s => s.CoaId == coa).OrderBy(s => s.SegmentNum).Select(s => new
            {
                num = s.SegmentNum, code = s.SegmentCode, nameEn = s.SegmentNameEn, type = s.SegmentType, minLen = s.MinLength,
                maxLen = s.MaxLength, format = s.ValueFormat, upper = s.IsUppercase ? "Y" : "N",
            }).ToListAsync(ct),
            accounts = accounts.Select(r => new
            {
                id = r.a.AccountId, code = r.a.AccountCode, name = r.a.AccountNameEn, nameAr = r.a.AccountNameAr,
                description = r.a.Description, parentId = r.a.ParentAccountId, type = r.a.AccountType, normalBalance = r.a.NormalBalance,
                fsClass = r.a.FinancialStatementClass, cashFlowClass = r.a.CashFlowClass, summary = Yn(r.a.IsSummary),
                posting = Yn(r.a.IsPostingAllowed), control = Yn(r.a.IsControlAccount), recon = Yn(r.a.IsReconciliation),
                reqBranch = Yn(r.a.IsBranchRequired), reqCc = Yn(r.a.IsCostCenterRequired), reqBl = Yn(r.a.IsBusinessLineRequired),
                manual = Yn(r.a.IsManualJournalAllowed), subledger = Yn(r.a.IsSubledgerPostingAllowed), reval = Yn(r.a.IsCurrencyRevaluation),
                retained = Yn(r.a.IsRetainedEarnings), active = Yn(r.a.IsActive), start = Ymd(r.a.EffectiveStartDate),
                end = Ymd(r.a.EffectiveEndDate), sortOrder = r.a.SortOrder, segCode = r.segCode, combos = r.combos, combosOn = r.combosOn,
                createdBy = r.a.CreatedBy, createdDt = Stamp(r.a.CreatedAt), updatedBy = r.a.UpdatedBy, updatedDt = Stamp(r.a.UpdatedAt),
            }),
            // The usual classification of each account type in this structure (most frequent combination).
            types = accounts.GroupBy(r => new { r.a.AccountType, r.a.NormalBalance, r.a.FinancialStatementClass, r.a.CashFlowClass })
                .Select(g => new { g.Key, n = g.Count() })
                .GroupBy(g => g.Key.AccountType)
                .Select(t => t.OrderByDescending(g => g.n).First().Key)
                .OrderBy(k => k.AccountType, StringComparer.Ordinal)
                .Select(k => new { type = k.AccountType, normalBalance = k.NormalBalance, fsClass = k.FinancialStatementClass, cashFlowClass = k.CashFlowClass }),
            lists = new
            {
                types = await all.Select(a => a.AccountType).Distinct().OrderBy(s => s).ToListAsync(ct),
                balances = await all.Select(a => a.NormalBalance).Distinct().OrderBy(s => s).ToListAsync(ct),
                fsClasses = await all.Select(a => a.FinancialStatementClass).Distinct().OrderBy(s => s).ToListAsync(ct),
                cfClasses = await all.Select(a => a.CashFlowClass).Distinct().OrderBy(s => s).ToListAsync(ct),
            },
        };
    }
}

/// <summary>GL_ACC_BAL: ledgers, periods and the combinations (with their balances) under an account.</summary>
public sealed class P02AccountBalances(ICapitraDbContext db, IClock clock) : P02Process
{
    public override string Name => "GL_ACC_BAL";

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var acc = long.Parse(request.X01 ?? string.Empty, CultureInfo.InvariantCulture);
        long? led = Text(request.X02) is { } l ? long.Parse(l, CultureInfo.InvariantCulture) : null;
        long? lp = Text(request.X03) is { } p ? long.Parse(p, CultureInfo.InvariantCulture) : null;
        var coa = await db.Set<GlAccount>().Where(a => a.AccountId == acc).Select(a => a.CoaId).SingleAsync(ct);
        var today = DateOnly.FromDateTime(clock.UtcNow);

        led ??= await db.Set<GlLedger>().Where(x => x.CoaId == coa)
            .OrderBy(x => x.IsPrimaryLedger ? 0 : 1).ThenBy(x => x.LedgerCode).Select(x => (long?)x.LedgerId).FirstOrDefaultAsync(ct);

        string? currency = null;
        if (led is not null)
        {
            currency = await db.Set<GlLedger>().Where(x => x.LedgerId == led).Select(x => x.LedgerCurrencyCode).SingleAsync(ct);
            var periods = from x in db.Set<GlLedgerPeriod>()
                          join ap in db.Set<GlAccountingPeriod>() on x.PeriodId equals ap.PeriodId
                          where x.LedgerId == led
                          select new { x.LedgerPeriodId, ap };
            lp ??= await periods.Where(r => !r.ap.IsAdjustmentPeriod && r.ap.StartDate <= today && r.ap.EndDate >= today)
                .Select(r => (long?)r.LedgerPeriodId).FirstOrDefaultAsync(ct);
            lp ??= await periods.Where(r => r.ap.StartDate <= today).OrderByDescending(r => r.ap.StartDate).ThenByDescending(r => r.LedgerPeriodId)
                .Select(r => (long?)r.LedgerPeriodId).FirstOrDefaultAsync(ct);
        }

        var subtree = await SubtreeAsync(db, acc, coa, ct);
        var combos = await (from c in db.Set<GlAccountCombination>().AsNoTracking()
                            join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
                            where subtree.Contains(c.AccountId)
                            orderby c.ConcatenatedAccount
                            select new { c.AccountCombinationId, c.ConcatenatedAccount, c.IsEnabled, a.AccountCode, a.AccountNameEn })
            .Take(501).ToListAsync(ct);
        var shown = combos.Take(500).Select(c => c.AccountCombinationId).ToList();
        var segs = await (from cs in db.Set<GlAccountCombinationSegment>().AsNoTracking()
                          join v in db.Set<GlCoaSegmentValue>() on cs.SegmentValueId equals v.SegmentValueId into vs
                          from v in vs.DefaultIfEmpty()
                          where shown.Contains(cs.AccountCombinationId)
                          orderby cs.SegmentNum
                          select new { cs.AccountCombinationId, cs.SegmentNum, cs.SegmentValueCode, v.ValueNameEn }).ToListAsync(ct);
        var bals = await db.Set<GlAccountBalance>().AsNoTracking()
            .Where(b => shown.Contains(b.AccountCombinationId) && b.LedgerPeriodId == lp).ToListAsync(ct);

        return new
        {
            ok = true,
            ledgerId = led,
            lpId = lp,
            ledgerCurrency = currency,
            accounts = subtree.Count,
            accountSeg = await db.Set<GlCoaStructure>().Where(s => s.CoaId == coa).Select(s => s.AccountSegmentNum).SingleAsync(ct),
            ledgers = await db.Set<GlLedger>().Where(x => x.CoaId == coa).OrderBy(x => x.LedgerCode)
                .Select(x => new { id = x.LedgerId, code = x.LedgerCode, currency = x.LedgerCurrencyCode }).ToListAsync(ct),
            periods = await (from x in db.Set<GlLedgerPeriod>()
                             join ap in db.Set<GlAccountingPeriod>() on x.PeriodId equals ap.PeriodId
                             where x.LedgerId == led
                             orderby ap.StartDate descending, ap.PeriodNo descending
                             select new { lp = x.LedgerPeriodId, code = ap.PeriodCode, status = x.PeriodStatus }).ToListAsync(ct),
            segments = await db.Set<GlCoaSegment>().Where(s => s.CoaId == coa).OrderBy(s => s.SegmentNum)
                .Select(s => new { num = s.SegmentNum, name = s.SegmentNameEn, type = s.SegmentType }).ToListAsync(ct),
            combos = combos.Take(500).Select(c => new
            {
                id = c.AccountCombinationId, concat = c.ConcatenatedAccount, enabled = Yn(c.IsEnabled), accCode = c.AccountCode,
                accName = c.AccountNameEn,
                segs = segs.Where(s => s.AccountCombinationId == c.AccountCombinationId)
                    .Select(s => new { num = s.SegmentNum, code = s.SegmentValueCode, name = s.ValueNameEn }),
                bals = bals.Where(b => b.AccountCombinationId == c.AccountCombinationId).Select(b => new
                {
                    cur = b.CurrencyCode, odr = b.OpeningDr, ocr = b.OpeningCr, pdr = b.PeriodDr, pcr = b.PeriodCr, cdr = b.ClosingDr,
                    ccr = b.ClosingCr,
                }),
            }),
            truncated = combos.Count > 500,
        };
    }
}

/// <summary>GL_SAVE_ACCOUNT: checks the code against the account segment, saves the segment value, then the account.</summary>
public sealed partial class P02SaveAccount(ICapitraDbContext db, GlCoaSetup setup, IClock clock) : P02Process
{
    public override string Name => "GL_SAVE_ACCOUNT";

    public override string Permission => Permissions.Gl.SetupEdit;

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex Digits();

    [GeneratedRegex("^[A-Z0-9]+$")]
    private static partial Regex LettersAndDigits();

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var id = await SaveAsync(new ApexJson(request.X01), ct);
            await tx.CommitAsync(ct);
            return new { ok = true, id };
        }
        catch (Exception ex) when (ex is ApexProcessException or Common.Exceptions.ConflictException or FormatException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = ex.Message };
        }
    }

    private async Task<long> SaveAsync(ApexJson j, CancellationToken ct)
    {
        string Yn(string key) => j.Str(key) == "Y" ? "Y" : "N";

        var id = j.Key("id");
        var code = j.Str("code")?.ToUpperInvariant();
        var parent = j.Key("parentId");
        var coa = j.Key("coaId");
        if (code is null || j.Str("name") is null || j.Str("type") is null || j.Str("normalBalance") is null
            || j.Str("fsClass") is null || j.Str("cashFlowClass") is null)
            throw new ApexProcessException("Code, name, account type, normal balance, financial statement class and cash flow class are required.");

        GlAccount? old = null;
        if (id is not null)
        {
            old = await db.Set<GlAccount>().AsNoTracking().SingleAsync(a => a.AccountId == id, ct);
            coa = old.CoaId;
            if (old.AccountCode != code)
                throw new ApexProcessException("The account code cannot be changed: it is the value of the account segment.");
        }

        string? parentCode = null, parentSegValue = null;
        if (parent is not null)
        {
            var p = await (from a in db.Set<GlAccount>()
                           join pv in db.Set<GlCoaSegmentValue>() on a.AccountSegmentValueId equals pv.SegmentValueId into pvs
                           from pv in pvs.DefaultIfEmpty()
                           where a.AccountId == parent
                           select new { a.CoaId, a.AccountCode, pv.ValueCode }).SingleAsync(ct);
            if (p.CoaId != coa)
                throw new ApexProcessException("The parent account belongs to another account structure.");
            if (id is not null && (await SubtreeAsync(db, id.Value, p.CoaId, ct)).Contains(parent.Value))
                throw new ApexProcessException("The selected parent is below this account. This would create a circular hierarchy.");
            (parentCode, parentSegValue) = (p.AccountCode, p.ValueCode);
        }

        // The account segment of the structure decides what an account code may look like.
        var seg = await (from st in db.Set<GlCoaStructure>()
                         join sg in db.Set<GlCoaSegment>() on new { st.CoaId, Num = st.AccountSegmentNum } equals new { sg.CoaId, Num = sg.SegmentNum }
                         where st.CoaId == coa
                         select new { st.CoaCode, sg.SegmentCode, sg.CoaSegmentId, sg.MinLength, sg.MaxLength, sg.ValueFormat, sg.SegmentType })
            .SingleOrDefaultAsync(ct)
            ?? throw new ApexProcessException("The account structure has no account segment. Define it on the Account segments page first.");
        if (seg.SegmentType != "ACCOUNT")
            throw new ApexProcessException($"The structure's account segment is {seg.SegmentCode} (type {seg.SegmentType}), not an ACCOUNT segment. Fix it on the Account segments page (Edit structure) first.");
        if (code.Length < seg.MinLength || code.Length > seg.MaxLength)
            throw new ApexProcessException($"The account code must be {seg.MinLength}{(seg.MaxLength != seg.MinLength ? " to " + seg.MaxLength : "")} characters (account segment {seg.SegmentCode}).");
        if (seg.ValueFormat == "NUMERIC" && !Digits().IsMatch(code))
            throw new ApexProcessException($"The account segment {seg.SegmentCode} is numeric: use digits only.");
        if (seg.ValueFormat == "ALPHANUMERIC" && !LettersAndDigits().IsMatch(code))
            throw new ApexProcessException($"The account segment {seg.SegmentCode} accepts letters and digits only.");
        if (id is null && await db.Set<GlAccount>().AnyAsync(a => a.CoaId == coa && a.AccountCode == code, ct))
            throw new ApexProcessException($"Account {code} already exists.");

        var summary = Yn("summary");
        var posting = summary == "Y" ? "N" : Yn("posting");
        var start = j.Date("start") ?? old?.EffectiveStartDate ?? DateOnly.FromDateTime(clock.UtcNow);
        var sort = old?.SortOrder ?? (await db.Set<GlAccount>().Where(a => a.CoaId == coa && a.ParentAccountId == parent)
            .MaxAsync(a => (long?)a.SortOrder, ct) ?? 0) + 10;

        // 1. The account segment value, kept in step with the account; an existing value keeps its own references.
        var sv = await (from v in db.Set<GlCoaSegmentValue>().AsNoTracking()
                        join le in db.Set<FinLegalEntity>() on v.ReferenceLegalEntityId equals le.LegalEntityId into les
                        from le in les.DefaultIfEmpty()
                        where v.CoaSegmentId == seg.CoaSegmentId && v.ValueCode == code
                        select new { v.SortOrder, le.LegalEntityCode, v.ReferenceBusinessLineCode, v.ReferenceBranchId, v.ReferenceCostCenterId })
            .SingleOrDefaultAsync(ct);
        var svId = await setup.SaveSegmentValueAsync(new GlCoaSetup.SegmentValueInput(
            seg.CoaCode, seg.SegmentCode, code, j.Str("name"), j.Str("nameAr"), j.Str("description"), parentSegValue, summary, posting,
            Yn("active"), start, j.Date("end"), sv?.SortOrder ?? sort, sv?.LegalEntityCode, null, null, sv?.ReferenceBusinessLineCode), ct);
        if (sv is not null && (sv.ReferenceBranchId is not null || sv.ReferenceCostCenterId is not null))
        {
            var row = await db.Set<GlCoaSegmentValue>().SingleAsync(v => v.SegmentValueId == svId, ct);
            row.ReferenceBranchId ??= sv.ReferenceBranchId;
            row.ReferenceCostCenterId ??= sv.ReferenceCostCenterId;
            await db.SaveChangesAsync(ct);
        }

        // 2. The account, linked to that segment value.
        return await setup.SaveAccountAsync(new GlCoaSetup.AccountInput(
            seg.CoaCode, code, j.Str("name"), j.Str("nameAr"), j.Str("description"), parentCode, code, j.Str("type"), j.Str("normalBalance"),
            j.Str("fsClass"), j.Str("cashFlowClass"), summary, posting, Yn("control"), Yn("recon"), Yn("reqBranch"), Yn("reqCc"), Yn("reqBl"),
            Yn("manual"), Yn("subledger"), Yn("reval"), Yn("retained"), Yn("active"), start, j.Date("end"), sort), ct);
    }
}
