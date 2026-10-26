using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_FINANCIAL_STUDIES: copy of FINOVA FNV_FINANCIAL_STUDY (APEX), all columns kept.
/// </summary>
public class MtgFinancialStudy : AuditableEntity
{
    public long FinStudyId { get; set; }

    public long PortfolioId { get; set; }

    public decimal PortfolioFaceValue { get; set; }

    public decimal OutstandingValue { get; set; }

    public decimal ProposedPurchaseValue { get; set; }

    public int? TermMonths { get; set; }

    public decimal ExpectedReturnAmount { get; set; }

    public decimal? ExpectedReturnRate { get; set; }

    public decimal AdminExpenseAmount { get; set; }

    public decimal NetReturnAmount { get; set; }

    public decimal ChequeTotalValue { get; set; }

    public decimal? ChequeCoveragePct { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal? DepositPct { get; set; }

    public string? FinancialComments { get; set; }

    public string? PreparedBy { get; set; }

    public DateOnly? PreparedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }
}
