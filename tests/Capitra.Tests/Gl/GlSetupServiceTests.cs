using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl.AccountStructure;
using Capitra.Application.Modules.Gl.ChartOfAccounts;
using Capitra.Domain.Modules.Gl;
using Capitra.Tests.Steps;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Gl;

/// <summary>Account structure, segments, values (APEX App 120 page 40) and the chart of accounts (pages 2/3).</summary>
public sealed class GlSetupServiceTests
{
    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc);
    }

    private static readonly IClock Clock = new FakeClock();

    private static SaveCoaStructureRequest Structure(string code = "cap", int count = 3, int? account = 3, int? rowVersion = null) =>
        new(code, "Capitra", null, null, null, count, account, 1, 2, null, null, null, true, false, true, null, null, rowVersion);

    private static SaveCoaSegmentRequest Segment(int num, string code, string type, int min, int max, string format = "NUMERIC") =>
        new(num, code, code + " name", null, type, min, max, format, null, true, true, true, true, null, null, null);

    private static SaveSegmentValueRequest Value(string code, long? parent = null, bool summary = false, bool posting = true, int? rowVersion = null) =>
        new(code, code + " name", null, null, parent, summary, posting, true, new DateOnly(2026, 1, 1), null, null, null, null, rowVersion);

    private static SaveGlAccountRequest Account(long coaId, string code, long? parent = null, bool summary = false, bool retained = false, int? rowVersion = null) =>
        new(coaId, code, code + " account", null, null, parent, "asset", "dr", "balance_sheet", "none", summary, true,
            false, false, false, false, false, true, true, false, retained, true, null, null, rowVersion);

    /// <summary>Structure CAP: 1 company (2 digits), 2 branch, 3 natural account (4 to 6 digits).</summary>
    private static async Task<(string Database, long CoaId, long Company, long Branch, long Natural)> SeedAsync()
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var service = new GlAccountStructureService(db, Clock);
        var coa = await service.CreateStructureAsync(Structure(), default);
        var company = await service.CreateSegmentAsync(coa.CoaId, Segment(1, "company", "company", 2, 2), default);
        var branch = await service.CreateSegmentAsync(coa.CoaId, Segment(2, "branch", "BRANCH", 2, 3, "ALPHANUMERIC"), default);
        var natural = await service.CreateSegmentAsync(coa.CoaId, Segment(3, "natural", "ACCOUNT", 4, 6), default);
        return (database, coa.CoaId, company.CoaSegmentId, branch.CoaSegmentId, natural.CoaSegmentId);
    }

    [Fact]
    public async Task A_structure_needs_an_account_segment_inside_its_segment_count()
    {
        await using var db = StepTestDb.Create(Guid.NewGuid().ToString());
        var service = new GlAccountStructureService(db, Clock);

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateStructureAsync(Structure(account: null), default));
        Assert.Contains("accountSegmentNum", missing.Errors.Keys);

        var outside = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateStructureAsync(Structure(count: 2, account: 3), default));
        Assert.Contains("accountSegmentNum", outside.Errors.Keys);

        var saved = await service.CreateStructureAsync(Structure(), default);
        Assert.Equal("CAP", saved.CoaCode);
        Assert.Equal("-", saved.DelimiterChar);
        Assert.Equal(new DateOnly(2026, 10, 23), saved.EffectiveStartDate);

        var duplicate = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateStructureAsync(Structure(" Cap "), default));
        Assert.Contains("coaCode", duplicate.Errors.Keys);
    }

    [Fact]
    public async Task Segments_stay_inside_the_structure_and_the_count_cannot_drop_below_them()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = new GlAccountStructureService(db, Clock);

        var tooHigh = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateSegmentAsync(s.CoaId, Segment(4, "extra", "FUTURE", 1, 5), default));
        Assert.Contains("segmentNum", tooHigh.Errors.Keys);

        var sameNum = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateSegmentAsync(s.CoaId, Segment(1, "other", "FUTURE", 1, 5), default));
        Assert.Contains("segmentNum", sameNum.Errors.Keys);

        var lengths = await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateSegmentAsync(s.Company, Segment(1, "company", "COMPANY", 5, 3) with { RowVersion = 1 }, default));
        Assert.Contains("maxLength", lengths.Errors.Keys);

        var shrink = await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateStructureAsync(s.CoaId, Structure(count: 2, account: 2, rowVersion: 1), default));
        Assert.Contains("segmentCount", shrink.Errors.Keys);

        var page = await service.GetAsync(null, default);
        Assert.Equal(["COMPANY", "BRANCH", "NATURAL"], page.Segments.Select(x => x.SegmentCode));
    }

    [Fact]
    public async Task Values_follow_the_segment_rules_and_their_hierarchy_cannot_loop()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = new GlAccountStructureService(db, Clock);

        var shortCode = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateValueAsync(s.Company, Value("1"), default));
        Assert.Contains("valueCode", shortCode.Errors.Keys);
        var letters = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateValueAsync(s.Company, Value("AB"), default));
        Assert.Contains("valueCode", letters.Errors.Keys);
        var summaryPosting = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateValueAsync(s.Branch, Value("EG", summary: true, posting: true), default));
        Assert.Contains("isPostingAllowed", summaryPosting.Errors.Keys);

        var eg = await service.CreateValueAsync(s.Branch, Value("eg", summary: true, posting: false), default);
        var hq = await service.CreateValueAsync(s.Branch, Value("hq", eg.SegmentValueId), default);
        Assert.Equal("HQ", hq.ValueCode);
        Assert.Equal("EG", hq.ParentValueCode);

        var loop = await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateValueAsync(eg.SegmentValueId, Value("EG", hq.SegmentValueId, true, false, eg.RowVersion), default));
        Assert.Contains("parentValueId", loop.Errors.Keys);

        var company = await service.CreateValueAsync(s.Company, Value("01"), default);
        var otherSegment = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateValueAsync(s.Branch, Value("ALX", company.SegmentValueId), default));
        Assert.Contains("parentValueId", otherSegment.Errors.Keys);
    }

    [Fact]
    public async Task Only_a_posting_value_can_be_the_journal_default_and_never_on_the_account_segment()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = new GlAccountStructureService(db, Clock);
        await service.CreateValueAsync(s.Branch, Value("EG", summary: true, posting: false), default);
        await service.CreateValueAsync(s.Branch, Value("HQ"), default);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.SaveDefaultAsync(s.Branch, new("EG", "FIXED"), default));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.SaveDefaultAsync(s.Natural, new("1000", "SUGGEST"), default));

        await service.SaveDefaultAsync(s.Branch, new("hq", "fixed"), default);
        var branch = await db.Set<GlCoaSegment>().AsNoTracking().FirstAsync(x => x.CoaSegmentId == s.Branch);
        Assert.Equal(("HQ", "FIXED"), (branch.DefaultValueCode, branch.DefaultMode));

        await service.SaveDefaultAsync(s.Branch, new(null, "FIXED"), default);
        branch = await db.Set<GlCoaSegment>().AsNoTracking().FirstAsync(x => x.CoaSegmentId == s.Branch);
        Assert.Equal((null, "SUGGEST"), (branch.DefaultValueCode, branch.DefaultMode));
    }

    [Fact]
    public async Task An_account_is_saved_with_its_account_segment_value_and_keeps_it_in_step()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = new GlChartOfAccountsService(db, Clock);

        var bad = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(Account(s.CoaId, "11A"), default));
        Assert.Contains("accountCode", bad.Errors.Keys);

        var assets = await service.CreateAsync(Account(s.CoaId, "1000", summary: true), default);
        Assert.False(assets.IsPostingAllowed); // a summary account never allows posting
        var cash = await service.CreateAsync(Account(s.CoaId, "1100", assets.AccountId), default);
        Assert.Equal(("ASSET", "DR", "BALANCE_SHEET"), (cash.AccountType, cash.NormalBalance, cash.FinancialStatementClass));

        var values = await db.Set<GlCoaSegmentValue>().AsNoTracking().Where(x => x.CoaSegmentId == s.Natural).OrderBy(x => x.ValueCode).ToListAsync();
        Assert.Equal(["1000", "1100"], values.Select(v => v.ValueCode));
        Assert.Equal(values[0].SegmentValueId, values[1].ParentValueId);
        Assert.True(values[0].IsSummary);
        Assert.Equal("1100", cash.SegmentValueCode);

        var duplicate = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(Account(s.CoaId, "1100"), default));
        Assert.Contains("accountCode", duplicate.Errors.Keys);

        var renamed = await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateAsync(cash.AccountId, Account(s.CoaId, "1200", assets.AccountId, rowVersion: cash.RowVersion), default));
        Assert.Contains("accountCode", renamed.Errors.Keys);

        var loop = await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateAsync(assets.AccountId, Account(s.CoaId, "1000", cash.AccountId, true, rowVersion: assets.RowVersion), default));
        Assert.Contains("parentAccountId", loop.Errors.Keys);

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(cash.AccountId, Account(s.CoaId, "1100", rowVersion: 99), default));

        var page = await service.GetAsync(null, default);
        Assert.Equal(2, page.Accounts.Count);
        Assert.Contains(page.TypeDefaults, d => d.AccountType == "REVENUE" && d.NormalBalance == "CR" && d.FinancialStatementClass == "INCOME_STATEMENT");
    }

    [Fact]
    public async Task Only_one_account_is_the_retained_earnings_account()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = new GlChartOfAccountsService(db, Clock);

        await service.CreateAsync(Account(s.CoaId, "3100", retained: true), default);
        await service.CreateAsync(Account(s.CoaId, "3200", retained: true), default);

        var flagged = await db.Set<GlAccount>().AsNoTracking().Where(x => x.IsRetainedEarnings).Select(x => x.AccountCode).ToListAsync();
        Assert.Equal(["3200"], flagged);
    }

    [Fact]
    public async Task Accounts_need_a_structure_whose_account_segment_is_of_type_account()
    {
        await using var db = StepTestDb.Create(Guid.NewGuid().ToString());
        var structures = new GlAccountStructureService(db, Clock);
        var coa = await structures.CreateStructureAsync(Structure(count: 1, account: 1) with { BranchSegmentNum = null }, default);
        var service = new GlChartOfAccountsService(db, Clock);

        var noSegment = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(Account(coa.CoaId, "1000"), default));
        Assert.Contains("no account segment", noSegment.Errors["accountCode"][0]);

        await structures.CreateSegmentAsync(coa.CoaId, Segment(1, "company", "COMPANY", 4, 4), default);
        var wrongType = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(Account(coa.CoaId, "1000"), default));
        Assert.Contains("not an ACCOUNT segment", wrongType.Errors["accountCode"][0]);
    }
}
