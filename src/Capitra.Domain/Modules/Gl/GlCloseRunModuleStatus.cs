using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_RUN_MODULE_STATUSES: copy of FINOVA GL_CLOSE_RUN_MODULE_STATUS (APEX), all columns kept.
/// </summary>
public class GlCloseRunModuleStatus : AuditableEntity
{
    public long RunModuleStatusId { get; set; }

    public long CloseRunId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string? SourceModuleNameEn { get; set; }

    /// <summary>One of: NOT_STARTED, IN_PROGRESS, READY, CERTIFIED, FAILED, WAIVED, NOT_APPLICABLE.</summary>
    public string ModuleStatus { get; set; } = "NOT_STARTED";

    /// <summary>One of: NOT_REQUIRED, NOT_POSTED, PARTIAL, POSTED, ERROR, WAIVED.</summary>
    public string? PostingStatus { get; set; }

    /// <summary>One of: NOT_REQUIRED, PENDING, MATCHED, VARIANCE, WAIVED.</summary>
    public string? ReconciliationStatus { get; set; }

    public decimal? TotalTransactions { get; set; }

    public int? PostedTransactions { get; set; }

    public int? UnpostedTransactions { get; set; }

    public int? ErrorTransactions { get; set; }

    public decimal? SourceControlTotal { get; set; }

    public decimal? GlControlTotal { get; set; }

    public decimal? VarianceAmount { get; set; }

    public string? CertifiedBy { get; set; }

    public DateTime? CertifiedAt { get; set; }

    public string? SourceReferenceNo { get; set; }

    public string? Notes { get; set; }
}
