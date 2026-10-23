using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PORTFOLIO: a mortgage portfolio being bought from a developer (APEX FNV_PURSHASED_PORTFOLIO,
/// App 105 pages 201-203). Face value and outstanding are totals of its accounts, kept by the service.
/// </summary>
public class MtgPortfolio : AuditableEntity
{
    /// <summary>Status every new portfolio starts in.</summary>
    public const string DraftStatus = "DRAFT";

    public long PortfolioId { get; set; }

    public string PortfolioNo { get; set; } = string.Empty;

    public long DeveloperId { get; set; }

    public long ProjectId { get; set; }

    public DateOnly RequestDate { get; set; }

    /// <summary>MTG_LKP_PORTFOLIO_STATUS code.</summary>
    public string PortfolioStatusCode { get; set; } = DraftStatus;

    /// <summary>ADM_LKP_CURRENCY code.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    public int? TermMonths { get; set; }

    public string? Comments { get; set; }

    public decimal FaceValueAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public decimal ProposedPurchaseAmount { get; set; }

    public decimal ExpectedReturnAmount { get; set; }

    public decimal AdminExpenseAmount { get; set; }

    public decimal? CoveragePct { get; set; }

    public decimal? ReservedAmountPct { get; set; }

    /// <summary>Present value calculation rate.</summary>
    public decimal? PvRate { get; set; }

    public decimal? LateFeeRate { get; set; }

    public decimal? MarginPct { get; set; }

    public decimal? AdminExpensePct { get; set; }

    public decimal? LegalFeePerUnitAmount { get; set; }

    public decimal? ValuationFeePerUnitAmount { get; set; }
}
