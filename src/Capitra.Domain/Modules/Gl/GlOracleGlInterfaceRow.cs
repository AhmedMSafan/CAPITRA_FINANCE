using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_INTERFACE_ROWS: copy of FINOVA GL_ORACLE_GL_INTERFACE_ROWS (APEX), all columns kept.
/// </summary>
public class GlOracleGlInterfaceRow : AuditableEntity
{
    public long InterfaceRowId { get; set; }

    public long ExportBatchId { get; set; }

    public long ExportLineId { get; set; }

    public long? GroupId { get; set; }

    /// <summary>One of: NEW, VALID, ERROR, EXPORTED, IMPORT_SUBMITTED, IMPORTED, REJECTED.</summary>
    public string InterfaceStatus { get; set; } = "NEW";

    public string? OracleStatus { get; set; }

    public long? LedgerId { get; set; }

    public string? LedgerName { get; set; }

    public long? SetOfBooksId { get; set; }

    public DateOnly AccountingDate { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public bool IsActual { get; set; }

    public string UserJeSourceName { get; set; } = string.Empty;

    public string UserJeCategoryName { get; set; } = string.Empty;

    public decimal? EnteredDr { get; set; }

    public decimal? EnteredCr { get; set; }

    public decimal? AccountedDr { get; set; }

    public decimal? AccountedCr { get; set; }

    public long? CodeCombinationId { get; set; }

    public string? Segment1 { get; set; }

    public string? Segment2 { get; set; }

    public string? Segment3 { get; set; }

    public string? Segment4 { get; set; }

    public string? Segment5 { get; set; }

    public string? Segment6 { get; set; }

    public string? Segment7 { get; set; }

    public string? Segment8 { get; set; }

    public string? Segment9 { get; set; }

    public string? Segment10 { get; set; }

    public string? Segment11 { get; set; }

    public string? Segment12 { get; set; }

    public string? Segment13 { get; set; }

    public string? Segment14 { get; set; }

    public string? Segment15 { get; set; }

    public string? Segment16 { get; set; }

    public string? Segment17 { get; set; }

    public string? Segment18 { get; set; }

    public string? Segment19 { get; set; }

    public string? Segment20 { get; set; }

    public string? Segment21 { get; set; }

    public string? Segment22 { get; set; }

    public string? Segment23 { get; set; }

    public string? Segment24 { get; set; }

    public string? Segment25 { get; set; }

    public string? Segment26 { get; set; }

    public string? Segment27 { get; set; }

    public string? Segment28 { get; set; }

    public string? Segment29 { get; set; }

    public string? Segment30 { get; set; }

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

    public string? AttributePayload { get; set; }

    public long? OracleInterfaceId { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ExportedAt { get; set; }

    public DateTime? ImportedAt { get; set; }
}
