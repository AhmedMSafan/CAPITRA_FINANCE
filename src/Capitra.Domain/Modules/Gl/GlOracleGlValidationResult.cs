using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_VALIDATION_RESULTS: copy of FINOVA GL_ORACLE_GL_VALIDATION_RESULTS (APEX), all columns kept.
/// </summary>
public class GlOracleGlValidationResult : AuditableEntity
{
    public long ValidationResultId { get; set; }

    public long ExportBatchId { get; set; }

    public long? ExportHeaderId { get; set; }

    public long? ExportLineId { get; set; }

    public string ValidationCode { get; set; } = string.Empty;

    public string? ValidationName { get; set; }

    /// <summary>One of: INFO, WARNING, ERROR, BLOCKER.</summary>
    public string Severity { get; set; } = "ERROR";

    /// <summary>One of: PASS, FAIL, WARNING.</summary>
    public string ValidationStatus { get; set; } = string.Empty;

    public string? ValidationMessage { get; set; }
}
