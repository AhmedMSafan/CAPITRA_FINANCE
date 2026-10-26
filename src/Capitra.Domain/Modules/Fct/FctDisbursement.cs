using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_DISBURSEMENTS: copy of FINOVA FCT_DISBURSEMENTS (APEX), all columns kept.
/// </summary>
public class FctDisbursement : AuditableEntity
{
    public long DisbursementId { get; set; }

    public string DisbursementNumber { get; set; } = string.Empty;

    public string? DisbursementRef { get; set; }

    public DateOnly DisbursementDate { get; set; }

    public long? FundingRequestId { get; set; }

    public long ClientId { get; set; }

    public long? AgreementId { get; set; }

    public string? DisbursementType { get; set; }

    public string? DisbursementTypeAr { get; set; }

    public long CurrencyId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal? FeeDeducted { get; set; }

    public decimal? InterestDeducted { get; set; }

    public decimal? TaxDeducted { get; set; }

    public decimal? OtherDeductions { get; set; }

    public decimal? NetAmount { get; set; }

    public long? BaseCurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal? BaseNetAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentMethodAr { get; set; }

    public string? PaymentChannel { get; set; }

    public string? PaymentChannelAr { get; set; }

    public long? BankAccountId { get; set; }

    public string? BeneficiaryName { get; set; }

    public string? BeneficiaryBank { get; set; }

    public string? BeneficiaryIban { get; set; }

    public string? BeneficiarySwift { get; set; }

    public string? BeneficiaryAccount { get; set; }

    public string? RemittingBank { get; set; }

    public string? RemittingAccount { get; set; }

    public string? RemittingSwift { get; set; }

    public DateOnly? ValueDate { get; set; }

    public DateOnly? ExecutionDate { get; set; }

    public DateTime? ExecutionTimeAt { get; set; }

    public string? PaymentReference { get; set; }

    public string? BankReference { get; set; }

    public string? SwiftReference { get; set; }

    public string? TransactionId { get; set; }

    public bool IsConfirmed { get; set; }

    public DateOnly? ConfirmationDate { get; set; }

    public string? ConfirmationRef { get; set; }

    public string? ConfirmedBy { get; set; }

    public bool IsReconciled { get; set; }

    public DateOnly? ReconciliationDate { get; set; }

    public string? ReconciliationRef { get; set; }

    public string? ReconciledBy { get; set; }

    public bool IsReversed { get; set; }

    public DateOnly? ReversalDate { get; set; }

    public string? ReversalReason { get; set; }

    public string? ReversalReasonAr { get; set; }

    public string? ReversedBy { get; set; }

    public string? ReversalRef { get; set; }

    public long? OriginalDisbursementId { get; set; }

    public bool IsGlPosted { get; set; }

    public DateOnly? GlPostingDate { get; set; }

    public string? GlJournalRef { get; set; }

    public string? ProcessedBy { get; set; }

    public string? AuthorizedBy { get; set; }

    public DateTime? AuthorizationAt { get; set; }

    public string? SecondAuthorizedBy { get; set; }

    public DateTime? SecondAuthAt { get; set; }

    public long? StatusId { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentStatusAr { get; set; }

    public string? FailureReason { get; set; }

    public string? FailureReasonAr { get; set; }

    public int? RetryCount { get; set; }

    public DateTime? LastRetryAt { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public bool IsActive { get; set; } = true;

    public long? LimitId { get; set; }

    public DateOnly? RepaymentStartDate { get; set; }

    public int? TenorMonth { get; set; }

    public decimal? IndexRate { get; set; }

    public decimal? SpreadRate { get; set; }

    public string? BaseDayCount { get; set; }

    public string? PrincipalRepaymentScheduleType { get; set; }

    public string? PrincipalRepaymentMethod { get; set; }

    public string? PrincipalDistributionMethod { get; set; }

    public string? InterestRepaymentMethod { get; set; }

    public string? StatusCode { get; set; }

    public decimal? AdvanceRatePct { get; set; }

    public long? ProductId { get; set; }

    public long? LegalEntityId { get; set; }

    public decimal ApprovedInvoiceAmount { get; set; }

    public decimal ApprovedAdvanceAmount { get; set; }

    public DateTime? ProcessingAt { get; set; }

    public string? SourceFundType { get; set; }

    public long? FacilityId { get; set; }

    public bool IsProcessed { get; set; }

    public long? SourceBankAccountId { get; set; }

    public long? FundingMappingId { get; set; }

    public long? TrancheId { get; set; }

    public long? SublimitId { get; set; }

    public long? FacilityPolicyCheckId { get; set; }

    /// <summary>One of: NONE, POLICY_HOLD, RESERVED, DRAWN, RELEASED, REVERSED.</summary>
    public string FacilityReservationStatus { get; set; } = "NONE";

    public decimal? FacilityReservedAmount { get; set; }

    public string? FacilityReservationKey { get; set; }

    public DateTime? FacilityReservedAt { get; set; }

    public long? TreasuryDrawdownId { get; set; }

    public long? PolicyBusinessLineMemberId { get; set; }

    public long? PolicyProductMemberId { get; set; }

    public long? PolicyActivityMemberId { get; set; }

    public long? PolicyAssetMemberId { get; set; }

    public long? PolicyClientSegmentMemberId { get; set; }

    public long? FacilityRateId { get; set; }

    public decimal? FacilityInterestRatePct { get; set; }

    /// <summary>One of: NOT_REQUIRED, PENDING, GENERATED, FAILED, REVERSED.</summary>
    public string BankScheduleStatus { get; set; } = "NOT_REQUIRED";

    public int? BankScheduleInstallmentCount { get; set; }

    public decimal? BankScheduleTotalAmount { get; set; }
}
