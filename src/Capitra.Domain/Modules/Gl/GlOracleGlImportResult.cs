using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_IMPORT_RESULTS: copy of FINOVA GL_ORACLE_GL_IMPORT_RESULTS (APEX), all columns kept.
/// </summary>
public class GlOracleGlImportResult : AuditableEntity
{
    public long ImportResultId { get; set; }

    public long ImportRequestId { get; set; }

    public long ExportBatchId { get; set; }

    public long? ExportHeaderId { get; set; }

    public long? ExportLineId { get; set; }

    public long? InterfaceRowId { get; set; }

    /// <summary>One of: BATCH, HEADER, LINE, INTERFACE_ROW.</summary>
    public string ResultLevel { get; set; } = string.Empty;

    /// <summary>One of: SUCCESS, WARNING, ERROR, REJECTED.</summary>
    public string ResultStatus { get; set; } = string.Empty;

    public long? OracleJeBatchId { get; set; }

    public long? OracleJeHeaderId { get; set; }

    public int? OracleJeLineNum { get; set; }

    public string? OraclePeriodName { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }
}
