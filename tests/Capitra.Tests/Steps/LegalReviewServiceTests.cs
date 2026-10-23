using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.LegalReview;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Legal Review" (APEX page 211) against an in-memory database.</summary>
public sealed class LegalReviewServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long Account1, long Account2);

    /// <summary>A portfolio whose inquiries step is completed: two investors, one contract each, both selected.</summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPortfolio.InquiryAcceptedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور", LegalNameEn = "Developer One" };
        db.Add(developer);
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        db.AddRange(project, investor1, investor2);
        await db.SaveChangesAsync();
        var unit1 = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-1" };
        var unit2 = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-2" };
        var portfolio = new MtgPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status
        };
        db.AddRange(unit1, unit2, portfolio);
        await db.SaveChangesAsync();
        var account1 = new MtgPortfolioAccount
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor1.InvestorId, ProjectUnitId = unit1.ProjectUnitId,
            ContractAmount = 1000, PaidAmount = 200, OutstandingAmount = 800, DealDecisionCode = MtgPortfolioAccount.SelectedDecision
        };
        var account2 = new MtgPortfolioAccount
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor2.InvestorId, ProjectUnitId = unit2.ProjectUnitId,
            ContractAmount = 3000, PaidAmount = 1000, OutstandingAmount = 2000, DealDecisionCode = MtgPortfolioAccount.SelectedDecision
        };
        db.AddRange(account1, account2);
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId, account1.PortfolioAccountId, account2.PortfolioAccountId);
    }

    private static SaveLegalReviewRequest Review(bool titleDeed = true, DateOnly? approvedDate = null, int? rowVersion = null) =>
        new(true, true, true, true, true, titleDeed, "  Title clear.  ", null, "A. Lawyer", new DateOnly(2026, 10, 22),
            "Legal Manager", approvedDate ?? new DateOnly(2026, 10, 23), rowVersion);

    private static MtgLegalReviewService Service(CapitraDbContext db) => new(db);

    private static readonly LegalDecisionRequest Accept = new("accept", null);

    [Fact]
    public async Task Accept_needs_a_complete_saved_checklist_then_keeps_the_selected_totals()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);
        const string incomplete = "Legal Review cannot be accepted until all six legal checks are Yes and both Review Date and Approved Date are completed.";

        var error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, Accept, default));
        Assert.Equal(incomplete, error.Message);

        var step = await service.SaveAsync(s.PortfolioId, Review(titleDeed: false), default);
        Assert.Equal("Title clear.", step.Review!.LegalFindings);
        Assert.False(step.Review.IsComplete);
        Assert.Equal("loader", step.Review.SavedBy);
        error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, Accept, default));
        Assert.Equal(incomplete, error.Message);

        // The second save changes the same review, so it must carry its row version.
        await Assert.ThrowsAsync<ConflictException>(() => service.SaveAsync(s.PortfolioId, Review(), default));
        step = await service.SaveAsync(s.PortfolioId, Review(rowVersion: step.Review.RowVersion), default);
        Assert.True(step.Review!.IsComplete);
        Assert.Equal(1, await db.Set<MtgLegalReview>().CountAsync());

        await service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account2, MtgPortfolioAccount.ExcludedDecision)]), default);

        step = await service.DecideAsync(s.PortfolioId, Accept, default);
        Assert.Equal(MtgPortfolio.LegalReviewAcceptedStatus, step.PortfolioStatusCode);
        Assert.False(step.IsOpen);

        var portfolio = await db.Set<MtgPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal(1, portfolio.SelectedAccountCount);
        Assert.Equal(1000m, portfolio.SelectedFaceValueAmount);
        Assert.Equal(800m, portfolio.SelectedOutstandingAmount);
        Assert.Equal(200m, portfolio.SelectedPaidAmount);
        Assert.Equal("Legal Review accepted.", (await db.Set<MtgPortfolioStatusHistory>().SingleAsync()).Comments);

        await Assert.ThrowsAsync<ConflictException>(() => service.SaveAsync(s.PortfolioId, Review(rowVersion: step.Review!.RowVersion), default));
    }

    [Fact]
    public async Task Excluded_contracts_can_be_included_again_but_undecided_ones_block_accept()
    {
        var s = await SeedAsync();
        await using (var setup = StepTestDb.Create(s.Database))
        {
            var account = await setup.Set<MtgPortfolioAccount>().SingleAsync(x => x.PortfolioAccountId == s.Account1);
            account.DealDecisionCode = null;
            await setup.SaveChangesAsync();
        }

        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);
        await service.SaveAsync(s.PortfolioId, Review(), default);

        var error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, Accept, default));
        Assert.Equal("Every investor must be either Selected or Excluded before Legal Review can be accepted.", error.Message);

        await service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account1, MtgPortfolioAccount.ExcludedDecision)]), default);
        await service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account1, MtgPortfolioAccount.SelectedDecision)]), default);
        var step = await service.GetAsync(s.PortfolioId, default);
        Assert.All(step.Accounts, a => Assert.Equal(MtgPortfolioAccount.SelectedDecision, a.DealDecisionCode));
    }

    [Fact]
    public async Task Reject_leaves_the_step_open_and_it_is_closed_before_inquiries_are_done()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        await Assert.ThrowsAsync<BusinessValidationException>(
            () => Service(db).DecideAsync(s.PortfolioId, new LegalDecisionRequest("APPROVE", null), default));
        var step = await Service(db).DecideAsync(s.PortfolioId, new LegalDecisionRequest("REJECT", "Title deed missing."), default);
        Assert.Equal(MtgPortfolio.LegalReviewRejectedStatus, step.PortfolioStatusCode);
        Assert.True(step.IsOpen);
        Assert.Null(step.Review);
        Assert.Equal("Title deed missing.", (await db.Set<MtgPortfolioStatusHistory>().SingleAsync()).Comments);

        var early = await SeedAsync(MtgPortfolio.AmlAcceptedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(earlyDb).SaveAsync(early.PortfolioId, Review(), default));
    }

    [Fact]
    public async Task Text_lengths_are_checked()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() => Service(db).SaveAsync(s.PortfolioId,
            Review() with { ReviewedBy = new string('x', 256), LegalFindings = new string('x', 4001) }, default));
        Assert.Equal(["LegalFindings", "ReviewedBy"], error.Errors.Keys.Order(StringComparer.Ordinal));
    }
}
