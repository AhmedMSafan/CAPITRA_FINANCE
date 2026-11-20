using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_REPAYMENTS: copy of FINOVA FNV_REPAYMENTS (APEX), all columns kept.
/// </summary>
public class FctRepayment : AuditableEntity
{
    public long ScheduleId { get; set; }

    public long DisbursementId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly DueDate { get; set; }

    public DateTime? AdjustedDueAt { get; set; }

    public decimal? OpeningPrincipal { get; set; }

    public decimal? OutstandingPrincipal { get; set; }

    public decimal? OutstandingInterest { get; set; }

    public decimal? TotalInstallment { get; set; }

    public decimal? ClosingPrincipal { get; set; }

    public decimal? PaidPrincipal { get; set; }

    public decimal? PaidInterest { get; set; }

    public decimal? PaidFee { get; set; }

    public decimal? PaidPenalty { get; set; }

    public decimal? PrincipalDue { get; set; }

    public decimal? InterestDue { get; set; }

    public decimal? PenaltyDue { get; set; }

    public decimal? FeeDue { get; set; }

    public decimal? InterestRate { get; set; }

    public long? IndexRateId { get; set; }

    public int? DayCount { get; set; }

    public string? DayCountBasis { get; set; }

    public DateTime? LastPaymentAt { get; set; }

    public int? DaysOverdue { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public int? ScheduleVersion { get; set; }

    public bool IsCurrentVersion { get; set; }

    public string? RecalcReason { get; set; }
}
