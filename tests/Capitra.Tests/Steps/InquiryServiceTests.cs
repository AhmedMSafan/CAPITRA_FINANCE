using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Inquiries;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Inquiries" (APEX page 210) against an in-memory database.</summary>
public sealed class InquiryServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long DeveloperId, long Investor1, long Account1, long Account2, long OtherInvestor);

    /// <summary>A portfolio whose AML review is accepted: two investors, one contract each, both selected.</summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPortfolio.AmlAcceptedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور", LegalNameEn = "Developer One" };
        db.Add(developer);
        db.AddRange(
            new MtgLkpInquiryType { Code = "CREDIT_BUREAU", NameAr = "استعلام ائتماني", NameEn = "Credit bureau" },
            new MtgLkpInquiryType { Code = "OLD", NameAr = "قديم", NameEn = "Old", IsActive = false });
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var investor1 = new MtgInvestor { InvestorCode = "INV1", FullNameAr = "مستثمر ١", FullNameEn = "Investor One" };
        var investor2 = new MtgInvestor { InvestorCode = "INV2", FullNameAr = "مستثمر ٢" };
        var outsider = new MtgInvestor { InvestorCode = "INV9", FullNameAr = "مستثمر ٩" };
        db.AddRange(project, investor1, investor2, outsider);
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
        return new Seeded(database, portfolio.PortfolioId, developer.DeveloperId, investor1.InvestorId,
            account1.PortfolioAccountId, account2.PortfolioAccountId, outsider.InvestorId);
    }

    private static SaveInquiryRequest Inquiry(string subjectType, long? subjectId, string type = "credit_bureau", string? risk = "low", int? rowVersion = null) =>
        new(subjectType, subjectId, type, new DateOnly(2026, 10, 23), "CB-1", "No adverse record", risk, null, rowVersion);

    private static MtgInquiryService Service(CapitraDbContext db) => new(db);

    [Fact]
    public async Task Inquiries_are_recorded_edited_and_deleted_with_their_subject_names()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var developer = await service.CreateAsync(s.PortfolioId, Inquiry("developer", s.DeveloperId), default);
        Assert.Equal("Developer One", developer.SubjectName);
        Assert.Equal("CREDIT_BUREAU", developer.InquiryTypeCode);
        Assert.Equal("LOW", developer.RiskLevelCode);
        Assert.Equal("loader", developer.CreatedBy);

        var investor = await service.CreateAsync(s.PortfolioId, Inquiry("INVESTOR", s.Investor1, risk: "HIGH"), default);
        var edited = await service.UpdateAsync(s.PortfolioId, investor.InquiryResultId,
            Inquiry("INVESTOR", s.Investor1, risk: null, rowVersion: investor.RowVersion), default);
        Assert.Null(edited.RiskLevelCode);

        // A stale row version is refused, for edits and deletes alike.
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(s.PortfolioId, investor.InquiryResultId,
            Inquiry("INVESTOR", s.Investor1, rowVersion: investor.RowVersion), default));
        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(s.PortfolioId, investor.InquiryResultId, investor.RowVersion, default));

        await service.DeleteAsync(s.PortfolioId, edited.InquiryResultId, edited.RowVersion, default);
        var step = await service.GetAsync(s.PortfolioId, default);
        Assert.Equal(developer.InquiryResultId, Assert.Single(step.Inquiries).InquiryResultId);
        Assert.Equal(["CREDIT_BUREAU"], step.InquiryTypes.Select(t => t.Code));
        Assert.Equal(0, step.Accounts.Single(a => a.PortfolioAccountId == s.Account1).InquiryCount);
    }

    [Fact]
    public async Task Inquiries_are_validated_and_must_be_about_the_portfolio()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(s.PortfolioId,
            new SaveInquiryRequest("PARTNER", null, "OLD", null, null, null, "SEVERE", null, null), default));
        Assert.Equal(["InquiryTypeCode", "RiskLevelCode", "SubjectId", "SubjectTypeCode"], error.Errors.Keys.Order(StringComparer.Ordinal));

        error = await Assert.ThrowsAsync<BusinessValidationException>(
            () => service.CreateAsync(s.PortfolioId, Inquiry("INVESTOR", s.OtherInvestor), default));
        Assert.Equal("Selected subject does not belong to this portfolio.", error.Errors["SubjectId"][0]);

        // Guarantees have no screens yet, so any id is accepted.
        var guarantee = await service.CreateAsync(s.PortfolioId, Inquiry("GUARANTEE", 77), default);
        Assert.Equal(77, guarantee.SubjectId);
    }

    [Fact]
    public async Task Exclusion_is_final_and_approve_keeps_the_selected_totals()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        await service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account2, MtgPortfolioAccount.ExcludedDecision)]), default);
        var error = await Assert.ThrowsAsync<ConflictException>(() => service.SaveDealDecisionsAsync(s.PortfolioId,
            new SaveDealDecisionsRequest([new DealDecisionItem(s.Account2, MtgPortfolioAccount.SelectedDecision)]), default));
        Assert.Equal("Excluded investors cannot be selected again.", error.Message);

        var step = await service.DecideAsync(s.PortfolioId, new InquiryDecisionRequest("approve", null), default);
        Assert.Equal(MtgPortfolio.InquiryAcceptedStatus, step.PortfolioStatusCode);
        Assert.False(step.IsOpen);

        var portfolio = await db.Set<MtgPortfolio>().AsNoTracking().SingleAsync();
        Assert.Equal(1, portfolio.SelectedAccountCount);
        Assert.Equal(1000m, portfolio.SelectedFaceValueAmount);
        Assert.Equal(800m, portfolio.SelectedOutstandingAmount);
        Assert.Equal(200m, portfolio.SelectedPaidAmount);
        Assert.Equal("Portfolio Inquiry workflow task completed.", (await db.Set<MtgPortfolioStatusHistory>().SingleAsync()).Comments);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(s.PortfolioId, Inquiry("DEVELOPER", s.DeveloperId), default));
    }

    [Fact]
    public async Task Reject_leaves_the_step_open_and_it_is_closed_before_aml_is_accepted()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        await Assert.ThrowsAsync<BusinessValidationException>(
            () => Service(db).DecideAsync(s.PortfolioId, new InquiryDecisionRequest("MAYBE", null), default));
        var step = await Service(db).DecideAsync(s.PortfolioId, new InquiryDecisionRequest("REJECT", "  "), default);
        Assert.Equal(MtgPortfolio.InquiryRejectedStatus, step.PortfolioStatusCode);
        Assert.True(step.IsOpen);
        Assert.Equal("Portfolio Inquiry rejected.", (await db.Set<MtgPortfolioStatusHistory>().SingleAsync()).Comments);

        var early = await SeedAsync(MtgPortfolio.DataLoadingCompletedStatus);
        await using var earlyDb = StepTestDb.Create(early.Database);
        Assert.False((await Service(earlyDb).GetAsync(early.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(
            () => Service(earlyDb).CreateAsync(early.PortfolioId, Inquiry("DEVELOPER", early.DeveloperId), default));
    }
}
