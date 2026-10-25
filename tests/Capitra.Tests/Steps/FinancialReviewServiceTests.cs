using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.FinancialReview;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Financial Review" (APEX page 214): the pricing calculator and the step service.</summary>
public sealed class FinancialReviewServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId);

    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc); // 23 Oct 2026 in Cairo
    }

    /// <summary>
    /// A portfolio whose cheque coverage is accepted, priced at 12% PV, 5% margin, 1% admin, 10 legal and 5 valuation per unit.
    /// Contract A (outstanding 1000, unit 1) has a 1000 cheque due in a year, contract B (500, unit 2, no decision so kept)
    /// a 500 cheque due today, and the excluded contract C has a cheque that must be ignored.
    /// </summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPurchasedPortfolio.ChequeCoverageAcceptedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور" };
        db.Add(developer);
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر", FullNameEn = "Investor One" };
        db.AddRange(project, investor);
        await db.SaveChangesAsync();
        var portfolio = new MtgPurchasedPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status,
            PvRate = 12, MarginPct = 5, AdminExpensePct = 1, LegalFeePerUnitAmount = 10, ValuationFeePerUnitAmount = 5
        };
        db.Add(portfolio);
        await db.SaveChangesAsync();
        MtgPurchasedPortfolioAccount Account(long unit, decimal outstanding, string? decision) => new()
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor.InvestorId, ProjectUnitId = unit,
            ContractAmount = outstanding * 2, PaidAmount = outstanding, OutstandingAmount = outstanding, DealDecisionCode = decision
        };
        var a = Account(1, 1000, MtgPurchasedPortfolioAccount.SelectedDecision);
        var b = Account(2, 500, null);
        var c = Account(3, 900, MtgPurchasedPortfolioAccount.ExcludedDecision);
        db.AddRange(a, b, c);
        await db.SaveChangesAsync();
        MtgPurchasedPortfolioCheque Cheque(MtgPurchasedPortfolioAccount account, string no, DateOnly due, decimal amount) => new()
        {
            PortfolioId = portfolio.PortfolioId, PortfolioAccountId = account.PortfolioAccountId, ChequeNo = no, DueDate = due, ChequeAmount = amount
        };
        db.AddRange(
            Cheque(a, "C-1", new DateOnly(2027, 10, 23), 1000),
            Cheque(b, "C-2", new DateOnly(2026, 10, 23), 500),
            Cheque(c, "C-3", new DateOnly(2027, 1, 1), 900));
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId);
    }

    private static MtgFinancialReviewService Service(CapitraDbContext db) => new(db, new FakeClock());

    [Theory]
    [InlineData("2026-12-23", "2026-10-23", "2")]
    [InlineData("2027-02-28", "2026-11-30", "3")] // both month ends
    [InlineData("2026-11-30", "2026-10-23", "1.2258064516129032258064516129")] // 1 + 7/31
    [InlineData("2026-10-01", "2026-10-23", "-0.7096774193548387096774193548")]
    public void Months_between_follows_oracle(string later, string earlier, string expected) =>
        Assert.Equal(decimal.Parse(expected), FinancialReviewCalculator.MonthsBetween(DateOnly.Parse(later), DateOnly.Parse(earlier)));

    [Theory]
    [InlineData("ANNUAL", 365, 12, "892.86")] // 1000 / 1.12
    [InlineData("MONTHLY", 365, 12, "887.45")] // 1000 / 1.01^12
    [InlineData("SIMPLE", 182, 6, "943.54")] // 1000 / (1 + 0.12 * 182 / 365)
    [InlineData("ANNUAL", 0, 0, "1000")]
    public void Present_value_by_method(string method, int days, int months, string expected) =>
        Assert.Equal(decimal.Parse(expected),
            Math.Round(FinancialReviewCalculator.PresentValue(1000, 0.12m, method, days, months), 2, MidpointRounding.AwayFromZero));

    [Fact]
    public void A_zero_rate_leaves_the_cheque_as_it_is() =>
        Assert.Equal(1000m, FinancialReviewCalculator.PresentValue(1000, 0, FinancialReviewCalculator.MonthlyMethod, 365, 12));

    [Fact]
    public void Margin_and_admin_bases_and_overrides_change_the_price()
    {
        var terms = new FinancialTerms(12, 5, 1, 10, 5);
        var kept = new KeptContracts(1500, 3000, 2);
        IReadOnlyList<ChequeToValue> cheques = [new(1, "C-1", "Investor", new DateOnly(2027, 10, 23), 1000)];
        var today = new DateOnly(2026, 10, 23);

        var c = FinancialReviewCalculator.Calculate(
            FinancialReviewInputs.Default with
            {
                MarginBaseCode = FinancialReviewCalculator.ChequesPvBase, AdminBaseCode = FinancialReviewCalculator.ProposedPurchaseBase,
                LegalFinalAmount = 7.005m, IsValuationIncluded = false
            }, terms, kept, cheques, today);

        Assert.Equal(892.86m, c.ChequesPvAmount);
        Assert.Equal(44.64m, c.MarginAmount); // 5% of 892.86
        Assert.Equal(848.22m, c.AdminBaseAmount); // pv - margin
        Assert.Equal(8.48m, c.AdminCalcAmount);
        Assert.Equal((20m, 7.01m), (c.LegalCalcAmount, c.LegalFinalAmount));
        Assert.Equal(15.49m, c.TotalChargesAmount); // admin + legal, valuation not deducted
        Assert.Equal(832.73m, c.ProposedPurchaseAmount);
        Assert.Equal(167.27m, c.ExpectedReturnAmount);
    }

    [Fact]
    public async Task Save_prices_the_kept_cheques_and_copies_the_price_to_the_portfolio()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var step = await service.GetAsync(s.PortfolioId, default);
        Assert.True(step.IsOpen);
        Assert.Null(step.Saved);
        var c = step.Calculation!;
        Assert.Equal(new DateOnly(2026, 10, 23), c.PvBaseDate);
        Assert.Equal(["C-2", "C-1"], c.Cheques.Select(x => x.ChequeNo)); // excluded contract's cheque left out, by due date
        Assert.Equal((1500m, 1392.86m, 1500m, 3000m, 2), (c.ChequesFaceAmount, c.ChequesPvAmount, c.SelectedOutstandingAmount, c.SelectedFaceValueAmount, c.UnitCount));
        Assert.Equal((75m, 15m, 20m, 10m, 45m), (c.MarginAmount, c.AdminCalcAmount, c.LegalCalcAmount, c.ValuationCalcAmount, c.TotalChargesAmount));
        Assert.Equal((1272.86m, 227.14m), (c.ProposedPurchaseAmount, c.ExpectedReturnAmount));

        var inputs = FinancialReviewInputs.Default with { AdminFinalAmount = 12, IsAdminIncluded = true, IsLegalIncluded = false };
        var saved = await service.SaveAsync(s.PortfolioId, new(inputs, null), default);
        Assert.Equal(1295.86m, saved.Saved!.ProposedPurchaseAmount); // 1392.86 - 75 - 12 - 10
        Assert.Equal("loader", saved.Saved.SavedBy);
        Assert.Equal(inputs, saved.Inputs); // the admin override comes back, untouched amounts stay null

        var portfolio = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal((1295.86m, 204.14m, 12m), (portfolio.ProposedPurchaseAmount, portfolio.ExpectedReturnAmount, portfolio.AdminExpenseAmount));

        // A stale save is refused; a current one replaces the row.
        await Assert.ThrowsAsync<ConflictException>(() => service.SaveAsync(s.PortfolioId, new(inputs, saved.Saved.RowVersion + 1), default));
        var again = await service.SaveAsync(s.PortfolioId, new(FinancialReviewInputs.Default with { IsAdminIncluded = false }, saved.Saved.RowVersion), default);
        Assert.Equal(1287.86m, again.Saved!.ProposedPurchaseAmount);
        Assert.Equal(1, await db.Set<MtgPurchasedPortfolioFinancialReview>().CountAsync());
        Assert.Equal(0m, (await db.Set<MtgPurchasedPortfolio>().AsNoTracking().SingleAsync()).AdminExpenseAmount);
    }

    [Fact]
    public async Task Accept_needs_a_comment_and_a_saved_review()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", " "), default));
        Assert.Equal("A comment is required for the decision.", missing.Errors["Comment"][0]);
        var unsaved = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("Save the financial review before completing the step.", unsaved.Message);

        await service.SaveAsync(s.PortfolioId, new(FinancialReviewInputs.Default, null), default);
        var step = await service.DecideAsync(s.PortfolioId, new("accept", "Priced."), default);
        Assert.Equal(MtgPurchasedPortfolio.FinancialReviewAcceptedStatus, step.PortfolioStatusCode);
        Assert.False(step.IsOpen);
        var portfolio = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal(new DateOnly(2026, 10, 23), portfolio.SubmittedDate);
        Assert.Equal("Priced.", (await db.Set<MtgPurchasedPortfolioStatusHistory>().SingleAsync()).Comments);
        await Assert.ThrowsAsync<ConflictException>(() => service.SaveAsync(s.PortfolioId, new(FinancialReviewInputs.Default, step.Saved!.RowVersion), default));
    }

    [Fact]
    public async Task Accept_needs_cheques_and_a_price_above_zero()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var saved = await service.SaveAsync(s.PortfolioId, new(FinancialReviewInputs.Default with { AdminFinalAmount = 5000 }, null), default);
        Assert.True(saved.Saved!.ProposedPurchaseAmount < 0);
        var negative = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("The proposed purchase value must be above zero.", negative.Message);

        db.RemoveRange(db.Set<MtgPurchasedPortfolioCheque>());
        await db.SaveChangesAsync();
        await service.SaveAsync(s.PortfolioId, new(FinancialReviewInputs.Default, saved.Saved.RowVersion), default);
        var none = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("There are no cheques to value for the included investors.", none.Message);
    }

    [Fact]
    public async Task Reject_keeps_the_reason_and_leaves_the_step_open_but_it_is_closed_before_cheque_coverage()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var step = await Service(db).DecideAsync(s.PortfolioId, new("REJECT", "Margin too thin."), default);
        Assert.Equal(MtgPurchasedPortfolio.FinancialReviewRejectedStatus, step.PortfolioStatusCode);
        Assert.Equal("Margin too thin.", step.RejectionReason);
        Assert.True(step.IsOpen);

        var early = await SeedAsync(MtgPurchasedPortfolio.CoverageReviewAcceptedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(earlyDb).SaveAsync(early.PortfolioId, new(FinancialReviewInputs.Default, null), default));
    }
}
