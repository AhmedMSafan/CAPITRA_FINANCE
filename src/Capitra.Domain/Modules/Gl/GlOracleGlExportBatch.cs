using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_EXPORT_BATCHES: copy of FINOVA GL_ORACLE_GL_EXPORT_BATCHES (APEX), all columns kept.
/// </summary>
public class GlOracleGlExportBatch : AuditableEntity
{
    public long ExportBatchId { get; set; }

    public long ExternalSystemId { get; set; }

    public long ExportProfileId { get; set; }

    public string ExportBatchNo { get; set; } = string.Empty;

    /// <summary>One of: JOURNAL_EXPORT, REVERSAL_EXPORT, REPROCESS_EXPORT.</summary>
    public string ExportType { get; set; } = "JOURNAL_EXPORT";

    /// <summary>One of: DRAFT, VALIDATING, VALIDATED, EXPORT_READY, EXPORTED, IMPORT_SUBMITTED, IMPORTED, ORACLE_POSTED, ERROR, CANCELLED, REVERSED.</summary>
    public string ExportStatus { get; set; } = "DRAFT";

    public long? GroupId { get; set; }

    public string? OracleImportRequestId { get; set; }

    public string? ExportFileName { get; set; }

    public string? ExportFileMimeType { get; set; }

    public byte[]? ExportFileBlob { get; set; }

    public decimal TotalHeaders { get; set; }

    public decimal TotalLines { get; set; }

    public decimal TotalEnteredDr { get; set; }

    public decimal TotalEnteredCr { get; set; }

    public decimal TotalAccountedDr { get; set; }

    public decimal TotalAccountedCr { get; set; }

    /// <summary>One of: NOT_VALIDATED, VALIDATING, VALID, ERROR, WARNING.</summary>
    public string ValidationStatus { get; set; } = "NOT_VALIDATED";

    public string? ErrorMessage { get; set; }

    public string? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public string? ExportedBy { get; set; }

    public DateTime? ExportedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
