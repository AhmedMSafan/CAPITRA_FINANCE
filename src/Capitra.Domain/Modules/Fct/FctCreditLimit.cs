using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_CREDIT_LIMITS: copy of FINOVA FCT_CREDIT_LIMITS (APEX), all columns kept.
/// </summary>
public class FctCreditLimit : AuditableEntity
{
    public long CreditLimitId { get; set; }

    public string LimitNumber { get; set; } = string.Empty;

    public string? LimitRef { get; set; }

    public string LimitName { get; set; } = string.Empty;

    public string LimitNameAr { get; set; } = string.Empty;

    public string LimitLevel { get; set; } = string.Empty;

    public long? ClientId { get; set; }

    public long? DebtorId { get; set; }

    public long? AgreementId { get; set; }

    public long? AgrmtDebtorId { get; set; }

    public string LimitType { get; set; } = string.Empty;

    public string? LimitTypeAr { get; set; }

    public string? LimitCategory { get; set; }

    public string? LimitCategoryAr { get; set; }

    public long CurrencyId { get; set; }

    public decimal? RequestedAmount { get; set; }

    public decimal? RecommendedAmount { get; set; }

    public decimal ApprovedAmount { get; set; }

    public decimal CurrentLimit { get; set; }

    public decimal UtilizedAmount { get; set; }

    public decimal? AvailableAmount { get; set; }

    public decimal? PendingAmount { get; set; }

    public decimal? ReservedAmount { get; set; }

    public decimal? ExcessAmount { get; set; }

    public decimal? UtilizationPct { get; set; }

    public decimal? PeakUtilization { get; set; }

    public DateOnly? PeakUtilizationDate { get; set; }

    public decimal? AvgUtilizationPct { get; set; }

    public decimal? SublimitAmount { get; set; }

    public string? SublimitType { get; set; }

    public string? SublimitTypeAr { get; set; }

    public long? ParentLimitId { get; set; }

    public string? RiskGrade { get; set; }

    public string? RiskGradeAr { get; set; }

    public decimal? RiskScore { get; set; }

    public decimal? PdPercentage { get; set; }

    public decimal? LgdPercentage { get; set; }

    public decimal? RiskWeightPct { get; set; }

    public decimal? ExpectedLoss { get; set; }

    public decimal? ProvisionAmount { get; set; }

    public decimal? ProvisionPct { get; set; }

    public decimal? AdvanceRatePct { get; set; }

    public decimal? MaxAdvanceRatePct { get; set; }

    public decimal? ConcentrationLimitPct { get; set; }

    public decimal? CurrentConcentrationPct { get; set; }

    public bool IsInsuranceRequired { get; set; }

    public decimal? InsuredAmount { get; set; }

    public decimal? InsuranceCoveragePct { get; set; }

    public string? InsurancePolicyRef { get; set; }

    public DateTime? ApplicationAt { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateTime EffectiveAt { get; set; }

    public DateTime ExpiryAt { get; set; }

    public DateOnly? LastReviewDate { get; set; }

    public DateOnly? NextReviewDate { get; set; }

    public string? ReviewFrequency { get; set; }

    public DateOnly? LastUtilizationDate { get; set; }

    public string? RequestedBy { get; set; }

    public string? RecommendedBy { get; set; }

    public DateOnly? RecommendationDate { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ApprovalAuthority { get; set; }

    public string? ApprovalAuthorityAr { get; set; }

    public string? ApprovalReference { get; set; }

    public string? ApprovalNotes { get; set; }

    public string? ApprovalNotesAr { get; set; }

    public string? LimitConditions { get; set; }

    public string? LimitConditionsAr { get; set; }

    public long StatusId { get; set; }

    public long? PreviousLimitId { get; set; }

    public int? VersionNumber { get; set; }

    public bool IsTemporary { get; set; }

    public DateOnly? TempExpiryDate { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public string? Attribute1 { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal? SpreadRate { get; set; }
}
