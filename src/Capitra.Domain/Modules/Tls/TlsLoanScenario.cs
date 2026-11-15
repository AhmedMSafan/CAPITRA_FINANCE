using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Tls;

/// <summary>
/// TLS_LOAN_SCENARIOS: a saved Loan Calculator scenario (FINOVA_FINANCEING TOOLS > Loan Calculator, Ahmed 15 Nov 2026). A scenario
/// belongs to the user who created it (CREATED_BY); its schedule is calculated when it is saved and kept with it.
/// </summary>
public class TlsLoanScenario : AuditableEntity
{
    public long ScenarioId { get; set; }

    public string ScenarioName { get; set; } = string.Empty;

    /// <summary>The scenario it was derived from, if any.</summary>
    public long? ParentScenarioId { get; set; }

    /// <summary>The rate index of Administration > Rate Maintenance that gave the rate, if one did.</summary>
    public long? RateIndexId { get; set; }

    public decimal PrincipalAmount { get; set; }

    public DateOnly ContractDate { get; set; }

    public DateOnly MaturityDate { get; set; }

    public int TenorValue { get; set; }

    /// <summary>DAY, WEEK, MONTH or YEAR.</summary>
    public string TenorUnit { get; set; } = "MONTH";

    /// <summary>COMBINED (one payment of principal and interest) or SEPARATE (own principal and interest dates).</summary>
    public string ScheduleMode { get; set; } = "SEPARATE";

    /// <summary>EQUAL_PRINCIPAL or EQUAL_INSTALLMENT.</summary>
    public string PrincipalMethod { get; set; } = "EQUAL_PRINCIPAL";

    public string InterestPaymentMethod { get; set; } = "REGULAR";

    /// <summary>ACT_ACT, ACT_360, ACT_365, 30_ACT, 30_360 or 30_365.</summary>
    public string DayCountMethod { get; set; } = "ACT_365";

    public DateOnly PrincipalPaymentStartDate { get; set; }

    public DateOnly InterestPaymentStartDate { get; set; }

    public int? PrincipalFrequencyValue { get; set; }

    public string? PrincipalFrequencyUnit { get; set; }

    public string? PrincipalDateRule { get; set; }

    public int? InterestFrequencyValue { get; set; }

    public string? InterestFrequencyUnit { get; set; }

    public string? InterestDateRule { get; set; }

    public int? CombinedFrequencyValue { get; set; }

    /// <summary>DAY, WEEK, MONTH, YEAR, EOM, NEXT_EOM, EOM_30, NEXT_EOM_30 or MATURITY.</summary>
    public string? CombinedFrequencyUnit { get; set; }

    public string? CombinedDateRule { get; set; }
}

/// <summary>TLS_LOAN_SCENARIO_RATES: a scenario's interest rate history; the first rate is in force from the contract date.</summary>
public class TlsLoanScenarioRate : AuditableEntity
{
    public long ScenarioRateId { get; set; }

    public long ScenarioId { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public decimal InterestRatePct { get; set; }

    public int SequenceNo { get; set; }
}

/// <summary>TLS_LOAN_SCENARIO_LINES: a scenario's calculated schedule.</summary>
public class TlsLoanScenarioLine : AuditableEntity
{
    public long ScheduleLineId { get; set; }

    public long ScenarioId { get; set; }

    public int InstallmentNo { get; set; }

    public DateOnly AccrualStartDate { get; set; }

    public DateOnly AccrualEndDate { get; set; }

    public DateOnly PaymentDate { get; set; }

    public decimal OpeningPrincipalAmount { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal InterestAmount { get; set; }

    public decimal TotalPaymentAmount { get; set; }

    public decimal ClosingPrincipalAmount { get; set; }

    public decimal InterestRatePct { get; set; }

    public decimal DayCountFraction { get; set; }
}
