using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.BoardReview;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Board Review" (APEX page 215) against an in-memory database.</summary>
public sealed class BoardReviewServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId);

    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc); // 23 Oct 2026 in Cairo
    }

    private static readonly DateOnly Today = new(2026, 10, 23);

    /// <summary>
    /// A portfolio whose financial review is accepted: investor 1 kept with two contracts (one cheque), investor 2 excluded
    /// (one cheque), one flagged AML result, one high risk inquiry, a legal review, two valuations and a saved price.
    /// </summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPortfolio.FinancialReviewAcceptedStatus, bool priced = true)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور", LegalNameEn = "Developer One" };
        db.Add(developer);
        db.AddRange(
            new MtgLkpPortfolioStatus { Code = MtgPortfolio.CoverageReviewAcceptedStatus, NameAr = "-", NameEn = "Coverage Review Accepted" },
            new MtgLkpPortfolioStatus { Code = MtgPortfolio.FinancialReviewAcceptedStatus, NameAr = "-", NameEn = "Financial Review Accepted" });
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        db.AddRange(project, investor1, investor2);
        await db.SaveChangesAsync();
        var portfolio = new MtgPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status,
            PvRate = 12, MarginPct = 5, TermMonths = 24, SubmittedDate = new DateOnly(2026, 10, 20), RejectionReason = "Earlier"
        };
        db.Add(portfolio);
        await db.SaveChangesAsync();
        var id = portfolio.PortfolioId;
        MtgPortfolioAccount Account(MtgInvestor investor, long unit, decimal outstanding, string? decision) => new()
        {
            PortfolioId = id, InvestorId = investor.InvestorId, ProjectUnitId = unit,
            ContractAmount = outstanding * 2, PaidAmount = outstanding, OutstandingAmount = outstanding, DealDecisionCode = decision
        };
        var a1 = Account(investor1, 1, 800, MtgPortfolioAccount.SelectedDecision);
        var a2 = Account(investor1, 1, 200, null);
        var a3 = Account(investor2, 2, 500, MtgPortfolioAccount.ExcludedDecision);
        db.AddRange(a1, a2, a3);
        await db.SaveChangesAsync();
        db.AddRange(
            new MtgPortfolioCheque { PortfolioId = id, PortfolioAccountId = a1.PortfolioAccountId, ChequeNo = "C-1", DueDate = Today, ChequeAmount = 1000 },
            new MtgPortfolioCheque { PortfolioId = id, PortfolioAccountId = a3.PortfolioAccountId, ChequeNo = "C-2", DueDate = Today, ChequeAmount = 500 },
            new MtgAmlResult { PortfolioId = id, SubjectTypeCode = "INVESTOR", AmlTypeCode = "AML_SCREENING", AmlResultCode = "CLEAR", RiskLevelCode = "LOW" },
            new MtgAmlResult { PortfolioId = id, SubjectTypeCode = "INVESTOR", AmlTypeCode = "AML_SCREENING", AmlResultCode = "POTENTIAL_MATCH", RiskLevelCode = "HIGH" },
            new MtgInquiryResult { PortfolioId = id, SubjectTypeCode = "PORTFOLIO", InquiryTypeCode = "CREDIT", RiskLevelCode = "critical" },
            new MtgInquiryResult { PortfolioId = id, SubjectTypeCode = "PORTFOLIO", InquiryTypeCode = "CREDIT" },
            new MtgLegalReview { PortfolioId = id, IsOwnershipVerified = true, IsTitleDeedVerified = true, LegalRecommendation = "Proceed" },
            new MtgValuation { PortfolioId = id, ValuationTypeCode = "MARKET", ValuerName = "Old", ValuationDate = new DateOnly(2026, 9, 1), MarketValueAmount = 1 },
            new MtgValuation { PortfolioId = id, ValuationTypeCode = "MARKET", ValuerName = "New", ValuationDate = new DateOnly(2026, 10, 10), MarketValueAmount = 2000 },
            new MtgPortfolioStatusHistory { PortfolioId = id, FromStatusCode = MtgPortfolio.CoverageReviewAcceptedStatus, ToStatusCode = MtgPortfolio.FinancialReviewAcceptedStatus, Comments = "Priced." });
        if (priced)
            db.Add(new MtgPortfolioFinancialReview
            {
                PortfolioId = id, PvMethodCode = "ANNUAL", PvRate = 12, PvBaseDate = Today, ChequeCount = 1, ChequesFaceAmount = 1000,
                ChequesPvAmount = 1000, MarginPct = 5, MarginAmount = 50, AdminFinalAmount = 10, IsAdminIncluded = false,
                LegalFinalAmount = 20, IsLegalIncluded = true, ValuationFinalAmount = 5, IsValuationIncluded = true,
                TotalChargesAmount = 25, ProposedPurchaseAmount = 925, ExpectedReturnAmount = 75
            });
        await db.SaveChangesAsync();
        return new Seeded(database, id);
    }

    private static MtgBoardReviewService Service(CapitraDbContext db) => new(db, new FakeClock());

    private static BoardDecisionRequest Decision(string action, string no = "BR-7", string comment = "Approved as priced.", DateOnly? date = null) =>
        new(action, no, date ?? new DateOnly(2026, 10, 22), comment);

    [Fact]
    public async Task The_report_brings_every_step_together()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var r = await Service(db).GetAsync(s.PortfolioId, default);

        Assert.True(r.IsOpen);
        Assert.Equal(("Developer One", "Financial Review Accepted"), (r.DeveloperName, r.PortfolioStatusName));
        var t = r.Totals;
        Assert.Equal((3, 2, 3000m, 1500m), (t.ContractCount, t.InvestorCount, t.FaceValueAmount, t.OutstandingAmount));
        Assert.Equal((2, 1, 1, 2000m, 1000m), (t.SelectedContractCount, t.SelectedInvestorCount, t.SelectedUnitCount, t.SelectedFaceValueAmount, t.SelectedOutstandingAmount));
        Assert.Equal((1, 1000m), (t.ChequeCount, t.ChequeAmount)); // the excluded investor's cheque is left out
        Assert.Equal((0m, 20m, 5m, 925m), (r.Pricing!.AdminDeductedAmount, r.Pricing.LegalDeductedAmount, r.Pricing.ValuationDeductedAmount, r.Pricing.ProposedPurchaseAmount));
        var dd = r.DueDiligence;
        Assert.Equal((2, 1, 2, 1), (dd.AmlCount, dd.AmlFlaggedCount, dd.InquiryCount, dd.InquiryHighRiskCount));
        Assert.Equal("Proceed", dd.Legal!.LegalRecommendation);
        Assert.Equal(("New", 2000m), (dd.Valuation!.ValuerName, dd.Valuation.MarketValueAmount));
        Assert.Equal(["INV1", "INV2"], r.Investors.Select(i => i.InvestorCode));
        Assert.Equal((2, true, 1000m), (r.Investors[0].SelectedContractCount, r.Investors[0].IsIncluded, r.Investors[0].ChequeAmount));
        Assert.Equal((false, 500m), (r.Investors[1].IsIncluded, r.Investors[1].ChequeAmount));
        Assert.Equal(("Coverage Review Accepted", "Financial Review Accepted", "loader"), (r.History.Single().FromStatusName, r.History.Single().ToStatusName, r.History.Single().ChangedBy));
        Assert.Null(r.Decision);
    }

    [Fact]
    public async Task Approval_records_the_resolution()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var r = await Service(db).DecideAsync(s.PortfolioId, Decision("accept"), default);

        Assert.Equal(MtgPortfolio.BoardReviewAcceptedStatus, r.PortfolioStatusCode);
        Assert.False(r.IsOpen);
        Assert.Equal(new BoardDecisionDto(MtgPortfolio.BoardAccepted, "BR-7", new DateOnly(2026, 10, 22), Today), r.Decision);
        var portfolio = await db.Set<MtgPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal(("Approved as priced.", null, new DateOnly(2026, 10, 20)), (portfolio.Comments, portfolio.RejectionReason, portfolio.SubmittedDate));
        Assert.Equal("Board decision BR-7: Approved as priced.", r.History[^1].Comments);
        await Assert.ThrowsAsync<ConflictException>(() => Service(db).DecideAsync(s.PortfolioId, Decision("REJECT"), default));
    }

    [Fact]
    public async Task Rejection_keeps_the_reason_and_the_step_open()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var r = await service.DecideAsync(s.PortfolioId, Decision("REJECT", "BR-8", "Price too high."), default);
        Assert.Equal(MtgPortfolio.BoardReviewRejectedStatus, r.PortfolioStatusCode);
        Assert.Equal("Price too high.", r.RejectionReason);
        Assert.True(r.IsOpen);
        Assert.Equal((MtgPortfolio.BoardRejected, (DateOnly?)null), (r.Decision!.BoardDecisionCode, r.Decision.ApprovedDate));

        // The board can decide again at a later meeting.
        r = await service.DecideAsync(s.PortfolioId, Decision("ACCEPT", "BR-9"), default);
        Assert.Equal(("BR-9", Today), (r.Decision!.BoardDecisionNo, r.Decision.ApprovedDate));
    }

    [Fact]
    public async Task The_resolution_is_validated()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            Service(db).DecideAsync(s.PortfolioId, new BoardDecisionRequest("MAYBE", " ", new DateOnly(2026, 10, 24), ""), default));
        Assert.Equal(["Action", "Comment", "DecisionDate", "DecisionNo"], error.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("The board decision date cannot be in the future.", error.Errors["DecisionDate"][0]);
        Assert.Equal("Board decision number is required.", error.Errors["DecisionNo"][0]);

        var noDate = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            Service(db).DecideAsync(s.PortfolioId, new BoardDecisionRequest("ACCEPT", "BR-1", null, "ok"), default));
        Assert.Equal("The board decision date is required.", noDate.Errors["DecisionDate"][0]);
    }

    [Fact]
    public async Task Approval_needs_a_price_and_the_step_is_closed_before_the_financial_review()
    {
        var unpriced = await SeedAsync(priced: false);
        await using var db = StepTestDb.Create(unpriced.Database);
        Assert.Null((await Service(db).GetAsync(unpriced.PortfolioId, default)).Pricing);
        var error = await Assert.ThrowsAsync<ConflictException>(() => Service(db).DecideAsync(unpriced.PortfolioId, Decision("ACCEPT"), default));
        Assert.Equal("The financial review has not been saved, so there is no price to approve.", error.Message);

        var early = await SeedAsync(MtgPortfolio.ChequeCoverageAcceptedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(earlyDb).DecideAsync(early.PortfolioId, Decision("REJECT"), default));
    }
}
