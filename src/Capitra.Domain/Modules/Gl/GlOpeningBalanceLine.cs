using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_OPENING_BALANCE_LINES: copy of FINOVA GL_OPENING_BALANCE_LINES (APEX), all columns kept.
/// </summary>
public class GlOpeningBalanceLine : AuditableEntity
{
    public long OpeningBalanceLineId { get; set; }

    public long OpeningBalanceRunId { get; set; }

    public long AccountCombinationId { get; set; }

    public decimal OpeningDrAmount { get; set; }

    public decimal OpeningCrAmount { get; set; }

    public decimal SourceEndingDrAmount { get; set; }

    public decimal SourceEndingCrAmount { get; set; }

    public long? JournalLineId { get; set; }

    /// <summary>One of: GENERATED, VALIDATED, POSTED, ERROR, CANCELLED.</summary>
    public string LineStatus { get; set; } = "GENERATED";

    public string? ErrorMessage { get; set; }
}
