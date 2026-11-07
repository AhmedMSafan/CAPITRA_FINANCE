using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Chq;

/// <summary>
/// CHQ_UPLOAD_BATCHES: copy of FINOVA FND_CHEQUE_UPLOAD_BATCH (APEX), all columns kept.
/// </summary>
public class ChqUploadBatch : AuditableEntity
{
    public long BatchId { get; set; }

    public string BatchNo { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, LOADED, VALIDATED, HAS_ERRORS, SUBMITTED, CANCELLED.</summary>
    public string BatchStatus { get; set; } = "DRAFT";

    public string? FileName { get; set; }

    public string? MimeType { get; set; }

    public long LegalEntityId { get; set; }

    public string? CustomerId { get; set; }

    public string? CustomerNameEn { get; set; }

    public string? CustomerNameAr { get; set; }

    public string? DrawerNameEn { get; set; }

    public string? DrawerNameAr { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string SourceModule { get; set; } = "FACTORING";

    public string? SourceRefType { get; set; }

    public string? SourceRefId { get; set; }

    public DateTime ChequeAt { get; set; }

    public DateTime ReceivedAt { get; set; }

    public string? DefaultLocationCode { get; set; } = "COMPANY_VAULT";

    public string? DefaultCustodianUser { get; set; }

    public decimal? TotalRows { get; set; }

    public int? ValidRows { get; set; }

    public int? ErrorRows { get; set; }

    public int? SubmittedRows { get; set; }

    public string? Notes { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    /// <summary>The GL transaction set posted when the batch was submitted to the wallet (CAP-CHW-...).</summary>
    public string? GlTranSetId { get; set; }
}
