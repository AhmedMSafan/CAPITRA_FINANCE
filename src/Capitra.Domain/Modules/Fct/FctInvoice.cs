using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_INVOICES: copy of FINOVA FCT_INVOICES (APEX), all columns kept.
/// </summary>
public class FctInvoice : AuditableEntity
{
    public long InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string? InvoiceRef { get; set; }

    public string? ClientInvoiceNumber { get; set; }

    public DateTime? ClientInvoiceAt { get; set; }

    public string? ClientPoNumber { get; set; }

    public string? ClientDoNumber { get; set; }

    public string? ClientContractRef { get; set; }

    public long? BatchId { get; set; }

    public long ClientId { get; set; }

    public long? DebtorId { get; set; }

    public long? AgreementId { get; set; }

    public long? AgrmtDebtorId { get; set; }

    public long? CreditLimitId { get; set; }

    public string? InvoiceType { get; set; }

    public string? InvoiceTypeAr { get; set; }

    public string? InvoiceCategory { get; set; }

    public string? InvoiceCategoryAr { get; set; }

    public string? FactoringType { get; set; }

    public string? FactoringTypeAr { get; set; }

    public string? RecourseType { get; set; }

    public string? RecourseTypeAr { get; set; }

    public long CurrencyId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? TaxRatePct { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public decimal? EligibleAmount { get; set; }

    public decimal? IneligibleAmount { get; set; }

    public string? IneligibleReason { get; set; }

    public string? IneligibleReasonAr { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal? AdvanceRatePct { get; set; }

    public decimal? AdvanceAmount { get; set; }

    public decimal? FundedAmount { get; set; }

    public decimal? UnfundedAmount { get; set; }

    public decimal? ReserveAmount { get; set; }

    public decimal? ReserveHeldPct { get; set; }

    public decimal? CollectedAmount { get; set; }

    public decimal? OutstandingAmount { get; set; }

    public decimal? SettledAmount { get; set; }

    public decimal? WriteOffAmount { get; set; }

    public decimal? CreditNoteAmount { get; set; }

    public decimal? DebitNoteAmount { get; set; }

    public decimal? AdjustmentAmount { get; set; }

    public decimal? DilutionAmount { get; set; }

    public decimal? DilutionPct { get; set; }

    public decimal? DisputedAmount { get; set; }

    public bool IsDisputed { get; set; }

    public DateOnly? DisputeDate { get; set; }

    public string? DisputeReason { get; set; }

    public string? DisputeReasonAr { get; set; }

    public DateTime? SubmissionAt { get; set; }

    public DateOnly? VerificationDate { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public DateOnly? FundingDate { get; set; }

    public DateTime? DueAt { get; set; }

    public DateOnly? OriginalDueDate { get; set; }

    public DateOnly? ExtendedDueDate { get; set; }

    public int? GracePeriodDays { get; set; }

    public DateOnly? EffectiveDueDate { get; set; }

    public DateOnly? FirstCollectionDate { get; set; }

    public DateOnly? LastCollectionDate { get; set; }

    public DateOnly? SettlementDate { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public int? DaysOutstanding { get; set; }

    public int? DaysPastDue { get; set; }

    public string? AgingBucket { get; set; }

    public string? AgingBucketAr { get; set; }

    public bool IsOverdue { get; set; }

    public DateOnly? OverdueDate { get; set; }

    public int? OverdueDays { get; set; }

    public decimal? InterestAmount { get; set; }

    public decimal? PenaltyAmount { get; set; }

    public decimal? FeeAmount { get; set; }

    public decimal? TotalCharges { get; set; }

    public long? BaseCurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal? BaseGrossAmount { get; set; }

    public decimal? BaseNetAmount { get; set; }

    public decimal? BaseFundedAmount { get; set; }

    public decimal? BaseOutstanding { get; set; }

    public bool IsDebtorConfirmed { get; set; }

    public DateOnly? DebtorConfirmDate { get; set; }

    public string? DebtorConfirmMethod { get; set; }

    public string? DebtorConfirmRef { get; set; }

    public string? DebtorRejectionReason { get; set; }

    public string? DebtorRejectionReasonAr { get; set; }

    public bool IsAssigned { get; set; }

    public DateOnly? AssignmentDate { get; set; }

    public string? AssignmentRef { get; set; }

    public bool IsNoticeSent { get; set; }

    public DateOnly? NoticeDate { get; set; }

    public string? NoticeMethod { get; set; }

    public bool IsNoticeAcknowledged { get; set; }

    public DateOnly? NoticeAckDate { get; set; }

    public bool IsInsured { get; set; }

    public decimal? InsuredAmount { get; set; }

    public string? InsurancePolicyRef { get; set; }

    public decimal? InsuranceCoveragePct { get; set; }

    public string? RiskGrade { get; set; }

    public decimal? RiskScore { get; set; }

    public decimal? ProvisionPct { get; set; }

    public decimal? ProvisionAmount { get; set; }

    public decimal? ExpectedLoss { get; set; }

    public bool IsRecourse { get; set; } = true;

    public DateOnly? RecourseDate { get; set; }

    public decimal? RecourseAmount { get; set; }

    public string? RecourseStatus { get; set; }

    public string? RecourseStatusAr { get; set; }

    public string? VerifiedBy { get; set; }

    public string? VerificationMethod { get; set; }

    public string? VerificationNotes { get; set; }

    public string? VerificationNotesAr { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ApprovalNotes { get; set; }

    public string? ApprovalNotesAr { get; set; }

    public string? RejectionReason { get; set; }

    public string? RejectionReasonAr { get; set; }

    public long? StatusId { get; set; }

    public long? PreviousStatusId { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public string? RiskRatingCode { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public string? Attribute6 { get; set; }

    public string? Attribute7 { get; set; }

    public string? Attribute8 { get; set; }

    public string? Attribute9 { get; set; }

    public string? Attribute10 { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public bool IsActive { get; set; } = true;

    public string? StatusCode { get; set; }

    public string? ChequeNo { get; set; }

    public long? DisbursementId { get; set; }
}
