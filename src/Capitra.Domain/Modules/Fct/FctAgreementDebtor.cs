using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_AGREEMENT_DEBTORS: copy of FINOVA FCT_AGREEMENT_DEBTORS (APEX), all columns kept.
/// </summary>
public class FctAgreementDebtor : AuditableEntity
{
    public long AgrmtDebtorId { get; set; }

    public long AgreementId { get; set; }

    public long? DebtorId { get; set; }

    public long? ClientDebtorId { get; set; }

    public long? AgrmtProductId { get; set; }

    public decimal? DebtorLimit { get; set; }

    public decimal? ApprovedLimit { get; set; }

    public decimal? UtilizedAmount { get; set; }

    public decimal? AvailableAmount { get; set; }

    public long CurrencyId { get; set; } = 1;

    public decimal? AdvanceRatePct { get; set; }

    public int? MaxInvoiceTenorDays { get; set; }

    public decimal? ConcentrationLimitPct { get; set; }

    public decimal? CurrentConcentrationPct { get; set; }

    public string? PaymentTerms { get; set; }

    public string? PaymentTermsAr { get; set; }

    public int? StandardPaymentDays { get; set; }

    public bool IsNotified { get; set; }

    public DateOnly? NotificationDate { get; set; }

    public string? NotificationMethod { get; set; }

    public bool IsAcknowledgmentReceived { get; set; }

    public DateOnly? AcknowledgmentDate { get; set; }

    public string? RiskGrade { get; set; }

    public bool IsInsuranceRequired { get; set; }

    public decimal? InsuranceCoveragePct { get; set; }

    public string? ApprovedBy { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public string? ApprovalReference { get; set; }

    public DateTime? EffectiveFromAt { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public long? StatusId { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public bool IsActive { get; set; } = true;

    public string? DebtorName { get; set; }

    public string? TaxId { get; set; }
}
