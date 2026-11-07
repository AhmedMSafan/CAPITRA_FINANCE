using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>App 120 page 30 "Accounting Periods": the eight on-demand processes, ported from PL/SQL.</summary>
public abstract class P30Process : IApexProcess
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public int App => 120;

    public int Page => 30;

    public abstract string Name { get; }

    public abstract string Permission { get; }

    public abstract Task<object> RunAsync(ApexRequest request, CancellationToken ct);

    /// <summary>Runs a change in one transaction; an error rolls it back and comes back as {ok:false, message}, as the PL/SQL did.</summary>
    protected static async Task<object> Guard(ICapitraDbContext db, Func<Task<object>> body, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var result = await body();
            await tx.CommitAsync(ct);
            return result;
        }
        catch (Exception ex) when (ex is ApexProcessException or Common.Exceptions.ConflictException or FormatException or OverflowException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = ex.Message };
        }
    }

    protected static int? Whole(string? value) =>
        Text(value) is { } v ? (int)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    protected static long? Id(string? value) =>
        Text(value) is { } v ? (long)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    /// <summary>to_date(value, 'YYYY-MM-DD').</summary>
    protected static DateOnly? Day(string? value) =>
        Text(value) is { } v ? DateOnly.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture) : null;

    /// <summary>to_char(date, 'DD-Mon-YYYY').</summary>
    protected static string Dmy(DateOnly value) => value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

    protected static DateOnly CairoToday(IClock clock) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    /// <summary>to_char(timestamp, 'DD Mon YYYY HH24:MI') in Cairo time.</summary>
    protected static string? Stamp(DateTime? utc) =>
        utc is null ? null : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), Cairo)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

    /// <summary>The ledger's next regular period after this one, as {lp, code, status}, or null.</summary>
    protected static async Task<object?> NextAsync(ICapitraDbContext db, long ledgerPeriodId, CancellationToken ct) =>
        await (from lp in db.Set<GlLedgerPeriod>()
               join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
               join lp2 in db.Set<GlLedgerPeriod>() on lp.LedgerId equals lp2.LedgerId
               join p2 in db.Set<GlAccountingPeriod>() on lp2.PeriodId equals p2.PeriodId
               where lp.LedgerPeriodId == ledgerPeriodId && !p2.IsAdjustmentPeriod && p2.StartDate > p.EndDate
               orderby p2.StartDate
               select new { lp = lp2.LedgerPeriodId, code = p2.PeriodCode, status = lp2.PeriodStatus }).FirstOrDefaultAsync(ct);
}

/// <summary>PER_LOAD: the calendars, the chosen calendar's ledgers, fiscal years and periods (with each ledger's status), and pick lists.</summary>
public sealed class P30Load(ICapitraDbContext db, IClock clock) : P30Process
{
    public override string Name => "PER_LOAD";

    public override string Permission => Permissions.Gl.SetupView;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var calendars = db.Set<GlCalendar>().AsNoTracking();
        var cal = Id(request.X01);
        cal ??= await calendars.Where(c => c.IsActive).MinAsync(c => (long?)c.CalendarId, ct);
        cal ??= await calendars.MinAsync(c => (long?)c.CalendarId, ct);

        var periods = await db.Set<GlAccountingPeriod>().AsNoTracking().Where(p => p.CalendarId == cal)
            .OrderBy(p => p.FiscalYearId).ThenBy(p => p.PeriodNo).ThenBy(p => p.StartDate).ToListAsync(ct);
        var periodIds = periods.Select(p => p.PeriodId).ToList();
        var ledgerPeriods = (await (from lp in db.Set<GlLedgerPeriod>()
                                    join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
                                    where periodIds.Contains(lp.PeriodId)
                                    orderby l.LedgerCode
                                    select new { lp.PeriodId, lp.LedgerPeriodId, l.LedgerCode, lp.PeriodStatus }).ToListAsync(ct))
            .ToLookup(x => x.PeriodId);

        static List<string> Pick(IEnumerable<string?> values, string always) =>
            values.Where(v => v is not null).Select(v => v!).Append(always).Distinct().Order(StringComparer.Ordinal).ToList();

