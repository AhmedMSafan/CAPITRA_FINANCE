using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_RECONCILIATIONS: copy of FINOVA GL_ORACLE_GL_RECONCILIATIONS (APEX), all columns kept.
/// </summary>
public class GlOracleGlReconciliation : AuditableEntity
{
    public long ReconciliationId { get; set; }

    public long ExportBatchId { get; set; }

    /// <summary>One of: DRAFT, MATCHED, DIFFERENCE, ERROR, WAIVED.</summary>
    public string ReconciliationStatus { get; set; } = "DRAFT";

    public decimal FinovaTotalDr { get; set; }

    public decimal FinovaTotalCr { get; set; }

    public decimal OracleImportedDr { get; set; }

    public decimal OracleImportedCr { get; set; }

    public decimal DifferenceDr { get; set; }

    public decimal DifferenceCr { get; set; }

    public string? ReconciledBy { get; set; }

    public DateTime? ReconciledAt { get; set; }

    public string? Notes { get; set; }
}
