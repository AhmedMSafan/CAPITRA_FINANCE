using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACCRUAL_LINES: copy of FINOVA GL_CLOSE_ACCRUAL_LINES (APEX), all columns kept.
/// </summary>
public class GlCloseAccrualLine : AuditableEntity
{
    public long AccrualLineId { get; set; }

    public long AccrualRunId { get; set; }

    public long? AccrualRuleLineId { get; set; }

    public int LineNo { get; set; }

    /// <summary>One of: DR, CR.</summary>
    public string DrCrFlag { get; set; } = string.Empty;

    public long? AccountCombinationId { get; set; }

    public string? EnteredCurrencyCode { get; set; }

    public decimal? EnteredAmount { get; set; }

    public decimal? AccountedAmount { get; set; }

    public string? SourceModule { get; set; }

    public string? SourceReferenceType { get; set; }

    public long? SourceReferenceId { get; set; }

    public string? SourceReferenceNo { get; set; }

    public string? AmountSourceCode { get; set; }

    public decimal? AmountSourceValue { get; set; }

    public bool IsGenerated { get; set; } = true;

    /// <summary>One of: GENERATED, VALIDATED, ERROR, POSTED, REVERSED, CANCELLED.</summary>
    public string LineStatus { get; set; } = "GENERATED";

    public string? ErrorMessage { get; set; }

    public string? LineDescription { get; set; }

    public long? JournalLineId { get; set; }
}
