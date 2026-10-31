using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_VALIDATION_RESULTS: copy of FINOVA GL_POSTING_VALIDATION_RESULTS (APEX), all columns kept.
/// </summary>
public class GlPostingValidationResult : AuditableEntity
{
    public long PostingValidationResultId { get; set; }

    public long? PostingRunId { get; set; }

    public long? QueueId { get; set; }

    public long? JournalBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    public long? PostingValidationRuleId { get; set; }

    /// <summary>One of: PRE_QUEUE, PRE_LOCK, PRE_POST, BALANCE_UPDATE, POST_BALANCE, SOURCE_NOTIFY.</summary>
    public string ValidationStage { get; set; } = string.Empty;

    /// <summary>One of: PASSED, FAILED, WARNING, SKIPPED.</summary>
    public string ResultStatus { get; set; } = string.Empty;

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string SeverityCode { get; set; } = "ERROR";

    public string? ResultMessage { get; set; }

    public string CheckedBy { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }
}