        return new
        {
            ok = true,
            calId = cal,
            today = Ymd(CairoToday(clock)),
            calendars = await calendars.OrderBy(c => c.CalendarCode).Select(c => new
            {
                id = c.CalendarId,
                code = c.CalendarCode,
                nameEn = c.CalendarNameEn,
                nameAr = c.CalendarNameAr,
                type = c.CalendarType,
                freq = c.PeriodFrequency,
                adj = c.IsAdjustmentPeriod ? "Y" : "N",
                active = c.IsActive ? "Y" : "N",
            }).ToListAsync(ct),
            ledgers = await db.Set<GlLedger>().AsNoTracking().Where(l => l.CalendarId == cal).OrderBy(l => l.LedgerCode)
                .Select(l => new { id = l.LedgerId, code = l.LedgerCode }).ToListAsync(ct),
            years = (await db.Set<GlFiscalYear>().AsNoTracking().Where(y => y.CalendarId == cal).OrderByDescending(y => y.FiscalYear)
                    .Select(y => new { y, n = db.Set<GlAccountingPeriod>().Count(p => p.FiscalYearId == y.FiscalYearId) }).ToListAsync(ct))
                .Select(r => new
                {
                    id = r.y.FiscalYearId,
                    year = r.y.FiscalYear,
                    name = r.y.YearName,
                    start = Ymd(r.y.StartDate),
                    end = Ymd(r.y.EndDate),
                    status = r.y.YearStatus,
                    periods = r.n,
                }),
            periods = periods.Select(p => new
            {
                id = p.PeriodId,
                yearId = p.FiscalYearId,
                no = p.PeriodNo,
                code = p.PeriodCode,
                nameEn = p.PeriodNameEn,
                nameAr = p.PeriodNameAr,
                start = Ymd(p.StartDate),
                end = Ymd(p.EndDate),
                qtr = p.QuarterNo,
                adj = Yn(p.IsAdjustmentPeriod),
                status = p.PeriodStatus,
                ledgers = ledgerPeriods[p.PeriodId].Select(x => new { lp = x.LedgerPeriodId, code = x.LedgerCode, status = x.PeriodStatus }),
            }),
            statuses = Pick(await db.Set<GlAccountingPeriod>().Select(p => p.PeriodStatus).Distinct().ToListAsync(ct), "NEVER_OPENED"),
            yearStatuses = Pick(await db.Set<GlFiscalYear>().Select(y => y.YearStatus).Distinct().ToListAsync(ct), "OPEN"),
            types = Pick(await calendars.Select(c => c.CalendarType).Distinct().ToListAsync(ct), "MONTHLY"),
            freqs = Pick(await calendars.Select(c => c.PeriodFrequency).Distinct().ToListAsync(ct), "MONTHLY"),
        };
    }
}

/// <summary>PER_GENERATE: a new fiscal year with its monthly periods, and when asked the ledger periods of every ledger on the calendar.</summary>
public sealed class P30Generate(ICapitraDbContext db, GlPeriodSetup setup) : P30Process
{
    public override string Name => "PER_GENERATE";

    public override string Permission => Permissions.Gl.SetupEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var code = Text(request.X01)?.ToUpperInvariant();
        var year = Whole(request.X02);
        var start = Day(request.X03);
        if (code is null || year is null || start is null)
            throw new ApexProcessException("Calendar, fiscal year and first day are required.");

        var end = start.Value.AddMonths(12).AddDays(-1);
        var cal = await db.Set<GlCalendar>().SingleOrDefaultAsync(c => c.CalendarCode == code, ct)
            ?? throw new ApexProcessException("Calendar not found: " + code);

        // never regenerate an existing year: that would reset its periods to NEVER_OPENED
        var count = await (from p in db.Set<GlAccountingPeriod>()
                           join fy in db.Set<GlFiscalYear>() on p.FiscalYearId equals fy.FiscalYearId
                           where fy.CalendarId == cal.CalendarId && fy.FiscalYear == year
                           select p).CountAsync(ct);
        if (count > 0)
            throw new ApexProcessException($"Fiscal year {year} already has {count} periods in calendar {code}. Edit its periods instead.");
        if (await db.Set<GlFiscalYear>().AnyAsync(y => y.CalendarId == cal.CalendarId && y.FiscalYear != year && y.StartDate <= end && y.EndDate >= start, ct))
            throw new ApexProcessException($"The dates {Dmy(start.Value)} to {Dmy(end)} overlap another fiscal year of calendar {code}.");

