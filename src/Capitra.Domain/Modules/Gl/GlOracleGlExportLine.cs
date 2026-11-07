using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_EXPORT_LINES: copy of FINOVA GL_ORACLE_GL_EXPORT_LINES (APEX), all columns kept.
/// </summary>
public class GlOracleGlExportLine : AuditableEntity
{
    public long ExportLineId { get; set; }

    public long ExportHeaderId { get; set; }

    public long JournalLineId { get; set; }

    public int LineNo { get; set; }

    public long AccountCombinationId { get; set; }

    public long? OracleCodeCombinationId { get; set; }

    public string? OracleConcatenatedAccount { get; set; }

    public decimal EnteredDr { get; set; }

    public decimal EnteredCr { get; set; }

    public decimal AccountedDr { get; set; }

    public decimal AccountedCr { get; set; }

    public string? LineDescription { get; set; }

    public string? Reference1 { get; set; }

    public string? Reference2 { get; set; }

    public string? Reference3 { get; set; }

    public string? Reference4 { get; set; }

    public string? Reference5 { get; set; }

    public string? Reference6 { get; set; }

    public string? Reference7 { get; set; }

    public string? Reference8 { get; set; }

    public string? Reference9 { get; set; }

    public string? Reference10 { get; set; }

    /// <summary>One of: DRAFT, VALID, ERROR, EXPORTED, IMPORTED, REJECTED.</summary>
    public string LineStatus { get; set; } = "DRAFT";

    public string? ErrorMessage { get; set; }
}
