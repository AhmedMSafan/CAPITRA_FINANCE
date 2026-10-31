using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_AGREEMENTS: copy of FINOVA FCT_AGREEMENTS (APEX), all columns kept.
/// </summary>
public class FctAgreement : AuditableEntity
{
    public long AgreementId { get; set; }

    public string AgreementNumber { get; set; } = string.Empty;

    public string? AgreementRef { get; set; }

    public string? AgreementName { get; set; }

    public string? AgreementNameAr { get; set; }

    public long ClientId { get; set; }

    public string? AgreementType { get; set; }

    public string? AgreementTypeAr { get; set; }

    public string FactoringType { get; set; } = string.Empty;

    public string? FactoringTypeAr { get; set; }

    public string RecourseType { get; set; } = string.Empty;

    public string? RecourseTypeAr { get; set; }

    public string? NotificationType { get; set; }

    public string? NotificationTypeAr { get; set; }

    public string? ProductCode { get; set; }

    public long CurrencyId { get; set; }

    public decimal FacilityLimit { get; set; }

    public decimal? ApprovedLimit { get; set; }

    public decimal? UtilizedAmount { get; set; }

    public decimal? AvailableAmount { get; set; }

    public decimal? SublimitAmount { get; set; }

    public decimal AdvanceRatePct { get; set; }

    public decimal? MaxAdvanceRatePct { get; set; }

    public decimal? MinAdvanceRatePct { get; set; }

    public decimal? ReserveRatePct { get; set; }

    public long? RateTypeId { get; set; }

    public decimal? BaseRatePct { get; set; }

    public decimal? SpreadPct { get; set; }

    public decimal? EffectiveRatePct { get; set; }

    public decimal? MarginPct { get; set; }

    public decimal? PenaltyRatePct { get; set; }

    public string? DayCountConvention { get; set; } = "ACT/360";

    public string? InterestCalcMethod { get; set; } = "DISCOUNT";

    public string? InterestFreq { get; set; } = "MONTHLY";

    public DateTime? ApplicationAt { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateOnly? SigningDate { get; set; }

    public DateTime? EffectiveAt { get; set; }

    public DateTime? MaturityAt { get; set; }

    public DateOnly? FirstUtilizationDate { get; set; }

    public DateOnly? LastUtilizationDate { get; set; }

    public DateOnly? RenewalDate { get; set; }

    public DateOnly? NextReviewDate { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public int? TenorMonths { get; set; }

    public int? MaxInvoiceTenorDays { get; set; }

    public int? MinInvoiceTenorDays { get; set; }

    public int? GracePeriodDays { get; set; }

    public decimal? MaxDebtorConcentrationPct { get; set; }

    public decimal? MaxSingleInvoicePct { get; set; }

    public decimal? MaxSingleInvoiceAmt { get; set; }

    public decimal? MinInvoiceAmount { get; set; }

    public decimal? MaxDilutionPct { get; set; }

    public decimal? DilutionReservePct { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ApprovalAuthority { get; set; }

    public string? ApprovalAuthorityAr { get; set; }

    public string? ApprovalReference { get; set; }

    public long? StatusId { get; set; }

    public long? PreviousAgreementId { get; set; }

    public int? VersionNumber { get; set; }

    public string? RelationshipManager { get; set; }

    public long? RmEmployeeId { get; set; }

    public string? OperationsOfficer { get; set; }

    public long? OpsEmployeeId { get; set; }

    public string? GoverningLaw { get; set; }

    public string? GoverningLawAr { get; set; }

    public string? Jurisdiction { get; set; }

    public string? JurisdictionAr { get; set; }

    public string? SpecialConditions { get; set; }

    public string? SpecialConditionsAr { get; set; }

    public string? Notes { get; set; }

    public string? NotesAr { get; set; }

    public string? Attribute1 { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public bool IsActive { get; set; } = true;

    public string? StatusCode { get; set; }

    public DateTime? InvestigationReportAt { get; set; }

    public decimal? CreditBureauScore { get; set; }

    public string? InvestigationNotes { get; set; }

    public string? AmlRiskBand { get; set; }

    public DateTime? AmlScreeningReportAt { get; set; }

    public string? AmlScreeningResult { get; set; }

    public string? AmlScreeningNotes { get; set; }

    public DateTime? LegalOpenionReportAt { get; set; }

    public string? LegalOpenionNotes { get; set; }

    public long? FsAssessmentId { get; set; }

    public DateOnly? FsAssessmentDate { get; set; }

    public string? FsAssessmentResult { get; set; }

    public long? RrAssessmentId { get; set; }

    public DateOnly? RrAssessmentDate { get; set; }

    public string? RrAssessmentResult { get; set; }

    public decimal? RequiredLimit { get; set; }

    public decimal? RecommendedLimt { get; set; }

    public decimal? RecommendedAdvRate { get; set; }

    public int? RecommendedTenorMth { get; set; }

    public string? CreditNotes { get; set; }

    public DateOnly? CreditMemoDate { get; set; }

    public string? RiskNotes { get; set; }

    public DateOnly? RiskMemoDate { get; set; }

    public string? RiskDecision { get; set; }

    public decimal? ApprovedAdvRate { get; set; }

    public int? ApprovedTenorMth { get; set; }

    public string? FsNotes { get; set; }

    public string? RrNotes { get; set; }

    public long? LimitId { get; set; }

    public DateOnly? ContractStartDate { get; set; }

    public DateOnly? ContractEndDate { get; set; }

    public DateOnly? ConstractSignoffDate { get; set; }

    public string? AdditionalTermsConditions { get; set; }

    public string? CurrentStep { get; set; }

    public long InstanceId { get; set; }

    public long TaskId { get; set; }
}
