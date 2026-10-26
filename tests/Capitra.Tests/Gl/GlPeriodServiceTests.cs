using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl.AccountStructure;
using Capitra.Application.Modules.Gl.ChartOfAccounts;
using Capitra.Application.Modules.Gl.Ledgers;
using Capitra.Application.Modules.Gl.Periods;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Capitra.Infrastructure.Persistence;
using Capitra.Tests.Steps;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Gl;

/// <summary>Calendars, fiscal years, periods and their open/close control (APEX App 120 page 30), and ledgers.</summary>
public sealed class GlPeriodServiceTests
{
    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeUser : ICurrentUser
    {
        public string? UserId => "u1";
        public string? UserName => "loader";
        public bool IsAuthenticated => true;
    }

    private static readonly IClock Clock = new FakeClock();

    private static GlAccountingPeriodService Periods(CapitraDbContext db) => new(db, Clock, new FakeUser());

    private static SaveGlLedgerRequest Ledger(string code, long calendarId, long? coaId = null, bool primary = true, string? retained = null, int? rowVersion = null) =>
        new("CAP", code, code + " ledger", null, null, "EGP", calendarId, coaId, primary, null, retained, null, null, null, null, null, null, null, null, rowVersion);

    private static GenerateFiscalYearRequest Year(int year, bool adjustment = true, bool ledgerPeriods = true) =>
        new(year, new DateOnly(year, 1, 1), adjustment, null, ledgerPeriods);

    /// <summary>Legal entity CAP, currency EGP and calendar MAIN.</summary>
    private static async Task<(string Database, long CalendarId)> SeedAsync()
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        db.Add(new AdmLkpCurrency { Code = "EGP", NameEn = "Egyptian pound", NameAr = "جنيه" });
        db.Add(new AdmLkpLegalEntity { Code = "CAP", NameEn = "Capitra", NameAr = "كابيترا" });
        await db.SaveChangesAsync();

