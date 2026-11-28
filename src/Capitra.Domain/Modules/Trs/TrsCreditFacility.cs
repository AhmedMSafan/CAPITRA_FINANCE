using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_CREDIT_FACILITIES: copy of FINOVA TRS_CREDIT_FACILITIES (APEX), all columns kept.
/// </summary>
public class TrsCreditFacility : AuditableEntity
{
    public long FacilityId { get; set; }

    public long CompanyId { get; set; }

    public string FacilityRef { get; set; } = string.Empty;

    public long BankId { get; set; }

    public string FacilityTypeCode { get; set; } = string.Empty;

    public string CurrencyCode { get; set; } = "EGP";

    public decimal ApprovedLimit { get; set; }

    public decimal OutstandingBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    /// <summary>One of: FIXED, FLOATING.</summary>
    public string InterestRateType { get; set; } = string.Empty;

    public string? BaseRateCode { get; set; }

    public decimal? MarginBps { get; set; }

    public decimal? AllInRatePct { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly MaturityDate { get; set; }

    public int? GracePeriodMonths { get; set; }

    public string? AmortizationType { get; set; }

    public string? SecurityDescriptionEn { get; set; }

    public string? SecurityDescriptionAr { get; set; }

    /// <summary>One of: TERM_SHEET, ACTIVE, EXPIRED, UNDER_RENEWAL, SUSPENDED, CLOSED.</summary>
    public string FacilityStatus { get; set; } = "ACTIVE";

    public string? FraRegistrationRef { get; set; }

    public long? TermSheetDocId { get; set; }

    public long? SignedContractDocId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsRevolving { get; set; } = true;

    public decimal NfaUtilizedAmount { get; set; }

    /// <summary>One of: ACTUAL_360, ACTUAL_365, THIRTY_360.</summary>
    public string InterestDayBasisCode { get; set; } = "ACTUAL_360";

    public string? RepaymentFrequencyCode { get; set; }

    /// <summary>One of: MANUAL_BANK_DATE, DUE_DATE, PREVIOUS_WORKING_DAY, NEXT_WORKING_DAY.</summary>
    public string BankCollectionPolicyCode { get; set; } = "MANUAL_BANK_DATE";

    /// <summary>One of: DAILY, MONTHLY, DAILY_MONTHLY_JE.</summary>
    public string AccrualFrequencyCode { get; set; } = "DAILY_MONTHLY_JE";

    /// <summary>One of: POST_IMMEDIATELY, STORE_ONLY, POST_ON_APPROVAL.</summary>
    public string AdjustmentPostingModeCode { get; set; } = "POST_IMMEDIATELY";

    public decimal ReservedBalance { get; set; }
}
