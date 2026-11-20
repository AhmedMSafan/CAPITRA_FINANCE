using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Fct;

/// <summary>
/// FCT_INVOICE_BATCHES: copy of FINOVA FCT_INVOICE_BATCHES (APEX), all columns kept.
/// </summary>
public class FctInvoiceBatch : AuditableEntity
{
    public long BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public string? BatchRef { get; set; }

    public string? BatchName { get; set; }

    public string? BatchNameAr { get; set; }

    public long ClientId { get; set; }

    public long? AgreementId { get; set; }

    public DateOnly BatchDate { get; set; }

    public DateTime? SubmissionAt { get; set; }

    public string? SubmissionMethod { get; set; }

    public string? SubmissionMethodAr { get; set; }

    public string? BatchType { get; set; }

    public string? BatchTypeAr { get; set; }

    public long? CurrencyId { get; set; }

    public long? TotalInvoices { get; set; }

    public decimal? TotalGrossAmount { get; set; }

    public decimal? TotalNetAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public long? ValidatedInvoices { get; set; }

    public long? RejectedInvoices { get; set; }

    public long? ApprovedInvoices { get; set; }

    public long? FundedInvoices { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal? RejectedAmount { get; set; }

    public DateTime? ProcessingStartAt { get; set; }

    public DateTime? ProcessingEndAt { get; set; }

    public long? ProcessingDurationSec { get; set; }

    public long? ErrorCount { get; set; }

    public long? WarningCount { get; set; }

    public string? UploadFileName { get; set; }

    public int? UploadFileSize { get; set; }

    public string? UploadFileType { get; set; }

    public byte[]? UploadFileContent { get; set; }

    public string? UploadTemplateVer { get; set; }

    public long? StatusId { get; set; }

    public string? SubmittedBy { get; set; }

    public string? ValidatedBy { get; set; }

    public DateTime? ValidationAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovalAt { get; set; }

    public string? Remarks { get; set; }

    public string? RemarksAr { get; set; }

    public bool IsActive { get; set; } = true;

    public long? DisbursementId { get; set; }
}
