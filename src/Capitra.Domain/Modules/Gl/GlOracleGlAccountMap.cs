using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_ACCOUNT_MAPS: copy of FINOVA GL_ORACLE_GL_ACCOUNT_MAP (APEX), all columns kept.
/// </summary>
public class GlOracleGlAccountMap : AuditableEntity
{
    public long AccountMapId { get; set; }

    public long ExternalSystemId { get; set; }

    public long AccountCombinationId { get; set; }

    public long? OracleCodeCombinationId { get; set; }

    public string? OracleConcatenatedAccount { get; set; }

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

    /// <summary>One of: DRAFT, VALIDATED, ERROR, INACTIVE.</summary>
    public string MappingStatus { get; set; } = "DRAFT";

    public string? ValidationMessage { get; set; }

    public bool IsActive { get; set; } = true;
}
