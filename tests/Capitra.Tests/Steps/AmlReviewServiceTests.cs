using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.AmlReview;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "AML Review" (APEX page 209) against an in-memory database.</summary>
public sealed class AmlReviewServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long DeveloperId, long Investor1, long Investor2, long Account1, long Account2);

    /// <summary>A portfolio whose data is loaded: two investors, one contract each, no deal decisions yet.</summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPurchasedPortfolio.DataLoadingCompletedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور", LegalNameEn = "Developer One" };
        var other = new MtgDeveloper { DeveloperCode = "DEV2", LegalNameAr = "مطور آخر" };
        db.AddRange(developer, other);
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        db.AddRange(project, investor1, investor2);
        await db.SaveChangesAsync();
        var unit1 = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-1" };
        var unit2 = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-2" };
        var portfolio = new MtgPurchasedPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status
        };
        db.AddRange(unit1, unit2, portfolio);
        await db.SaveChangesAsync();
        var account1 = new MtgPurchasedPortfolioAccount
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor1.InvestorId, ProjectUnitId = unit1.ProjectUnitId,
            ContractAmount = 1000, PaidAmount = 200, OutstandingAmount = 800
        };
        var account2 = new MtgPurchasedPortfolioAccount
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor2.InvestorId, ProjectUnitId = unit2.ProjectUnitId,
            ContractAmount = 3000, PaidAmount = 1000, OutstandingAmount = 2000
        };
        db.AddRange(account1, account2);
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId, developer.DeveloperId, investor1.InvestorId, investor2.InvestorId,
            account1.PortfolioAccountId, account2.PortfolioAccountId);
    }

    private static AddAmlResultRequest Check(string subjectType, long subjectId, string result = "CLEAR", decimal? score = null) =>
        new(subjectType, subjectId, "aml_screening", new DateOnly(2026, 10, 23), "REF-1", result, "low", score, null);

    private static MtgAmlReviewService Service(CapitraDbContext db) => new(db);

    [Fact]
    public async Task Accept_needs_every_screening_and_deal_decision_then_keeps_the_selected_totals()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);
        var accept = new AmlDecisionRequest("accept", "All clear.");

        var error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, accept, default));
        Assert.Equal("AML information is required for the portfolio developer.", error.Message);

        await service.AddResultAsync(s.PortfolioId, Check("DEVELOPER", s.DeveloperId), default);
        await service.AddResultAsync(s.PortfolioId, Check("INVESTOR", s.Investor1), default);
        error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, accept, default));
        Assert.Equal("AML information is missing for one or more portfolio investors.", error.Message);

        await service.AddResultAsync(s.PortfolioId, Check("INVESTOR", s.Investor2, "POTENTIAL_MATCH", 80), default);
        error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, accept, default));
        Assert.Equal("Every investor must be either Selected or Excluded before AML can be accepted.", error.Message);

        await service.SaveDealDecisionsAsync(s.PortfolioId, new SaveDealDecisionsRequest(
        [
            new DealDecisionItem(s.Account1, MtgPurchasedPortfolioAccount.SelectedDecision),
            new DealDecisionItem(s.Account2, MtgPurchasedPortfolioAccount.ExcludedDecision)
        ]), default);

        var review = await service.DecideAsync(s.PortfolioId, accept, default);
        Assert.Equal(MtgPurchasedPortfolio.AmlAcceptedStatus, review.PortfolioStatusCode);
        Assert.False(review.IsOpen);
        Assert.Equal("POTENTIAL_MATCH", review.Accounts.Single(a => a.PortfolioAccountId == s.Account2).AmlStatusCode);
        Assert.Equal("Developer One", review.Results.Single(r => r.SubjectTypeCode == "DEVELOPER").SubjectName);

        var portfolio = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal(1, portfolio.SelectedAccountCount);
        Assert.Equal(1000m, portfolio.SelectedFaceValueAmount);
        Assert.Equal(800m, portfolio.SelectedOutstandingAmount);
        Assert.Equal(200m, portfolio.SelectedPaidAmount);
        var history = await db.Set<MtgPurchasedPortfolioStatusHistory>().SingleAsync();
        Assert.Equal("All clear.", history.Comments);

        // Once accepted the step is closed.
        await Assert.ThrowsAsync<ConflictException>(() => service.AddResultAsync(s.PortfolioId, Check("DEVELOPER", s.DeveloperId), default));
    }

    [Fact]
    public async Task Reject_needs_a_comment_keeps_the_reason_and_leaves_the_review_open()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(
            () => service.DecideAsync(s.PortfolioId, new AmlDecisionRequest("REJECT", "  "), default));
        Assert.Equal("A comment is required for both AML Accept and Reject.", missing.Errors["Comment"][0]);

        var review = await service.DecideAsync(s.PortfolioId, new AmlDecisionRequest("REJECT", "Investor on sanctions list."), default);
        Assert.Equal(MtgPurchasedPortfolio.AmlRejectedStatus, review.PortfolioStatusCode);
        Assert.Equal("Investor on sanctions list.", review.RejectionReason);
        Assert.True(review.IsOpen);
    }

    [Fact]
    public async Task Checks_are_validated_and_must_be_about_the_portfolio()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() => service.AddResultAsync(s.PortfolioId,
            new AddAmlResultRequest("PARTNER", 1, "X", null, null, "MAYBE", "LOW", 101, null), default));
        Assert.Equal(["AmlResultCode", "AmlTypeCode", "MatchScorePct", "ScreeningDate", "SubjectTypeCode"], error.Errors.Keys.Order(StringComparer.Ordinal));

        var otherDeveloper = await db.Set<MtgDeveloper>().Where(x => x.DeveloperCode == "DEV2").Select(x => x.DeveloperId).SingleAsync();
        error = await Assert.ThrowsAsync<BusinessValidationException>(
            () => service.AddResultAsync(s.PortfolioId, Check("DEVELOPER", otherDeveloper), default));
        Assert.Equal("Selected subject does not belong to this portfolio.", error.Errors["SubjectId"][0]);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account1, "MAYBE")]), default));
    }

    [Fact]
    public async Task The_step_is_closed_before_the_data_is_loaded()
    {
        var s = await SeedAsync(MtgPurchasedPortfolio.DraftStatus);
        await using var db = StepTestDb.Create(s.Database);

        Assert.False((await Service(db).GetAsync(s.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(
            () => Service(db).DecideAsync(s.PortfolioId, new AmlDecisionRequest("ACCEPT", "x"), default));
    }
}
