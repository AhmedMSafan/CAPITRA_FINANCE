using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES: copy of FINOVA FNV_PP_INCOME_SCHEDULE (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioIncomeSchedule : AuditableEntity
{
    public long IncomeLineId { get; set; }

    public long DisbursementId { get; set; }

    public long PortfolioId { get; set; }

    public DateOnly DueDate { get; set; }

    public int? ChequeCount { get; set; }

    public decimal? FundingRecoveryAmount { get; set; }

    public decimal? MarginAmount { get; set; }

    public decimal? FeesAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string PostingStatus { get; set; } = "PENDING";

    public string? TranSetId { get; set; }

    public DateTime? PostedOnAt { get; set; }

    public string? PostedBy { get; set; }
}