        await setup.GenerateMonthlyPeriodsAsync(code, year.Value, start.Value, Text(request.X04) ?? "N", Text(request.X05) ?? "OPEN", ct);
        var yearId = await db.Set<GlFiscalYear>().Where(y => y.CalendarId == cal.CalendarId && y.FiscalYear == year).Select(y => y.FiscalYearId).SingleAsync(ct);

        int ledgers = 0, created = 0;
        if (request.X06 == "Y")
        {
            var before = await db.Set<GlLedgerPeriod>().CountAsync(ct);
            foreach (var ledger in await db.Set<GlLedger>().Where(l => l.CalendarId == cal.CalendarId).OrderBy(l => l.LedgerCode).Select(l => l.LedgerCode).ToListAsync(ct))
            {
                await setup.SyncAllPeriodSupportAsync(ledger, year, ct);
                ledgers++;
            }

            created = await db.Set<GlLedgerPeriod>().CountAsync(ct) - before;
        }

        count = await db.Set<GlAccountingPeriod>().CountAsync(p => p.FiscalYearId == yearId, ct);
        return new
        {
            ok = true,
            yearId,
            message = $"Fiscal year {year} generated with {count} periods"
                + (ledgers > 0 ? $"; {created} ledger periods created for {ledgers} ledger(s)" : string.Empty) + ".",
        };
    }, ct);
}

/// <summary>PER_LP_ACTION: open, set to future entry, close, reopen or permanently close one ledger period.</summary>
public sealed class P30LedgerPeriodAction(ICapitraDbContext db, GlPeriodControl control) : P30Process
{
    public override string Name => "PER_LP_ACTION";

    public override string Permission => Permissions.Gl.PeriodControl;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var lp = Id(request.X01) ?? throw new ApexProcessException("No data found.");
        var reason = Text(request.X03);
        var source = new GlPeriodControl.ActionSource(120, 30, null);
        switch (request.X02)
        {
            case "OPEN":
                await control.OpenPeriodAsync(lp, reason, source, ct);
                break;
            case "SET_FUTURE":
                await control.SetFutureEntryAsync(lp, reason, source, ct);
                break;
            case "CLOSE":
                await control.ClosePeriodAsync(lp, reason, source, ct);
                break;
            case "REOPEN":
                if (reason is null)
                    throw new ApexProcessException("A reason is required to reopen a period.");
                await control.ReopenPeriodAsync(lp, reason, source, ct);
                break;
            case "PERMANENT":
                if (reason is null)
                    throw new ApexProcessException("A reason is required to close a period permanently.");
                await control.PermanentlyClosePeriodAsync(lp, reason, source, ct);
                break;
            default:
                throw new ApexProcessException("Unknown action.");
        }

        var status = await db.Set<GlLedgerPeriod>().Where(x => x.LedgerPeriodId == lp).Select(x => x.PeriodStatus).SingleAsync(ct);
        return new { ok = true, status, next = await NextAsync(db, lp, ct) };
    }, ct);
}

/// <summary>PER_LP_INFO: one ledger period's status, who opened and closed it, whether it can close, its checklist and history.</summary>
public sealed class P30LedgerPeriodInfo(ICapitraDbContext db, GlPeriodControl control) : P30Process
{
    public override string Name => "PER_LP_INFO";

    public override string Permission => Permissions.Gl.SetupView;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = Id(request.X01) ?? 0;
        var r = await (from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
                       join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
                       join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                       where lp.LedgerPeriodId == id
                       select new { lp, l.LedgerCode, p.PeriodCode }).SingleOrDefaultAsync(ct);

        var result = new Dictionary<string, object?>();
        if (r is null)
        {
            result["ok"] = false;
            result["message"] = "Ledger period not found.";
        }
        else
        {
            result["ok"] = true;
            result["ledger"] = r.LedgerCode;
            result["period"] = r.PeriodCode;
            result["status"] = r.lp.PeriodStatus;
            result["openedBy"] = r.lp.OpenedBy;
            result["openedDt"] = Stamp(r.lp.OpenedAt);
            result["closedBy"] = r.lp.ClosedBy;
            result["closedDt"] = Stamp(r.lp.ClosedAt);
            result["permBy"] = r.lp.PermanentlyClosedBy;
            result["permDt"] = Stamp(r.lp.PermanentlyClosedAt);
            if (r.lp.PeriodStatus == "OPEN")
            {
                var (can, message) = await control.CanClosePeriodAsync(id, ct);
                result["canClose"] = can;
                result["canCloseMsg"] = message;
                result["blockers"] = await control.GetCloseBlockerCountAsync(id, ct);
            }
        }

