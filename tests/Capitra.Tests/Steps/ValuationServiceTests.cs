using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Valuation;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Valuation" (APEX page 401) against an in-memory database.</summary>
public sealed class ValuationServiceTests
{
    private sealed record Seeded(string Database, long PortfolioId, long ProjectId, long Unit1, long OtherProjectUnit);

    /// <summary>A portfolio whose legal review is accepted, its project with one unit, and another project's unit.</summary>
    private static async Task<Seeded> SeedAsync(string status = MtgPurchasedPortfolio.LegalReviewAcceptedStatus)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور", LegalNameEn = "Developer One" };
        db.Add(developer);
        db.AddRange(
            new MtgLkpValuationType { Code = "MARKET", NameAr = "سوقي", NameEn = "Market valuation" },
            new MtgLkpValuationType { Code = "OLD", NameAr = "قديم", NameEn = "Old", IsActive = false });
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع" };
        var other = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ2", ProjectNameAr = "مشروع آخر" };
        db.AddRange(project, other);
        await db.SaveChangesAsync();
        var unit1 = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-1" };
        var otherUnit = new MtgProjectUnit { ProjectId = other.ProjectId, UnitCode = "X-1" };
        var portfolio = new MtgPurchasedPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP", PortfolioStatusCode = status,
            FaceValueAmount = 4000, OutstandingAmount = 2800, SelectedFaceValueAmount = 1000, SelectedOutstandingAmount = 800
        };
        db.AddRange(unit1, otherUnit, portfolio);
        await db.SaveChangesAsync();
        return new Seeded(database, portfolio.PortfolioId, project.ProjectId, unit1.ProjectUnitId, otherUnit.ProjectUnitId);
    }

    private static SaveValuationRequest Valuation(long? unitId = null, decimal market = 5000, decimal? recommended = 4500, int? rowVersion = null) =>
        new("market", unitId, " Valu Co ", null, new DateOnly(2026, 10, 20), market, recommended, 75.5m, null, null, null, rowVersion);

    private static MtgValuationService Service(CapitraDbContext db) => new(db);

    [Fact]
    public async Task Valuations_are_recorded_edited_and_deleted_for_the_project_or_a_unit()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var project = await service.CreateAsync(s.PortfolioId, Valuation(), default);
        Assert.Equal("MARKET", project.ValuationTypeCode);
        Assert.Equal("Market valuation", project.ValuationTypeName);
        Assert.Equal("Valu Co", project.ValuerName);
        Assert.Null(project.UnitCode);

        var unit = await service.CreateAsync(s.PortfolioId, Valuation(s.Unit1, recommended: null), default);
        Assert.Equal("U-1", unit.UnitCode);
        Assert.Equal(s.ProjectId, (await db.Set<MtgValuation>().SingleAsync(x => x.ValuationId == unit.ValuationId)).ProjectId);

        var edited = await service.UpdateAsync(s.PortfolioId, unit.ValuationId, Valuation(s.Unit1, recommended: 100, rowVersion: unit.RowVersion), default);
        Assert.Equal(100m, edited.RecommendedValueAmount);
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(s.PortfolioId, unit.ValuationId, Valuation(rowVersion: unit.RowVersion), default));
        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(s.PortfolioId, unit.ValuationId, unit.RowVersion, default));

        await service.DeleteAsync(s.PortfolioId, edited.ValuationId, edited.RowVersion, default);
        var step = await service.GetAsync(s.PortfolioId, default);
        Assert.Equal(project.ValuationId, Assert.Single(step.Valuations).ValuationId);
        Assert.Equal(["MARKET"], step.ValuationTypes.Select(t => t.Code));
        Assert.Equal(["U-1"], step.Units.Select(u => u.UnitCode));
        Assert.Equal(1000m, step.FaceValueAmount);
        Assert.Equal(800m, step.OutstandingAmount);
    }

    [Fact]
    public async Task Valuations_are_validated()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);

        var error = await Assert.ThrowsAsync<BusinessValidationException>(() => Service(db).CreateAsync(s.PortfolioId,
            new SaveValuationRequest("OLD", s.OtherProjectUnit, " ", null, null, 0, -1, 101, null, null, null, null), default));
        Assert.Equal(
            ["MarketValueAmount", "ProjectCompletionPct", "ProjectUnitId", "RecommendedValueAmount", "ValuationDate", "ValuationTypeCode", "ValuerName"],
            error.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("The selected unit does not belong to this portfolio's project.", error.Errors["ProjectUnitId"][0]);
    }

    [Fact]
    public async Task Complete_needs_a_valuation_and_closes_the_step()
    {
        var s = await SeedAsync();
        await using var db = StepTestDb.Create(s.Database);
        var service = Service(db);

        var error = await Assert.ThrowsAsync<ConflictException>(() => service.CompleteAsync(s.PortfolioId, new CompleteValuationRequest(null), default));
        Assert.Equal("Record at least one valuation before completing the valuation step.", error.Message);

        // A missing recommended value does not block completion, as in APEX.
        await service.CreateAsync(s.PortfolioId, Valuation(recommended: null), default);
        var step = await service.CompleteAsync(s.PortfolioId, new CompleteValuationRequest("  "), default);
        Assert.Equal(MtgPurchasedPortfolio.ValuationCompletedStatus, step.PortfolioStatusCode);
        Assert.False(step.IsOpen);
        Assert.Equal("Valuation step completed.", (await db.Set<MtgPurchasedPortfolioStatusHistory>().SingleAsync()).Comments);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(s.PortfolioId, Valuation(), default));
    }

    [Fact]
    public async Task The_step_is_closed_before_the_legal_review_is_accepted()
    {
        var s = await SeedAsync(MtgPurchasedPortfolio.LegalReviewRejectedStatus);
        await using var db = StepTestDb.Create(s.Database);

        Assert.False((await Service(db).GetAsync(s.PortfolioId, default)).IsOpen);
        await Assert.ThrowsAsync<ConflictException>(() => Service(db).CreateAsync(s.PortfolioId, Valuation(), default));
    }
}
