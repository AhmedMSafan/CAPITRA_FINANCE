using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>
/// MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES: copy of FINOVA FNV_PP_REPAYMENT_SCHEDULE (APEX), all columns kept.
/// </summary>
public class MtgPurchasedPortfolioRepaymentSchedule : AuditableEntity
{
    public long ScheduleLineId { get; set; }

    public long DisbursementId { get; set; }

    public long PortfolioId { get; set; }

    public long PortfolioAccountId { get; set; }

    public long? InvestorId { get; set; }

    public long? ClientId { get; set; }

    public bool IsClientCreated { get; set; }

    public long? SourceChequeId { get; set; }

    public long? WalletChequeId { get; set; }

    public int? LineNo { get; set; }

    public string? ChequeNo { get; set; }

    public string? BankName { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal? ChequeAmount { get; set; }

    public decimal? PvAmount { get; set; }

    public int? DaysFromDisbursement { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal? PrincipalAmount { get; set; }

    public decimal? InterestAmount { get; set; }

    public decimal? ClosingBalance { get; set; }

    public decimal? MarginRelease { get; set; }

    public decimal? FeeRelease { get; set; }

    public decimal? EirRate { get; set; }
}
