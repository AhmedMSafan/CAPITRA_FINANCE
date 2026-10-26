using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl.AccountStructure;
using Capitra.Application.Modules.Gl.ChartOfAccounts;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Modules.Gl.Ledgers;
using Capitra.Application.Modules.Gl.Periods;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Capitra.Infrastructure.Persistence;
using Capitra.Tests.Steps;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Gl;

/// <summary>Manual journals from entry to posting and reversal (APEX App 120 pages 50-52, GL_POSTING_API_PKG).</summary>
public sealed class GlJournalServiceTests
{
    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeUser(string name) : ICurrentUser
    {
        public string? UserId => name;
        public string? UserName => name;
        public bool IsAuthenticated => true;
    }

    private static readonly IClock Clock = new FakeClock();

    private static GlJournalService Journals(CapitraDbContext db, string user = "loader") => new(db, Clock, new FakeUser(user));

    private static GlJournalQueryService Queries(CapitraDbContext db) => new(db, Clock, new FakeUser("loader"));

    private static readonly JournalActionRequest NoReason = new(null, null, null);

    private static SaveJournalLineRequest Line(string? company, string? branch, string? account, decimal? dr, decimal? cr) =>
        new(new Dictionary<int, string?> { [1] = company, [2] = branch, [3] = account }, dr, cr, null);

    private static SaveJournalRequest Journal(long ledgerId, DateOnly date, bool submit, params SaveJournalLineRequest[] lines) =>
        new(ledgerId, null, date, null, "Test journal", null, null, lines, submit, null);

    /// <summary>
    /// Structure CAP (company-branch-account, branch FIXED to HQ), accounts 1000 (summary), 1100 and 4100,
    /// ledger MAIN on calendar MAIN 2026 with OCT and NOV open.
    /// </summary>
    private static async Task<(string Database, long LedgerId)> SeedAsync()
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        db.Add(new AdmLkpCurrency { Code = "EGP", NameEn = "Egyptian pound", NameAr = "جنيه" });
        db.Add(new AdmLkpLegalEntity { Code = "CAP", NameEn = "Capitra", NameAr = "كابيترا" });
        db.Add(new GlJournalSource { SourceCode = GlCodes.SourceManual, SourceNameEn = "Manual", IsManualAllowed = true });
        db.Add(new GlJournalCategory { CategoryCode = GlCodes.CategoryStandard, CategoryNameEn = "Standard" });
        await db.SaveChangesAsync();

        var structures = new GlAccountStructureService(db, Clock);
        var coa = await structures.CreateStructureAsync(
            new("cap", "Capitra", null, null, null, 3, 3, 1, 2, null, null, null, true, false, true, null, null, null), default);
        var company = await structures.CreateSegmentAsync(coa.CoaId, new(1, "company", "Company", null, "COMPANY", 2, 2, "NUMERIC", null, true, true, false, true, null, null, null), default);
        var branch = await structures.CreateSegmentAsync(coa.CoaId, new(2, "branch", "Branch", null, "BRANCH", 2, 3, "ALPHANUMERIC", null, true, true, true, true, null, null, null), default);
        await structures.CreateSegmentAsync(coa.CoaId, new(3, "natural", "Account", null, "ACCOUNT", 4, 6, "NUMERIC", null, true, true, true, true, null, null, null), default);
        SaveSegmentValueRequest Value(string code, bool summary = false) =>
            new(code, code, null, null, null, summary, !summary, true, new DateOnly(2026, 1, 1), null, null, null, null, null);
        await structures.CreateValueAsync(company.CoaSegmentId, Value("01"), default);
        await structures.CreateValueAsync(branch.CoaSegmentId, Value("HQ"), default);
        await structures.CreateValueAsync(branch.CoaSegmentId, Value("ALX"), default);
        await structures.SaveDefaultAsync(branch.CoaSegmentId, new("HQ", GlCodes.DefaultModeFixed), default);

        var chart = new GlChartOfAccountsService(db, Clock);
        SaveGlAccountRequest Account(string code, string type, bool summary) =>
            new(coa.CoaId, code, "Account " + code, null, null, null, type, GlCodes.DefaultsFor(type).NormalBalance, GlCodes.DefaultsFor(type).StatementClass, "NONE", summary, !summary,
                false, false, false, false, false, true, true, false, false, true, new DateOnly(2026, 1, 1), null, null);
        await chart.CreateAsync(Account("1000", "ASSET", summary: true), default);
        await chart.CreateAsync(Account("1100", "ASSET", summary: false), default);
        await chart.CreateAsync(Account("4100", "REVENUE", summary: false), default);

        var periods = new GlAccountingPeriodService(db, Clock, new FakeUser("loader"));
        var calendar = await periods.CreateCalendarAsync(new("main", "Main", null, null, null, false, true, null), default);
        var ledger = await new GlLedgerService(db, Clock).CreateAsync(
            new("CAP", "main", "Main ledger", null, null, "EGP", calendar.CalendarId, coa.CoaId, true, null, null, null, null, null, null, null, null, null, null, null), default);
        await periods.GenerateYearAsync(calendar.CalendarId, new(2026, new DateOnly(2026, 1, 1), false, null, true), default);