        var calendar = await Periods(db).CreateCalendarAsync(new(" main ", "Main calendar", null, null, null, true, true, null), default);
        return (database, calendar.CalendarId);
    }

    [Fact]
    public async Task Generating_a_year_creates_twelve_months_and_an_adjustment_period_once()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Periods(db);

        var midMonth = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.GenerateYearAsync(s.CalendarId, Year(2026) with { StartDate = new DateOnly(2026, 1, 15) }, default));
        Assert.Contains("startDate", midMonth.Errors.Keys);

        var result = await service.GenerateYearAsync(s.CalendarId, Year(2026), default);
        Assert.StartsWith("Fiscal year 2026 generated with 13 periods", result.Message);

        var page = await service.GetAsync(null, default);
        Assert.Equal("MAIN", page.Calendars.Single().CalendarCode);
        var year = Assert.Single(page.Years);
        Assert.Equal((new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "OPEN", 13), (year.StartDate, year.EndDate, year.YearStatus, year.PeriodCount));

        var feb = page.Periods.Single(x => x.PeriodCode == "FEB-2026");
        Assert.Equal(("February 2026", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 1), (feb.PeriodNameEn, feb.StartDate, feb.EndDate, feb.QuarterNo));
        var adj = page.Periods.Single(x => x.PeriodCode == "ADJ-2026");
        Assert.True(adj.IsAdjustmentPeriod);
        Assert.Equal((13, new DateOnly(2026, 12, 31), 4), (adj.PeriodNo, adj.StartDate, adj.QuarterNo));
        Assert.All(page.Periods, p => Assert.Equal("NEVER_OPENED", p.DefaultPeriodStatus));

        var again = await Assert.ThrowsAsync<BusinessValidationException>(() => service.GenerateYearAsync(s.CalendarId, Year(2026), default));
        Assert.Contains("fiscalYear", again.Errors.Keys);

        var overlap = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.GenerateYearAsync(s.CalendarId, Year(2027) with { StartDate = new DateOnly(2026, 7, 1) }, default));
        Assert.Contains("startDate", overlap.Errors.Keys);
    }

    [Fact]
    public async Task Periods_stay_inside_their_year_and_regular_periods_never_overlap()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Periods(db);
        var yearId = (await service.GenerateYearAsync(s.CalendarId, Year(2026, adjustment: false), default)).FiscalYearId;
        var jan = await db.Set<GlAccountingPeriod>().AsNoTracking().FirstAsync(x => x.PeriodCode == "JAN-2026");

        SaveGlAccountingPeriodRequest Period(string code, DateOnly start, DateOnly end, bool adjustment = false, int? rowVersion = null) =>
            new(code, 14, code, null, start, end, 4, adjustment, null, rowVersion);

        var outside = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.CreatePeriodAsync(yearId, Period("X-2027", new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31)), default));
        Assert.Contains("startDate", outside.Errors.Keys);

        var overlap = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.CreatePeriodAsync(yearId, Period("X-2026", new DateOnly(2026, 3, 15), new DateOnly(2026, 4, 15)), default));
        Assert.Equal("The dates overlap period MAR-2026.", overlap.Errors["startDate"].Single());

        // An adjustment period may sit on top of the regular ones.
        var adj = await service.CreatePeriodAsync(yearId, Period("adj-2026", new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 31), adjustment: true), default);
        Assert.Equal("ADJ-2026", adj.PeriodCode);

        var rename = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdatePeriodAsync(jan.PeriodId, Period("JAN-26", jan.StartDate, jan.EndDate, rowVersion: jan.RowVersion), default));
        Assert.Contains("periodCode", rename.Errors.Keys);

        // The year cannot shrink past its periods.
        var year = await db.Set<GlFiscalYear>().AsNoTracking().FirstAsync(x => x.FiscalYearId == yearId);
        var shrink = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdateYearAsync(yearId, new("FY26", new DateOnly(2026, 1, 1), new DateOnly(2026, 11, 30), "OPEN", year.RowVersion), default));
        Assert.Contains("startDate", shrink.Errors.Keys);
    }

    [Fact]
    public async Task Ledger_periods_are_created_once_and_follow_open_close_reopen_and_permanent_close()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Periods(db);
        await new GlLedgerService(db, Clock).CreateAsync(Ledger("main", s.CalendarId), default);

        var yearId = (await service.GenerateYearAsync(s.CalendarId, Year(2026), default)).FiscalYearId;
        Assert.Equal(13, await db.Set<GlLedgerPeriod>().CountAsync());
        Assert.Equal(13, await db.Set<GlLedgerPeriodHistory>().CountAsync(x => x.ActionCode == "CREATE"));

        var sync = await service.SyncLedgerPeriodsAsync(yearId, default);
        Assert.Equal(0, sync.Created);

        var page = await service.GetAsync(s.CalendarId, default);
        var jan = page.Periods.Single(x => x.PeriodCode == "JAN-2026").Ledgers.Single();
        var adj = page.Periods.Single(x => x.PeriodCode == "ADJ-2026").Ledgers.Single();
        Assert.Equal(("MAIN", "NEVER_OPENED"), (jan.LedgerCode, jan.PeriodStatus));

        var stale = await Assert.ThrowsAsync<ConflictException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("OPEN", null, jan.RowVersion + 1), default));
        Assert.NotNull(stale);

        var opened = await service.ApplyActionAsync(jan.LedgerPeriodId, new("open", null, jan.RowVersion), default);
        Assert.Equal("OPEN", opened.PeriodStatus);
        Assert.Equal(("FEB-2026", "NEVER_OPENED"), (opened.Next!.PeriodCode, opened.Next.PeriodStatus));

        var info = await service.GetLedgerPeriodAsync(jan.LedgerPeriodId, default);
        Assert.Equal(("NORMAL", true, true, false, "loader"), (info.PostingMode, info.IsManualJournalAllowed, info.IsSubledgerAllowed, info.IsAdjustmentAllowed, info.OpenedBy));
        Assert.True(info.CanClose);

        var twice = await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("OPEN", null, info.RowVersion), default));
        Assert.Contains("action", twice.Errors.Keys);

        var closed = await service.ApplyActionAsync(jan.LedgerPeriodId, new("CLOSE", "Month end", info.RowVersion), default);
        Assert.Equal("CLOSED", closed.PeriodStatus);
        info = await service.GetLedgerPeriodAsync(jan.LedgerPeriodId, default);
        Assert.Equal(("NONE", false, "loader", "Month end"), (info.PostingMode, info.IsManualJournalAllowed, info.ClosedBy, info.CloseNotes));

        // A closed period is opened again only with Reopen, and Reopen needs a reason.
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("OPEN", null, info.RowVersion), default));
        var noReason = await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("REOPEN", " ", info.RowVersion), default));
        Assert.Contains("reason", noReason.Errors.Keys);

        await service.ApplyActionAsync(jan.LedgerPeriodId, new("REOPEN", "Late invoice", info.RowVersion), default);
        info = await service.GetLedgerPeriodAsync(jan.LedgerPeriodId, default);
        Assert.Equal(("OPEN", true), (info.PeriodStatus, info.IsAdjustmentAllowed));

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("PERMANENT", "Audit done", info.RowVersion), default));
        await service.ApplyActionAsync(jan.LedgerPeriodId, new("CLOSE", null, info.RowVersion), default);
        info = await service.GetLedgerPeriodAsync(jan.LedgerPeriodId, default);
        await service.ApplyActionAsync(jan.LedgerPeriodId, new("PERMANENT", "Audit done", info.RowVersion), default);
        info = await service.GetLedgerPeriodAsync(jan.LedgerPeriodId, default);
        Assert.Equal(("PERMANENTLY_CLOSED", "loader"), (info.PeriodStatus, info.PermanentlyClosedBy));
        Assert.Equal(["PERMANENT_CLOSE", "CLOSE", "REOPEN", "CLOSE", "OPEN", "CREATE"], info.History.Select(x => x.ActionCode));

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(jan.LedgerPeriodId, new("REOPEN", "Again", info.RowVersion), default));

        // The adjustment period takes adjustments only, never subledger postings.
        await service.ApplyActionAsync(adj.LedgerPeriodId, new("OPEN", null, adj.RowVersion), default);
        var adjInfo = await service.GetLedgerPeriodAsync(adj.LedgerPeriodId, default);
        Assert.Equal(("ADJUSTMENT_ONLY", false, true), (adjInfo.PostingMode, adjInfo.IsSubledgerAllowed, adjInfo.IsAdjustmentAllowed));

        // Future entry only for a period that was never opened.
        var feb = page.Periods.Single(x => x.PeriodCode == "FEB-2026").Ledgers.Single();
        var future = await service.ApplyActionAsync(feb.LedgerPeriodId, new("SET_FUTURE", null, feb.RowVersion), default);
        Assert.Equal("FUTURE_ENTRY", future.PeriodStatus);
        var febInfo = await service.GetLedgerPeriodAsync(feb.LedgerPeriodId, default);
        Assert.Equal("FUTURE_ONLY", febInfo.PostingMode);
        await service.ApplyActionAsync(feb.LedgerPeriodId, new("OPEN", null, febInfo.RowVersion), default);
        febInfo = await service.GetLedgerPeriodAsync(feb.LedgerPeriodId, default);
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.ApplyActionAsync(feb.LedgerPeriodId, new("SET_FUTURE", null, febInfo.RowVersion), default));
    }

    [Fact]
    public async Task A_ledger_has_one_primary_per_legal_entity_posting_control_accounts_and_keeps_its_calendar_once_it_has_periods()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var ledgers = new GlLedgerService(db, Clock);

        var structures = new GlAccountStructureService(db, Clock);
        var coa = await structures.CreateStructureAsync(
            new("cap", "Capitra", null, null, null, 1, 1, null, null, null, null, null, true, false, true, null, null, null), default);
        await structures.CreateSegmentAsync(coa.CoaId, new(1, "natural", "Natural", null, "ACCOUNT", 4, 6, "NUMERIC", null, true, true, true, true, null, null, null), default);
        var chart = new GlChartOfAccountsService(db, Clock);
        SaveGlAccountRequest Account(string code, bool summary) =>
            new(coa.CoaId, code, code, null, null, null, "EQUITY", "CR", "BALANCE_SHEET", "NONE", summary, !summary,
                false, false, false, false, false, true, true, false, false, true, null, null, null);
        await chart.CreateAsync(Account("3000", summary: true), default);
        await chart.CreateAsync(Account("3100", summary: false), default);

        var noStructure = await Assert.ThrowsAsync<BusinessValidationException>(() => ledgers.CreateAsync(Ledger("main", s.CalendarId, retained: "3100"), default));
        Assert.Contains("retainedEarningsAcctCode", noStructure.Errors.Keys);
        var summary = await Assert.ThrowsAsync<BusinessValidationException>(() => ledgers.CreateAsync(Ledger("main", s.CalendarId, coa.CoaId, retained: "3000"), default));
        Assert.Contains("retainedEarningsAcctCode", summary.Errors.Keys);

        var main = await ledgers.CreateAsync(Ledger(" main ", s.CalendarId, coa.CoaId, retained: "3100"), default);
        Assert.Equal(("MAIN", "MAIN", "CAP", "PRIMARY", "ACTIVE", new DateOnly(2026, 10, 23)),
            (main.LedgerCode, main.CalendarCode, main.CoaCode, main.LedgerType, main.LedgerStatus, main.StartDate));

        var second = await ledgers.CreateAsync(Ledger("ifrs", s.CalendarId), default);
        var page = await ledgers.GetAsync(default);
        Assert.Equal([("IFRS", true), ("MAIN", false)], page.Ledgers.Select(x => (x.LedgerCode, x.IsPrimary)));
        Assert.Equal(["3100"], page.Accounts.Select(x => x.AccountCode));

        var structurePage = await structures.GetAsync(coa.CoaId, default);
        Assert.Equal(["MAIN"], structurePage.Ledgers);

        var other = await Periods(db).CreateCalendarAsync(new("other", "Other", null, null, null, false, true, null), default);
        await Periods(db).GenerateYearAsync(s.CalendarId, Year(2026), default);
        var move = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            ledgers.UpdateAsync(second.LedgerId, Ledger("ifrs", other.CalendarId, rowVersion: second.RowVersion), default));
        Assert.Contains("calendarId", move.Errors.Keys);
    }
}
