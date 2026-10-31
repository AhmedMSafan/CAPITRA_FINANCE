using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_FX_REVALUATION_LINES: copy of FINOVA GL_FX_REVALUATION_LINES (APEX), all columns kept.
/// </summary>
public class GlFxRevaluationLine : AuditableEntity
{
    public long RevaluationLineId { get; set; }

    public long RevaluationRunId { get; set; }

    public int LineNo { get; set; }

    public long AccountCombinationId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal EnteredBalance { get; set; }

    public decimal AccountedBalance { get; set; }

    public decimal PeriodEndRate { get; set; }

    public decimal RevaluedBalance { get; set; }

    public decimal GainLossAmount { get; set; }

    public string? GainLossType { get; set; }

    public long? GainLossAccountCombId { get; set; }

    public string? SourceReferenceType { get; set; }

    public string? SourceReferenceId { get; set; }

    /// <summary>One of: CALCULATED, APPROVED, POSTED, ERROR, SKIPPED.</summary>
    public string LineStatus { get; set; } = "CALCULATED";

    public string? ErrorMessage { get; set; }
}
