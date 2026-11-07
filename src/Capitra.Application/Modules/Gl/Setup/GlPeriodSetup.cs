using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Setup;

/// <summary>
/// C# port of FINOVA gl_period_setup_pkg: calendars, fiscal years, accounting periods, and the ledger periods,
/// source statuses and close checklist rows made from them. Each upsert updates the row with the same code or inserts it.
/// </summary>
public sealed class GlPeriodSetup(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    // sync_source_status: the sub-ledger sources every ledger period tracks.
    private static readonly (string Source, string BusinessLine, string Role)[] Sources =
    [
        ("GL", "ALL", "GL_MANAGER"),
        ("LEASING", "LEASING", "LEASING_ACCOUNTING_USER"),
        ("FACTORING", "FACTORING", "FACTORING_ACCOUNTING_USER"),
        ("MORTGAGE", "MORTGAGE", "MORTGAGE_ACCOUNTING_USER"),
        ("TREASURY", "TREASURY", "TREASURY_ACCOUNTING_USER"),
        ("CASH", "ALL", "CASH_MANAGER"),
        ("FA", "ALL", "FA_ACCOUNTING_USER"),
    ];

    private string User => currentUser.UserName ?? "SYSTEM";

    public static string? NormalizeCode(string? code) => code?.Trim().ToUpperInvariant();

    /// <summary>UPPER(NVL(TRIM(value), default)) = 'Y'.</summary>
    private static bool Yn(string? value, bool whenNull) =>
        string.IsNullOrWhiteSpace(value) ? whenNull : string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

    public async Task<GlCalendar> GetCalendarAsync(string? calendarCode, CancellationToken ct)
    {
        var code = NormalizeCode(calendarCode);
        return await db.Set<GlCalendar>().SingleOrDefaultAsync(c => c.CalendarCode == code, ct)
            ?? throw new ApexProcessException("Calendar not found: " + calendarCode);
    }

    public async Task UpsertCalendarAsync(
        string? calendarCode, string calendarNameEn, string? calendarNameAr, string calendarType, string periodFrequency,
        string? adjustmentPeriodFlag, string? activeFlag, CancellationToken ct)
    {
        var code = NormalizeCode(calendarCode) ?? string.Empty;
        var row = await db.Set<GlCalendar>().SingleOrDefaultAsync(c => c.CalendarCode == code, ct);
        if (row is null)
        {
            row = new GlCalendar { CalendarCode = code };
            db.Add(row);
        }

        row.CalendarNameEn = calendarNameEn;
        row.CalendarNameAr = calendarNameAr;
        row.CalendarType = NormalizeCode(calendarType)!;
        row.PeriodFrequency = NormalizeCode(periodFrequency)!;
        row.IsAdjustmentPeriod = Yn(adjustmentPeriodFlag, true);
        row.IsActive = Yn(activeFlag, true);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertFiscalYearAsync(
        string? calendarCode, int fiscalYear, string? yearName, DateOnly startDate, DateOnly endDate, string yearStatus, CancellationToken ct)
    {
        if (endDate < startDate)
            throw new ApexProcessException("Fiscal year end date cannot be before start date.");

        var calendar = await GetCalendarAsync(calendarCode, ct);
        var row = await db.Set<GlFiscalYear>().SingleOrDefaultAsync(y => y.CalendarId == calendar.CalendarId && y.FiscalYear == fiscalYear, ct);
        if (row is null)
        {
            row = new GlFiscalYear { CalendarId = calendar.CalendarId, FiscalYear = fiscalYear };
            db.Add(row);
        }

        row.YearName = yearName ?? fiscalYear.ToString(CultureInfo.InvariantCulture);
        row.StartDate = startDate;
        row.EndDate = endDate;
        row.YearStatus = NormalizeCode(yearStatus)!;
        await db.SaveChangesAsync(ct);
    }

    public sealed record PeriodInput(
        string? CalendarCode, int FiscalYear, int PeriodNo, string? PeriodCode, string PeriodNameEn, string? PeriodNameAr,
        DateOnly StartDate, DateOnly EndDate, int? QuarterNo, string? AdjustmentPeriodFlag, string PeriodStatus);

    public async Task UpsertPeriodAsync(PeriodInput p, CancellationToken ct)
    {
        if (p.EndDate < p.StartDate)
            throw new ApexProcessException("Period end date cannot be before start date.");

        var calendar = await GetCalendarAsync(p.CalendarCode, ct);
        var year = await db.Set<GlFiscalYear>().SingleOrDefaultAsync(y => y.CalendarId == calendar.CalendarId && y.FiscalYear == p.FiscalYear, ct)
            ?? throw new ApexProcessException($"Fiscal year not found: {p.CalendarCode} / {p.FiscalYear}");
        var code = NormalizeCode(p.PeriodCode) ?? string.Empty;

        var row = await db.Set<GlAccountingPeriod>().SingleOrDefaultAsync(x => x.CalendarId == calendar.CalendarId && x.PeriodCode == code, ct);
        if (row is null)
        {
            row = new GlAccountingPeriod { FiscalYearId = year.FiscalYearId, CalendarId = calendar.CalendarId, PeriodCode = code };
            db.Add(row);
        }

        row.PeriodNo = p.PeriodNo;
        row.PeriodNameEn = p.PeriodNameEn;
        row.PeriodNameAr = p.PeriodNameAr;
        row.StartDate = p.StartDate;
        row.EndDate = p.EndDate;
        row.QuarterNo = p.QuarterNo;
        row.IsAdjustmentPeriod = Yn(p.AdjustmentPeriodFlag, false);
        row.PeriodStatus = NormalizeCode(p.PeriodStatus)!;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>The fiscal year from the start date for 12 months, its 12 monthly periods and, when asked, the adjustment period.</summary>
    public async Task GenerateMonthlyPeriodsAsync(
        string calendarCode, int fiscalYear, DateOnly startDate, string? includeAdjustmentPeriod, string yearStatus, CancellationToken ct)
    {
        var yearText = fiscalYear.ToString(CultureInfo.InvariantCulture);
        var end = startDate.AddMonths(12).AddDays(-1);
        await UpsertFiscalYearAsync(calendarCode, fiscalYear, yearText, startDate, end, yearStatus, ct);

        for (var i = 1; i <= 12; i++)
        {
            var periodStart = startDate.AddMonths(i - 1);
            var periodEnd = new DateOnly(periodStart.Year, periodStart.Month, DateTime.DaysInMonth(periodStart.Year, periodStart.Month));
            var code = periodStart.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant() + "-" + yearText;
            var name = periodStart.ToString("MMMM", CultureInfo.InvariantCulture) + " " + yearText;
            await UpsertPeriodAsync(
                new PeriodInput(calendarCode, fiscalYear, i, code, name, null, periodStart, periodEnd, (i + 2) / 3, "N", "NEVER_OPENED"), ct);
        }

        if (Yn(includeAdjustmentPeriod, true))
        {
            await UpsertPeriodAsync(
                new PeriodInput(calendarCode, fiscalYear, 13, "ADJ-" + yearText, "Adjustment Period " + yearText, null, end, end, 4, "Y", "NEVER_OPENED"), ct);
        }
    }

    /// <summary>sync_ledger_periods: a ledger period for every accounting period of the ledger's calendar that has none yet.</summary>
    public async Task SyncLedgerPeriodsAsync(string? ledgerCode, int? fiscalYear, CancellationToken ct)
    {
        var code = NormalizeCode(ledgerCode);
        var missing = await (from l in db.Set<GlLedger>()
                             join p in db.Set<GlAccountingPeriod>() on l.CalendarId equals p.CalendarId
                             join fy in db.Set<GlFiscalYear>() on p.FiscalYearId equals fy.FiscalYearId
                             where (code == null || l.LedgerCode == code) && (fiscalYear == null || fy.FiscalYear == fiscalYear)
                                   && !db.Set<GlLedgerPeriod>().Any(lp => lp.LedgerId == l.LedgerId && lp.PeriodId == p.PeriodId)
                             select new { l.LedgerId, p.PeriodId, p.PeriodStatus, p.IsAdjustmentPeriod }).ToListAsync(ct);

        foreach (var r in missing)
        {
            var open = r.PeriodStatus == "OPEN";
            var row = new GlLedgerPeriod
            {
                LedgerId = r.LedgerId,
                PeriodId = r.PeriodId,
                PeriodStatus = r.PeriodStatus,
                PostingMode = open && r.IsAdjustmentPeriod ? "ADJUSTMENT_ONLY"
                    : open ? "NORMAL"
                    : r.PeriodStatus == "FUTURE_ENTRY" ? "FUTURE_ONLY"
                    : "NONE",
                IsManualJournalAllowed = open,
                IsSubledgerAllowed = open,
                IsReversalAllowed = open,
                IsAdjustmentAllowed = open && r.IsAdjustmentPeriod,
            };
            db.Add(row);
            await db.SaveChangesAsync(ct);
            db.Add(new GlPeriodStatusHistory
            {
                LedgerPeriodId = row.LedgerPeriodId,
                NewPeriodStatus = row.PeriodStatus,
                ActionCode = "CREATE",
                ActionReason = "Ledger period initialized from accounting period definition.",
                ActionBy = User,
                ActionAt = clock.UtcNow,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>sync_source_status: the source status rows of a ledger period that are missing, as OPEN / NOT_REQUIRED.</summary>
    public async Task SyncSourceStatusAsync(long ledgerPeriodId, CancellationToken ct)
    {
        var have = await db.Set<GlPeriodSourceStatus>().Where(s => s.LedgerPeriodId == ledgerPeriodId)
            .Select(s => new { s.SourceCode, s.BusinessLineCode }).ToListAsync(ct);
        foreach (var s in Sources.Where(s => !have.Any(h => h.SourceCode == s.Source && h.BusinessLineCode == s.BusinessLine)))
        {
            db.Add(new GlPeriodSourceStatus
            {
                LedgerPeriodId = ledgerPeriodId,
                SourceCode = s.Source,
                BusinessLineCode = s.BusinessLine,
                SourceStatus = "OPEN",
                TransferStatus = "NOT_REQUIRED",
                ResponsibleRoleCode = s.Role,
                Notes = "Initialized by GL_PERIOD_SETUP_PKG.",
            });
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>sync_close_checklist: a NOT_STARTED checklist row for every active close task the ledger period lacks.</summary>
    public async Task SyncCloseChecklistAsync(long ledgerPeriodId, CancellationToken ct)
    {
        var tasks = await db.Set<GlPeriodCloseTask>().Where(t => t.IsActive
                && !db.Set<GlPeriodCloseChecklist>().Any(c => c.LedgerPeriodId == ledgerPeriodId && c.CloseTaskId == t.CloseTaskId))
            .ToListAsync(ct);
        foreach (var t in tasks)
        {
            db.Add(new GlPeriodCloseChecklist
            {
                LedgerPeriodId = ledgerPeriodId,
                CloseTaskId = t.CloseTaskId,
                TaskStatus = "NOT_STARTED",
                IsRequired = t.IsMandatory,
                ResponsibleRoleCode = t.DefaultResponsibleRole,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>sync_all_period_support: ledger periods, then the source statuses and close checklist of each one.</summary>
    public async Task SyncAllPeriodSupportAsync(string? ledgerCode, int? fiscalYear, CancellationToken ct)
    {
        await SyncLedgerPeriodsAsync(ledgerCode, fiscalYear, ct);

        var code = NormalizeCode(ledgerCode);
        var ids = await (from lp in db.Set<GlLedgerPeriod>()
                         join l in db.Set<GlLedger>() on lp.LedgerId equals l.LedgerId
                         join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                         join fy in db.Set<GlFiscalYear>() on p.FiscalYearId equals fy.FiscalYearId
                         where (code == null || l.LedgerCode == code) && (fiscalYear == null || fy.FiscalYear == fiscalYear)
                         select lp.LedgerPeriodId).ToListAsync(ct);
        foreach (var id in ids)
        {
            await SyncSourceStatusAsync(id, ct);
            await SyncCloseChecklistAsync(id, ct);
        }
    }
}
