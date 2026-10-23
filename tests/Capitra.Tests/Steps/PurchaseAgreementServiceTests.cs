using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.PurchaseAgreement;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Portfolio Purchase Agreement" (APEX page 216) against an in-memory database.</summary>
public sealed class PurchaseAgreementServiceTests
{
    /// <summary>
    /// A board-approved portfolio: investor B (two kept contracts on one unit, two cheques), investor A (one kept contract
    /// with no unit on file), and an excluded contract whose cheque must stay out of the agreement.
    /// </summary>
    private static async Task<(string Database, long PortfolioId)> SeedAsync(bool priced)
    {
        var database = Guid.NewGuid().ToString();
        await using var db = StepTestDb.Create(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور" };
        db.Add(developer);
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = "PRJ1", ProjectNameAr = "مشروع", ProjectNameEn = "Project One" };
        var a = new MtgInvestor { InvestorCode = "A", FullNameAr = "أ" };
        var b = new MtgInvestor { InvestorCode = "B", FullNameAr = "ب", FullNameEn = "Bee" };
        db.AddRange(project, a, b);
        await db.SaveChangesAsync();
        var unit = new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-1" };
        var portfolio = new MtgPortfolio
        {
            PortfolioNo = "PF-1", DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId, RequestDate = new DateOnly(2026, 10, 1),
            CurrencyCode = "EGP", PortfolioStatusCode = MtgPortfolio.BoardReviewAcceptedStatus, BoardDecisionCode = MtgPortfolio.BoardAccepted,
            BoardDecisionNo = "BR-7", BoardDecisionDate = new DateOnly(2026, 10, 22), ProposedPurchaseAmount = 925, PvRate = 12, ReservedAmountPct = 10
        };
        db.AddRange(unit, portfolio);
        await db.SaveChangesAsync();
        MtgPortfolioAccount Account(MtgInvestor investor, long unitId, string no, decimal outstanding, string? decision) => new()
        {
            PortfolioId = portfolio.PortfolioId, InvestorId = investor.InvestorId, ProjectUnitId = unitId, SellerContractNo = no,
            ContractAmount = outstanding, OutstandingAmount = outstanding, DealDecisionCode = decision
        };
        var b1 = Account(b, unit.ProjectUnitId, "C-1", 600, MtgPortfolioAccount.SelectedDecision);
        var b2 = Account(b, unit.ProjectUnitId, "C-2", 400, null);
        var a1 = Account(a, 999, "C-3", 300, MtgPortfolioAccount.SelectedDecision);
        var x = Account(a, 998, "C-4", 700, MtgPortfolioAccount.ExcludedDecision);
        db.AddRange(b1, b2, a1, x);
        await db.SaveChangesAsync();
        MtgPortfolioCheque Cheque(MtgPortfolioAccount account, string no, DateOnly due, decimal amount) => new()
        {
            PortfolioId = portfolio.PortfolioId, PortfolioAccountId = account.PortfolioAccountId, ChequeNo = no, BankName = "QNB", DueDate = due, ChequeAmount = amount
        };
        db.AddRange(
            Cheque(b1, "Q-2", new DateOnly(2027, 3, 1), 600),
            Cheque(b2, "Q-1", new DateOnly(2027, 1, 1), 400),
            Cheque(x, "Q-9", new DateOnly(2026, 12, 1), 700));
        if (priced)
            db.Add(new MtgPortfolioFinancialReview
            {
                PortfolioId = portfolio.PortfolioId, PvMethodCode = "ANNUAL", AdminFinalAmount = 10, IsAdminIncluded = true,
                LegalFinalAmount = 20, IsLegalIncluded = false, ValuationFinalAmount = 5, IsValuationIncluded = true
            });
        await db.SaveChangesAsync();
        return (database, portfolio.PortfolioId);
    }

    [Fact]
    public async Task The_agreement_covers_the_kept_contracts_and_their_cheques()
    {
        var (database, id) = await SeedAsync(priced: true);
        await using var db = StepTestDb.Create(database);

        var r = await new MtgPurchaseAgreementService(db).GetAsync(id, default);

        Assert.True(r.IsBoardApproved);
        Assert.Equal(("مطور", "مطور", "Project One", "BR-7", 925m), (r.DeveloperNameEn, r.DeveloperNameAr, r.ProjectNameEn, r.BoardDecisionNo, r.ProposedPurchaseAmount));
        Assert.Equal((3, 2, 1300m, 2, 1000m), (r.ContractCount, r.UnitCount, r.OutstandingAmount, r.ChequeCount, r.ChequeAmount));
        Assert.Equal(["C-3", "C-1", "C-2"], r.Contracts.Select(c => c.SellerContractNo)); // by investor code
        Assert.Equal((null, "Bee"), (r.Contracts[0].UnitCode, r.Contracts[1].InvestorNameEn));
        Assert.Equal("U-1", r.Contracts[1].UnitCode);
        Assert.Equal(["Q-1", "Q-2"], r.Cheques.Select(c => c.ChequeNo)); // by due date, excluded contract's cheque left out
        Assert.Equal((10m, 0m, 5m), (r.AdminDeductedAmount, r.LegalDeductedAmount, r.ValuationDeductedAmount));
    }

    [Fact]
    public async Task Charges_stay_blank_until_the_financial_review_is_saved()
    {
        var (database, id) = await SeedAsync(priced: false);
        await using var db = StepTestDb.Create(database);
        var service = new MtgPurchaseAgreementService(db);

        var r = await service.GetAsync(id, default);
        Assert.Equal((null, null, null), (r.AdminDeductedAmount, r.LegalDeductedAmount, r.ValuationDeductedAmount));
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(id + 1, default));
    }
}
