using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_IMPORT_REQUESTS: copy of FINOVA GL_ORACLE_GL_IMPORT_REQUESTS (APEX), all columns kept.
/// </summary>
public class GlOracleGlImportRequest : AuditableEntity
{
    public long ImportRequestId { get; set; }

    public long ExportBatchId { get; set; }

    /// <summary>One of: EBS_CONCURRENT_PROGRAM, FUSION_ESS_JOB, MANUAL_IMPORT, API_CALL.</summary>
    public string RequestMethod { get; set; } = string.Empty;

    public string? OracleRequestId { get; set; }

    public string? OracleProcessName { get; set; }

    /// <summary>One of: SUBMITTED, RUNNING, COMPLETED, WARNING, ERROR, CANCELLED.</summary>
    public string RequestStatus { get; set; } = "SUBMITTED";

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? OutputReference { get; set; }

    public string? LogReference { get; set; }

    public string? ErrorMessage { get; set; }
}
