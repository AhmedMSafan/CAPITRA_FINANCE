using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ChequeCoverage;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Cheque Coverage Review" (APEX page 213) against an in-memory database.</summary>
public sealed class ChequeCoverageServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long Investor1, long Account1a, long Account1b, long Account2, long ExcludedAccount);

    /// <summary>
    /// A portfolio whose coverage review is accepted. Investor 1 owes 800 + 200 over two contracts, investor 2 owes 500
    /// (no decision recorded, so kept), investor 3 is excluded.
    /// </summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPurchasedPortfolio.CoverageReviewAcceptedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور" };
        db.Add(developer);
        db.AddRange(
            new AdmLkpChequeStatus { Code = "PENDING", NameAr = "قيد الانتظار", NameEn = "Pending" },
            new AdmLkpChequeStatus { Code = "CLEARED", NameAr = "محصل", NameEn = "Cleared" });
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        var investor3 = new MtgInvestor { InvestorCode = "INV3", FullNameAr = "مستثمر ٣" };
        db.AddRange(project, investor1, investor2, investor3);
        await db.SaveChangesAsync();
        var portfolio = new MtgPurchasedPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status
        };
        db.Add(portfolio);
        await db.SaveChangesAsync();
        MtgPurchasedPortfolioAccount Account(MtgInvestor investor, decimal outstanding, string? decision) => new()
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor.InvestorId,
            ContractAmount = outstanding * 2, PaidAmount = outstanding, OutstandingAmount = outstanding, DealDecisionCode = decision
        };
        var a1a = Account(investor1, 800, MtgPurchasedPortfolioAccount.SelectedDecision);
        var a1b = Account(investor1, 200, MtgPurchasedPortfolioAccount.SelectedDecision);
        var a2 = Account(investor2, 500, null);
        var a3 = Account(investor3, 900, MtgPurchasedPortfolioAccount.ExcludedDecision);
        db.AddRange(a1a, a1b, a2, a3);
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId, investor1.InvestorId, a1a.PortfolioAccountId, a1b.PortfolioAccountId,
            a2.PortfolioAccountId, a3.PortfolioAccountId);
    }

    private static SaveChequeRequest Cheque(long accountId, decimal amount, string no = "C-1", string? status = null, int? rowVersion = null) =>
        new(accountId, no, "QNB", null, new DateOnly(2026, 12, 1), amount, true, status, null, null, rowVersion);

    private static MtgChequeCoverageService Service(CapitraDbContext db) => new(db);

    [Fact]
    public async Task Cheques_count_towards_each_kept_investor()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var first = await service.CreateAsync(s.PortfolioId, Cheque(s.Account1a, 600.456m), default);
        Assert.Equal(600.46m, first.ChequeAmount);
        Assert.Equal(MtgPurchasedPortfolioCheque.PendingStatus, first.ChequeStatusCode);
        Assert.Equal("Pending", first.ChequeStatusName);
        await service.CreateAsync(s.PortfolioId, Cheque(s.Account1b, 399.54m, "C-2", "cleared"), default);

        var step = await service.GetAsync(s.PortfolioId, default);
        Assert.Equal(["INV1", "INV2"], step.Investors.Select(i => i.InvestorCode));
        var one = step.Investors.Single(i => i.InvestorId == s.Investor1);
        Assert.Equal((2, 1000m, 2, 1000m, true), (one.ContractCount, one.OutstandingAmount, one.ChequeCount, one.ChequeAmount, one.IsCovered));
        Assert.False(step.Investors.Single(i => i.InvestorCode == "INV2").IsCovered);
        Assert.DoesNotContain(step.Accounts, a => a.PortfolioAccountId == s.ExcludedAccount);

        // Moving a cheque to a smaller balance, a stale edit and deleting.
        var moved = await service.UpdateAsync(s.PortfolioId, first.ChequeId, Cheque(s.Account2, 500, rowVersion: first.RowVersion), default);
        Assert.Equal(s.Account2, moved.PortfolioAccountId);
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(s.PortfolioId, first.ChequeId, Cheque(s.Account2, 1, rowVersion: first.RowVersion), default));
        await service.DeleteAsync(s.PortfolioId, moved.ChequeId, moved.RowVersion, default);
        Assert.Single((await service.GetAsync(s.PortfolioId, default)).Cheques);
    }

    [Fact]
    public async Task Cheques_are_validated()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() => Service(db).CreateAsync(s.PortfolioId,
            new SaveChequeRequest(s.ExcludedAccount, " ", null, null, null, 0, false, "BOUNCED", null, null, null), default));
        Assert.Equal(["ChequeAmount", "ChequeNo", "ChequeStatusCode", "DueDate", "PortfolioAccountId"], error.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("Choose a contract of an investor who is included in the deal.", error.Errors["PortfolioAccountId"][0]);
        Assert.Equal("Unknown cheque status BOUNCED.", error.Errors["ChequeStatusCode"][0]);
    }

    [Fact]
    public async Task Accept_needs_a_comment_and_every_investor_covered()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var missing = await Assert.ThrowsAsync<BusinessValidationException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", ""), default));
        Assert.Equal("A comment is required for both Accept and Reject.", missing.Errors["Comment"][0]);

        await service.CreateAsync(s.PortfolioId, Cheque(s.Account1a, 1000), default);
        var error = await Assert.ThrowsAsync<ConflictException>(() => service.DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("1 investor(s) do not have cheques covering their outstanding balance.", error.Message);

        await service.CreateAsync(s.PortfolioId, Cheque(s.Account2, 499.996m, "C-2"), default); // rounds to 500.00
        var step = await service.DecideAsync(s.PortfolioId, new("accept", "All covered."), default);
        Assert.Equal(MtgPurchasedPortfolio.ChequeCoverageAcceptedStatus, step.PortfolioStatusCode);
        Assert.False(step.IsOpen);
        Assert.Equal("All covered.", (await db.Set<MtgPurchasedPortfolioStatusHistory>().SingleAsync()).Comments);
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(s.PortfolioId, Cheque(s.Account1a, 1), default));
    }

    [Fact]
    public async Task Accept_needs_an_investor_in_the_deal()
    {
        var s = await SeedAsync();
        await using (var setup = StepTestDb.Create(s.Database))
        {
            await setup.Set<MtgPurchasedPortfolioAccount>().ForEachAsync(a => a.DealDecisionCode = MtgPurchasedPortfolioAccount.ExcludedDecision);
            await setup.SaveChangesAsync();
        }

        await using var db = StepTestDb.Create(s.Database);
        var error = await Assert.ThrowsAsync<ConflictException>(() => Service(db).DecideAsync(s.PortfolioId, new("ACCEPT", "ok"), default));
        Assert.Equal("No investors are included in this deal.", error.Message);
    }

    [Fact]
    public async Task Reject_keeps_the_reason_and_leaves_the_step_open_but_it_is_closed_before_coverage_review()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var step = await Service(db).DecideAsync(s.PortfolioId, new("REJECT", "Cheques not endorsed."), default);
        Assert.Equal(MtgPurchasedPortfolio.ChequeCoverageRejectedStatus, step.PortfolioStatusCode);
        Assert.Equal("Cheques not endorsed.", step.RejectionReason);
        Assert.True(step.IsOpen);

        var early = await SeedAsync(MtgPurchasedPortfolio.ValuationCompletedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(earlyDb).CreateAsync(early.PortfolioId, Cheque(early.Account1a, 1), default));
    }
}
