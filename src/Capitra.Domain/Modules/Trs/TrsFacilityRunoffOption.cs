using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_FACILITY_RUNOFF_OPTIONS: copy of FINOVA TRS_FACILITY_RUNOFF_OPTIONS (APEX), all columns kept.
/// </summary>
public class TrsFacilityRunoffOption : AuditableEntity
{
    public long RunoffOptionsId { get; set; }

    public long RunoffCaseId { get; set; }

    /// <summary>One of: ACTUAL_360, ACTUAL_365, THIRTY_360.</summary>
    public string InterestDayBasisCode { get; set; } = "ACTUAL_360";

    /// <summary>One of: MONTHLY, QUARTERLY, SEMI_ANNUAL, ANNUAL, AT_MATURITY, CUSTOM.</summary>
    public string RepaymentFrequencyCode { get; set; } = "MONTHLY";

    /// <summary>One of: INTEREST_ONLY_PRINCIPAL_AT_MATURITY, EQUAL_PRINCIPAL, EQUAL_INSTALLMENT, BALLOON, CUSTOM.</summary>
    public string AmortizationTypeCode { get; set; } = "INTEREST_ONLY_PRINCIPAL_AT_MATURITY";

    /// <summary>One of: MANUAL_BANK_DATE, DUE_DATE, PREVIOUS_WORKING_DAY, NEXT_WORKING_DAY.</summary>
    public string BankCollectionPolicyCode { get; set; } = "MANUAL_BANK_DATE";

    /// <summary>One of: DAILY, MONTHLY, DAILY_MONTHLY_JE.</summary>
    public string AccrualFrequencyCode { get; set; } = "DAILY_MONTHLY_JE";

    /// <summary>One of: FIXED, FLOATING, RATE_BAND, RATE_HISTORY.</summary>
    public string RateModeCode { get; set; } = "FIXED";

    /// <summary>One of: POST_IMMEDIATELY, STORE_ONLY, POST_ON_APPROVAL.</summary>
    public string AdjustmentPostingModeCode { get; set; } = "POST_IMMEDIATELY";

    public bool IsGenerateAccounting { get; set; } = true;

    public bool IsMirrorFactoringRunoff { get; set; } = true;

    public bool IsIncludeFees { get; set; } = true;

    public bool IsNoBankLateFee { get; set; } = true;

    /// <summary>One of: GTT, APEX_COLLECTION, BOTH.</summary>
    public string PreviewTargetCode { get; set; } = "BOTH";

    public string? PreviewCollectionName { get; set; } = "TRS_FACILITY_RUNOFF_PREVIEW";
}