        if (await NextAsync(db, id, ct) is { } next)
            result["next"] = next;
        result["checklist"] = await db.Set<GlPeriodCloseChecklist>().Where(c => c.LedgerPeriodId == id)
            .GroupBy(c => c.TaskStatus).OrderBy(g => g.Key).Select(g => new { status = g.Key, n = g.Count() }).ToListAsync(ct);
        result["history"] = (await db.Set<GlPeriodStatusHistory>().AsNoTracking().Where(h => h.LedgerPeriodId == id)
                .OrderByDescending(h => h.ActionAt).ThenByDescending(h => h.PeriodHistoryId).Take(8).ToListAsync(ct))
            .Select(h => new { from = h.OldPeriodStatus, to = h.NewPeriodStatus, action = h.ActionCode, reason = h.ActionReason, by = h.ActionBy, at = Stamp(h.ActionAt) });
        return result;
    }
}

/// <summary>PER_SAVE_CAL: add or change a calendar.</summary>
public sealed class P30SaveCalendar(ICapitraDbContext db, GlPeriodSetup setup) : P30Process
{
    public override string Name => "PER_SAVE_CAL";

    public override string Permission => Permissions.Gl.SetupEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var code = Text(request.X01)?.ToUpperInvariant();
        var nameEn = Text(request.X02);
        if (code is null || nameEn is null)
            throw new ApexProcessException("Calendar code and English name are required.");

        await setup.UpsertCalendarAsync(code, nameEn, Text(request.X03), Text(request.X04) ?? "MONTHLY", Text(request.X05) ?? "MONTHLY",
            request.X06, request.X07, ct);
        var id = await db.Set<GlCalendar>().Where(c => c.CalendarCode == code).Select(c => c.CalendarId).SingleAsync(ct);
        return new { ok = true, id };
    }, ct);
}

/// <summary>PER_SAVE_PERIOD: add a period to a fiscal year or change one, inside the year's dates and without overlaps.</summary>
public sealed class P30SavePeriod(ICapitraDbContext db, GlPeriodSetup setup) : P30Process
{
    public override string Name => "PER_SAVE_PERIOD";

    public override string Permission => Permissions.Gl.SetupEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var id = Id(request.X01);
        var calCode = Text(request.X02)?.ToUpperInvariant();
        var year = Whole(request.X03);
        var code = Text(request.X05)?.ToUpperInvariant();
        var start = Day(request.X08);
        var end = Day(request.X09);
        var adj = request.X11 == "Y";
        var nameEn = Text(request.X06);
        var periodNo = Whole(request.X04);
        if (code is null || periodNo is null || nameEn is null || start is null || end is null)
            throw new ApexProcessException("Code, number, English name, start and end dates are required.");
        if (end < start)
            throw new ApexProcessException("The end date cannot be before the start date.");

        var cal = await db.Set<GlCalendar>().SingleOrDefaultAsync(c => c.CalendarCode == calCode, ct)
            ?? throw new ApexProcessException("Calendar not found: " + calCode);
        var fy = await db.Set<GlFiscalYear>().SingleOrDefaultAsync(y => y.CalendarId == cal.CalendarId && y.FiscalYear == year, ct)
            ?? throw new ApexProcessException($"Fiscal year {year} not found in calendar {calCode}.");
        if (start < fy.StartDate || end > fy.EndDate)
            throw new ApexProcessException($"The period must fall inside fiscal year {year} ({Dmy(fy.StartDate)} to {Dmy(fy.EndDate)}).");

        if (id is not null)
        {
            // the package updates by calendar + period code, so an existing period keeps its code
            var old = await db.Set<GlAccountingPeriod>().Where(p => p.PeriodId == id && p.CalendarId == cal.CalendarId).Select(p => p.PeriodCode).SingleOrDefaultAsync(ct)
                ?? throw new ApexProcessException("No data found.");
            if (old != code)
                throw new ApexProcessException("The period code cannot be changed.");
        }
        else if (await db.Set<GlAccountingPeriod>().AnyAsync(p => p.CalendarId == cal.CalendarId && p.PeriodCode == code, ct))
        {
            throw new ApexProcessException($"Period code {code} already exists in calendar {calCode}.");
        }