        foreach (var code in new[] { "OCT-2026", "NOV-2026" })
            await periods.ApplyActionAsync(await LedgerPeriodAsync(db, code), new("OPEN", null, 1), default);

        return (database, ledger.LedgerId);
    }

    private static async Task<long> LedgerPeriodAsync(CapitraDbContext db, string code) =>
        await (from lp in db.Set<GlLedgerPeriod>()
               join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
               where p.PeriodCode == code
               select lp.LedgerPeriodId).SingleAsync();

    [Fact]
    public async Task Saving_checks_every_line_applies_the_fixed_branch_and_keeps_an_unbalanced_journal_as_a_draft()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var journals = Journals(db);
        var october = new DateOnly(2026, 10, 20);

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => journals.CreateAsync(Journal(s.LedgerId, october, false,
            Line("01", null, "1000", 100, null),
            Line("01", null, "1100", 50, 50),
            Line(null, null, "4100", null, 100)), default));
        Assert.Contains("summary account", bad.Errors["lines[0]"].Single());
        Assert.Contains("either a debit or a credit", bad.Errors["lines[1]"].Single());
        Assert.Contains("Missing company", bad.Errors["lines[2]"].Single());
        Assert.Empty(await db.Set<GlJournalHeader>().ToListAsync());

        var september = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            journals.CreateAsync(Journal(s.LedgerId, new DateOnly(2026, 9, 10), false), default));
        Assert.Contains("has never been opened", september.Errors["accountingDate"].Single());

        var saved = await journals.CreateAsync(Journal(s.LedgerId, october, true,
            Line("01", "ALX", "1100", 100, null),
            Line("01", null, "4100", null, 90)), default);
        Assert.Equal(("JV-2026-000001", GlCodes.JournalDraft, "OCT-2026"), (saved.Journal.JournalNo, saved.Journal.JournalStatus, saved.Journal.PeriodCode));
        Assert.Contains("not balanced", saved.SubmitError);
        Assert.Equal(["01-HQ-1100", "01-HQ-4100"], saved.Journal.Lines.Select(x => x.ConcatenatedAccount));

        var fixedUp = await journals.UpdateAsync(saved.Journal.JournalHeaderId, Journal(s.LedgerId, october, true,
            Line("01", null, "1100", 100, null),
            Line("01", null, "4100", null, 100)) with { RowVersion = saved.Journal.RowVersion }, default);
        Assert.Null(fixedUp.SubmitError);
        Assert.Equal(GlCodes.JournalSubmitted, fixedUp.Journal.JournalStatus);
        Assert.Equal(2, await db.Set<GlAccountCombination>().CountAsync());
        Assert.Equal(2, await db.Set<GlJournalLine>().CountAsync());

        var locked = await Assert.ThrowsAsync<BusinessValidationException>(() => journals.UpdateAsync(saved.Journal.JournalHeaderId,
            Journal(s.LedgerId, october, false) with { RowVersion = fixedUp.Journal.RowVersion }, default));
        Assert.Contains("can no longer be changed", locked.Errors["action"].Single());
    }

    [Fact]
    public async Task Another_user_approves_and_posting_adds_to_the_balances_with_the_earlier_periods_in_before_and_after()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var first = await Journals(db).CreateAsync(Journal(s.LedgerId, new DateOnly(2026, 10, 20), true,
            Line("01", null, "1100", 1000, null), Line("01", null, "4100", null, 1000)), default);
        var id = first.Journal.JournalHeaderId;

        var own = await Assert.ThrowsAsync<BusinessValidationException>(() => Journals(db).ActAsync(id, GlJournalService.Approve, NoReason, default));
        Assert.Contains("another user", own.Errors["action"].Single());
        var early = await Assert.ThrowsAsync<BusinessValidationException>(() => Journals(db).ActAsync(id, GlJournalService.Post, NoReason, default));
        Assert.Contains("Only an approved journal", early.Errors["action"].Single());

        await Journals(db, "checker").ActAsync(id, GlJournalService.Approve, NoReason, default);
        var posted = await Journals(db, "checker").ActAsync(id, GlJournalService.Post, NoReason, default);
        Assert.Equal((GlCodes.JournalPosted, "checker"), (posted.Journal.JournalStatus, posted.Journal.PostedBy));
        Assert.Equal([(0m, 1000m), (0m, -1000m)], posted.Journal.Impacts.Select(x => (x.BalanceBefore, x.BalanceAfter)));

        // A November journal on the same accounts starts from October's balance.
        var second = await Journals(db).CreateAsync(Journal(s.LedgerId, new DateOnly(2026, 11, 3), true,
            Line("01", null, "1100", 250, null), Line("01", null, "4100", null, 250)), default);
        var bulk = await Journals(db, "checker").ActManyAsync(GlJournalService.Approve, new([second.Journal.JournalHeaderId, id], null), default);
        Assert.Equal((1, 1), (bulk.Succeeded, bulk.Failed));
        await Journals(db, "checker").ActManyAsync(GlJournalService.Post, new([second.Journal.JournalHeaderId], null), default);
        var november = await Journals(db).GetAsync(second.Journal.JournalHeaderId, default);
        Assert.Equal([(1000m, 1250m), (-1000m, -1250m)], november.Impacts.Select(x => (x.BalanceBefore, x.BalanceAfter)));

        var balances = await Queries(db).BalancesAsync(new(s.LedgerId, "NOV-2026", null), default);
        Assert.Equal([("01-HQ-1100", 1000m, 250m, 0m, 1250m), ("01-HQ-4100", -1000m, 0m, 250m, -1250m)],
            balances.Rows.Select(x => (x.ConcatenatedAccount, x.Opening, x.PeriodDr, x.PeriodCr, x.Closing)));
        Assert.Equal(0m, balances.TotalClosing);

        var ledger = await Queries(db).AccountLedgerAsync(new(s.LedgerId, "NOV-2026", "NOV-2026", null, new Dictionary<int, string?> { [3] = "11" }), default);
        Assert.Equal((1000m, 250m, 1250m), (ledger.BroughtForward, ledger.TotalDr, ledger.Closing));
        Assert.Equal([(1000m, 1250m)], ledger.Rows.Select(x => (x.BalanceBefore, x.BalanceAfter)));

        var mine = await Queries(db).SearchAsync(new("MINE", null, null, null, null, null, null, null, null, null, null, null, null), default);
        Assert.Empty(mine.Rows);
        Assert.Equal(2, mine.Counts[GlCodes.JournalPosted]);
    }

    [Fact]
    public async Task A_failed_post_is_kept_on_the_journal_and_a_posted_reversal_marks_the_original_reversed()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var saved = await Journals(db).CreateAsync(Journal(s.LedgerId, new DateOnly(2026, 10, 20), true,
            Line("01", null, "1100", 500, null), Line("01", null, "4100", null, 500)), default);
        var id = saved.Journal.JournalHeaderId;
        await Journals(db, "checker").ActAsync(id, GlJournalService.Approve, NoReason, default);

        var periods = new GlAccountingPeriodService(db, Clock, new FakeUser("loader"));
        var october = await LedgerPeriodAsync(db, "OCT-2026");
        var version = await db.Set<GlLedgerPeriod>().Where(x => x.LedgerPeriodId == october).Select(x => x.RowVersion).SingleAsync();
        await periods.ApplyActionAsync(october, new("CLOSE", null, version), default);

        await Assert.ThrowsAsync<BusinessValidationException>(() => Journals(db, "checker").ActAsync(id, GlJournalService.Post, NoReason, default));
        var failed = await Journals(db).GetAsync(id, default);
        Assert.Equal(GlCodes.JournalApproved, failed.JournalStatus);
        Assert.Contains("Period OCT-2026 is closed", failed.LastPostError);
        Assert.Equal("POST_FAILED", failed.History[0].ActionCode);
        Assert.Empty(await db.Set<GlAccountBalance>().ToListAsync());

        version = await db.Set<GlLedgerPeriod>().Where(x => x.LedgerPeriodId == october).Select(x => x.RowVersion).SingleAsync();
        await periods.ApplyActionAsync(october, new("REOPEN", "Late invoice", version), default);
        await Journals(db, "checker").ActAsync(id, GlJournalService.Post, NoReason, default);

        var noReason = await Assert.ThrowsAsync<BusinessValidationException>(() => Journals(db).ActAsync(id, GlJournalService.Reverse, NoReason, default));
        Assert.Contains("reason", noReason.Errors.Keys);
        var reversed = await Journals(db).ActAsync(id, GlJournalService.Reverse, new("Wrong account", new DateOnly(2026, 11, 5), null), default);
        var reversal = await Journals(db).GetAsync(reversed.Created!.JournalHeaderId, default);
        Assert.Equal(("REV-JV-2026-000001", GlCodes.JournalDraft, "NOV-2026", true), (reversal.JournalNo, reversal.JournalStatus, reversal.PeriodCode, reversal.IsReversal));
        Assert.Equal([(0m, 500m), (500m, 0m)], reversal.Lines.Select(x => (x.Dr, x.Cr)));
        Assert.False(reversal.Allowed.Edit);
        var twice = await Assert.ThrowsAsync<BusinessValidationException>(() => Journals(db).ActAsync(id, GlJournalService.Reverse, new("Again", null, null), default));
        Assert.Contains("already has the reversal", twice.Errors["action"].Single());

        await Journals(db).ActAsync(reversal.JournalHeaderId, GlJournalService.Submit, NoReason, default);
        await Journals(db, "checker").ActAsync(reversal.JournalHeaderId, GlJournalService.Approve, NoReason, default);
        await Journals(db, "checker").ActAsync(reversal.JournalHeaderId, GlJournalService.Post, NoReason, default);

        var original = await Journals(db).GetAsync(id, default);
        Assert.Equal(GlCodes.JournalReversed, original.JournalStatus);
        Assert.False(original.Allowed.Reverse);
        var balances = await Queries(db).BalancesAsync(new(s.LedgerId, "NOV-2026", null), default);
        Assert.All(balances.Rows, x => Assert.Equal(0m, x.Closing));
    }
}
