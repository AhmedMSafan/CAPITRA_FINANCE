using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.CoverageReview;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Coverage Review" (APEX page 212) against an in-memory database.</summary>
public sealed class CoverageReviewServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long Investor1, long Investor2, long Investor3);

    /// <summary>
    /// A portfolio whose valuation is completed. Investor 1 has paid 30% over two contracts, investor 2 has paid 10%,
    /// investor 3 has a contract with no value.
    /// </summary>
    private static async Task<Seeded> SeedAsync(decimal? coveragePct = 20, string status = MtgPortfolio.ValuationCompletedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور" };
        db.Add(developer);
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        var investor3 = new MtgInvestor { InvestorCode = "INV3", FullNameAr = "مستثمر ٣" };
        db.AddRange(project, investor1, investor2, investor3);
        await db.SaveChangesAsync();
        var portfolio = new MtgPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status, CoveragePct = coveragePct
        };
        db.Add(portfolio);
        await db.SaveChangesAsync();
        MtgPortfolioAccount Account(MtgInvestor investor, decimal contract, decimal paid, string? decision) => new()
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor.InvestorId,
            ContractAmount = contract, PaidAmount = paid, OutstandingAmount = contract - paid, DealDecisionCode = decision
        };
        db.AddRange(
            Account(investor1, 1000, 100, MtgPortfolioAccount.ExcludedDecision),
            Account(investor1, 1000, 500, MtgPortfolioAccount.SelectedDecision),
            Account(investor2, 3000, 300, MtgPortfolioAccount.SelectedDecision),
            Account(investor3, 0, 0, MtgPortfolioAccount.ExcludedDecision));
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId, investor1.InvestorId, investor2.InvestorId, investor3.InvestorId);
    }

    private static MtgCoverageReviewService Service(CapitraDbContext db) => new(db);

    [Fact]
    public async Task Investors_are_added_up_and_compared_with_the_required_coverage()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var review = await Service(db).GetAsync(s.PortfolioId, default);
        Assert.Equal(20m, review.RequiredCoveragePct);
        var one = review.Investors.Single(i => i.InvestorId == s.Investor1);
        Assert.Equal((2, 2000m, 600m, 30m, true, MtgCoverageReviewService.MixedDecision),
            (one.ContractCount, one.ContractAmount, one.PaidAmount, one.PaidPct!.Value, one.MeetsCoverage!.Value, one.DealDecisionCode));
        Assert.False(review.Investors.Single(i => i.InvestorId == s.Investor2).MeetsCoverage);
        var three = review.Investors.Single(i => i.InvestorId == s.Investor3);
        Assert.Null(three.PaidPct);
        Assert.Null(three.MeetsCoverage);
    }

    [Fact]
    public async Task The_rule_keeps_investors_at_or_above_coverage_and_excludes_the_rest()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var result = await Service(db).ApplyRuleAsync(s.PortfolioId, default);
        Assert.Equal(2, result.ChangedInvestorCount);
        var decisions = result.Review.Investors.ToDictionary(i => i.InvestorId, i => i.DealDecisionCode);
        Assert.Equal(MtgPortfolioAccount.SelectedDecision, decisions[s.Investor1]);
        Assert.Equal(MtgPortfolioAccount.ExcludedDecision, decisions[s.Investor2]);
        Assert.Equal(MtgPortfolioAccount.ExcludedDecision, decisions[s.Investor3]); // no contract value: left as it was

        Assert.Equal(0, (await Service(db).ApplyRuleAsync(s.PortfolioId, default)).ChangedInvestorCount);

        var manual = await Service(db).SetInvestorDecisionAsync(s.PortfolioId, s.Investor2, new("selected"), default);
        Assert.Equal(MtgPortfolioAccount.SelectedDecision, manual.Investors.Single(i => i.InvestorId == s.Investor2).DealDecisionCode);
        await Assert.ThrowsAsync<NotFoundException>(() => Service(db).SetInvestorDecisionAsync(s.PortfolioId, 999, new("EXCLUDED"), default));
        await Assert.ThrowsAsync<BusinessValidationException>(() => Service(db).SetInvestorDecisionAsync(s.PortfolioId, s.Investor2, new("MAYBE"), default));
    }

    [Fact]
    public async Task Coverage_must_be_set_before_the_rule_or_accept()
    {
        var s = await SeedAsync(coveragePct: null);
        await using var db = StepTestDb.Create(s.Database);

        var error = await Assert.ThrowsAsync<ConflictException>(() => Service(db).ApplyRuleAsync(s.PortfolioId, default));
        Assert.Equal("Set the required coverage percentage on the portfolio first.", error.Message);
        error = await Assert.ThrowsAsync<ConflictException>(() => Service(db).DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("Set the required coverage percentage on the portfolio before accepting.", error.Message);
    }

    [Fact]
    public async Task Accept_needs_a_comment_and_a_kept_investor_then_keeps_the_selected_totals()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", " "), default));
        Assert.Equal("A comment is required for both Accept and Reject.", missing.Errors["Comment"][0]);

        foreach (var investor in new[] { s.Investor1, s.Investor2 })
            await service.SetInvestorDecisionAsync(s.PortfolioId, investor, new("EXCLUDED"), default);
        var error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("At least one investor must stay in the deal before coverage can be accepted.", error.Message);

        await service.SetInvestorDecisionAsync(s.PortfolioId, s.Investor1, new("SELECTED"), default);
        var review = await service.DecideAsync(s.PortfolioId, new("accept", "Coverage met."), default);
        Assert.Equal(MtgPortfolio.CoverageReviewAcceptedStatus, review.PortfolioStatusCode);
        Assert.False(review.IsOpen);

        var portfolio = await db.Set<MtgPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal((2, 2000m, 1400m, 600m),
            (portfolio.SelectedAccountCount, portfolio.SelectedFaceValueAmount, portfolio.SelectedOutstandingAmount, portfolio.SelectedPaidAmount));
        Assert.Null(portfolio.RejectionReason);
    }

    [Fact]
    public async Task Reject_keeps_the_reason_and_leaves_the_step_open_but_it_is_closed_before_valuation()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var review = await Service(db).DecideAsync(s.PortfolioId, new("REJECT", "Too many investors below coverage."), default);
        Assert.Equal(MtgPortfolio.CoverageReviewRejectedStatus, review.PortfolioStatusCode);
        Assert.Equal("Too many investors below coverage.", review.RejectionReason);
        Assert.True(review.IsOpen);

        var early = await SeedAsync(status: MtgPortfolio.LegalReviewAcceptedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(earlyDb).ApplyRuleAsync(early.PortfolioId, default));
    }
}