        if (!adj)
        {
            var hit = await db.Set<GlAccountingPeriod>()
                .Where(p => p.CalendarId == cal.CalendarId && !p.IsAdjustmentPeriod && p.PeriodCode != code && p.StartDate <= end && p.EndDate >= start)
                .MaxAsync(p => p.PeriodCode, ct);
            if (hit is not null)
                throw new ApexProcessException($"The dates overlap period {hit}.");
        }

        await setup.UpsertPeriodAsync(new GlPeriodSetup.PeriodInput(
            calCode, year!.Value, periodNo.Value, code, nameEn, Text(request.X07), start.Value, end.Value,
            Whole(request.X10), adj ? "Y" : "N", Text(request.X12) ?? "NEVER_OPENED"), ct);
        return new { ok = true };
    }, ct);
}

/// <summary>PER_SAVE_YEAR: change a fiscal year's name, dates and status, keeping its periods inside and other years apart.</summary>
public sealed class P30SaveYear(ICapitraDbContext db, GlPeriodSetup setup) : P30Process
{
    private static readonly string[] YearStatuses = ["DRAFT", "OPEN", "CLOSED", "PERMANENTLY_CLOSED"];

    public override string Name => "PER_SAVE_YEAR";

    public override string Permission => Permissions.Gl.SetupEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var calCode = Text(request.X01)?.ToUpperInvariant();
        var year = Whole(request.X02);
        var start = Day(request.X04);
        var end = Day(request.X05);
        var status = request.X06;
        if (start is null || end is null)
            throw new ApexProcessException("Start and end dates are required.");
        if (end < start)
            throw new ApexProcessException("The end date cannot be before the start date.");
        if (status is null || !YearStatuses.Contains(status))
            throw new ApexProcessException("Unknown fiscal year status.");

        var cal = await setup.GetCalendarAsync(calCode, ct);
        var outside = await (from p in db.Set<GlAccountingPeriod>()
                             join fy in db.Set<GlFiscalYear>() on p.FiscalYearId equals fy.FiscalYearId
                             where fy.CalendarId == cal.CalendarId && fy.FiscalYear == year && (p.StartDate < start || p.EndDate > end)
                             select p).CountAsync(ct);
        if (outside > 0)
            throw new ApexProcessException($"{outside} period(s) of this year would fall outside the new dates.");
        if (await db.Set<GlFiscalYear>().AnyAsync(y => y.CalendarId == cal.CalendarId && y.FiscalYear != year && y.StartDate <= end && y.EndDate >= start, ct))
            throw new ApexProcessException("The dates overlap another fiscal year of this calendar.");

        await setup.UpsertFiscalYearAsync(calCode, year ?? throw new ApexProcessException("No data found."), Text(request.X03), start.Value, end.Value, status, ct);
        return new { ok = true };
    }, ct);
}

/// <summary>PER_SYNC: the missing ledger periods, source statuses and checklist rows of a fiscal year, for every ledger on the calendar.</summary>
public sealed class P30Sync(ICapitraDbContext db, GlPeriodSetup setup) : P30Process
{
    public override string Name => "PER_SYNC";

    public override string Permission => Permissions.Gl.SetupEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var cal = Id(request.X01);
        var year = Whole(request.X02);
        var before = await db.Set<GlLedgerPeriod>().CountAsync(ct);
        var ledgers = await db.Set<GlLedger>().Where(l => l.CalendarId == cal).OrderBy(l => l.LedgerCode).Select(l => l.LedgerCode).ToListAsync(ct);
        foreach (var ledger in ledgers)
            await setup.SyncAllPeriodSupportAsync(ledger, year, ct);
        if (ledgers.Count == 0)
            throw new ApexProcessException("No ledger uses this calendar.");

        var created = await db.Set<GlLedgerPeriod>().CountAsync(ct) - before;
        return new
        {
            ok = true,
            message = $"{created} ledger period(s) created for {ledgers.Count} ledger(s). Source status and close checklist rows were added where missing.",
        };
    }, ct);
}
