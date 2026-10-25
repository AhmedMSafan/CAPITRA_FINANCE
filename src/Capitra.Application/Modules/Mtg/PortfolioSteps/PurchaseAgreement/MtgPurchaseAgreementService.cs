using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.PurchaseAgreement;

/// <summary>Every figure the draft purchase agreement fills in, with its two annexes.</summary>
public sealed record PurchaseAgreementDto(
    long PortfolioId,
    string PortfolioNo,
    string DeveloperNameEn, // falls back to the Arabic name
    string DeveloperNameAr,
    string ProjectNameEn, // falls back to the Arabic name
    string ProjectNameAr,
    string ProjectCode,
    string CurrencyCode,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    bool IsBoardApproved,
    decimal ProposedPurchaseAmount,
    decimal? PvRate,
    decimal? ReservedAmountPct,
    decimal? LateFeeRate,
    string? BoardDecisionNo,
    DateOnly? BoardDecisionDate,
    int ContractCount,
    int UnitCount,
    decimal OutstandingAmount,
    int ChequeCount,
    decimal ChequeAmount,
    decimal? AdminDeductedAmount, // null until the financial review is saved
    decimal? LegalDeductedAmount,
    decimal? ValuationDeductedAmount,
    IReadOnlyList<AgreementContractDto> Contracts, // annex A
    IReadOnlyList<AgreementChequeDto> Cheques); // annex B

public sealed record AgreementContractDto(string InvestorNameEn, string InvestorNameAr, string? SellerContractNo, string? UnitCode, decimal OutstandingAmount);

public sealed record AgreementChequeDto(string InvestorName, string ChequeNo, string? BankName, DateOnly DueDate, decimal ChequeAmount);

public interface IMtgPurchaseAgreementService
{
    Task<PurchaseAgreementDto> GetAsync(long portfolioId, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Portfolio Purchase Agreement" (APEX App 105 page 216): the bilingual draft agreement for legal review,
/// filled in from the portfolio, the board resolution and the financial review, with the kept contracts (annex A) and
/// their cheques (annex B). APEX only displays and prints it; the step itself is completed from the workflow task.
/// </summary>
public sealed class MtgPurchaseAgreementService(ICapitraDbContext db) : IMtgPurchaseAgreementService
{
    public async Task<PurchaseAgreementDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from po in db.Set<MtgPurchasedPortfolio>()
            join d in db.Set<MtgDeveloper>() on po.DeveloperId equals d.DeveloperId
            join prj in db.Set<MtgProject>() on po.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on po.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where po.PortfolioId == portfolioId
            select new
            {
                Portfolio = po, StatusName = s != null ? s.NameEn : po.PortfolioStatusCode,
                d.LegalNameEn, d.LegalNameAr, prj.ProjectNameEn, prj.ProjectNameAr, prj.ProjectCode
            }).AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");
        var p = header.Portfolio;

        // A contract with no deal decision counts as kept, as in APEX.
        var contracts = await (
            from a in db.Set<MtgPurchasedPortfolioAccount>()
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId into units
            from u in units.DefaultIfEmpty()
            where a.PortfolioId == portfolioId && a.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision
            orderby i.InvestorCode, a.PortfolioAccountId
            select new
            {
                a.ProjectUnitId,
                Line = new AgreementContractDto(i.FullNameEn ?? i.FullNameAr, i.FullNameAr, a.SellerContractNo, u != null ? u.UnitCode : null, a.OutstandingAmount)
            }).AsNoTracking().ToListAsync(cancellationToken);

        var cheques = await (
            from c in db.Set<MtgPurchasedPortfolioCheque>()
            join a in db.Set<MtgPurchasedPortfolioAccount>() on c.PortfolioAccountId equals a.PortfolioAccountId
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            where c.PortfolioId == portfolioId && a.DealDecisionCode != MtgPurchasedPortfolioAccount.ExcludedDecision
            orderby c.DueDate, c.ChequeId
            select new AgreementChequeDto(i.FullNameEn ?? i.FullNameAr, c.ChequeNo, c.BankName, c.DueDate, c.ChequeAmount))
            .AsNoTracking().ToListAsync(cancellationToken);

        var review = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);

        return new PurchaseAgreementDto(
            p.PortfolioId, p.PortfolioNo, header.LegalNameEn ?? header.LegalNameAr, header.LegalNameAr,
            header.ProjectNameEn ?? header.ProjectNameAr, header.ProjectNameAr, header.ProjectCode, p.CurrencyCode,
            p.PortfolioStatusCode, header.StatusName, p.BoardDecisionCode == MtgPurchasedPortfolio.BoardAccepted,
            p.ProposedPurchaseAmount, p.PvRate, p.ReservedAmountPct, p.LateFeeRate, p.BoardDecisionNo, p.BoardDecisionDate,
            contracts.Count, contracts.Select(x => x.ProjectUnitId).Distinct().Count(), contracts.Sum(x => x.Line.OutstandingAmount),
            cheques.Count, cheques.Sum(x => x.ChequeAmount),
            review is null ? null : review.IsAdminIncluded ? review.AdminFinalAmount : 0,
            review is null ? null : review.IsLegalIncluded ? review.LegalFinalAmount : 0,
            review is null ? null : review.IsValuationIncluded ? review.ValuationFinalAmount : 0,
            contracts.Select(x => x.Line).ToList(), cheques);
    }
}
