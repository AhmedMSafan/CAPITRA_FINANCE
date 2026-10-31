using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_EXPORT_HEADERS: copy of FINOVA GL_ORACLE_GL_EXPORT_HEADERS (APEX), all columns kept.
/// </summary>
public class GlOracleGlExportHeader : AuditableEntity
{
    public long ExportHeaderId { get; set; }

    public long ExportBatchId { get; set; }

    public long? JournalBatchId { get; set; }

    public long JournalHeaderId { get; set; }

    public string? JournalNo { get; set; }

    public string? OracleJournalName { get; set; }

    public string? OracleJournalDescription { get; set; }

    public string? OracleJeSourceName { get; set; }

    public string? OracleJeCategoryName { get; set; }

    public DateOnly AccountingDate { get; set; }

    public string? PeriodName { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, VALID, ERROR, EXPORTED, IMPORTED, ORACLE_POSTED, REJECTED.</summary>
    public string HeaderStatus { get; set; } = "DRAFT";

    public long? OracleJeBatchId { get; set; }

    public long? OracleJeHeaderId { get; set; }

    public string? DuplicateKey { get; set; }

    public string? ErrorMessage { get; set; }
}
