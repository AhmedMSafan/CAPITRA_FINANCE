using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_VALIDATION_RESULTS: copy of FINOVA GL_CLOSE_VALIDATION_RESULTS (APEX), all columns kept.
/// </summary>
public class GlCloseValidationResult : AuditableEntity
{
    public long ValidationResultId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseRunTaskId { get; set; }

    public long? ValidationRuleId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string? SourceModule { get; set; }

    /// <summary>One of: NOT_RUN, PASSED, WARNING, FAILED, BLOCKED, WAIVED, ERROR.</summary>
    public string ResultStatus { get; set; } = "NOT_RUN";

    /// <summary>One of: INFO, WARNING, ERROR, BLOCKER.</summary>
    public string Severity { get; set; } = "ERROR";

    public bool IsBlocking { get; set; } = true;

    public int? ResultCount { get; set; }

    public decimal? ResultAmount { get; set; }

    public decimal? ThresholdAmount { get; set; }

    public decimal? VarianceAmount { get; set; }

    public string? ResultMessage { get; set; }

    public string? DetailReferenceType { get; set; }

    public long? DetailReferenceId { get; set; }

    public string? DetailReferenceNo { get; set; }

    public string? ExecutedBy { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolutionNotes { get; set; }
}
