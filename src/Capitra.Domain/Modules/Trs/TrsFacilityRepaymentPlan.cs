using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_FACILITY_REPAYMENT_PLANS: copy of FINOVA TRS_FACILITY_REPAYMENT_PLAN (APEX), all columns kept.
/// </summary>
public class TrsFacilityRepaymentPlan : AuditableEntity
{
    public long RepaymentPlanId { get; set; }

    public long RunoffCaseId { get; set; }

    public long FacilityId { get; set; }

    public long? DrawdownId { get; set; }

    public long? RepaymentRuleId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly ContractualDueDate { get; set; }

    public DateOnly? ManualBankCollectionDate { get; set; }

    public DateOnly BankCollectionDate { get; set; }

    public DateOnly? PeriodStartDate { get; set; }

    public DateOnly? PeriodEndDate { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal PrincipalDueAmount { get; set; }

    public decimal InterestDueAmount { get; set; }

    public decimal FeeDueAmount { get; set; }

    public decimal TotalDueAmount { get; set; }

    public decimal? ClosingBalance { get; set; }

    /// <summary>One of: GENERATED, MANUAL, REGENERATED, IMPORT.</summary>
    public string PlanSourceCode { get; set; } = "GENERATED";

    /// <summary>One of: PLANNED, ADJUSTED, COLLECTED, CANCELLED, SUPERSEDED.</summary>
    public string PlanStatusCode { get; set; } = "PLANNED";

    public string? Notes { get; set; }

    public int? DayCount { get; set; }

    public decimal? AnnualRatePct { get; set; }

    public decimal? ClientInterestRatePct { get; set; }

    public decimal? ClientInterestDueAmount { get; set; }
}
