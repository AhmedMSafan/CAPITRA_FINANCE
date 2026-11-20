using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_MEMOS: copy of FINOVA FNV_PURSHASED_PORTFOLIO_MEMO (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioMemo : AuditableEntity
{
    public long MemoId { get; set; }

    public long PortfolioId { get; set; }

    public int VersionNo { get; set; } = 1;

    public DateOnly MemoDate { get; set; }

    public string? ExecutiveSummary { get; set; }

    public string? DeveloperSummary { get; set; }

    public string? ProjectSummary { get; set; }

    public string? InvestorSummary { get; set; }

    public string? FinancialSummary { get; set; }

    public string? LegalSummary { get; set; }

    public string? ValuationSummary { get; set; }

    public string? GuaranteeSummary { get; set; }

    public string? Recommendation { get; set; }

    public string? MemoStatus { get; set; } = "DRAFT";

    public string? PreparedBy { get; set; }

    public DateOnly? PreparedDate { get; set; }
}
