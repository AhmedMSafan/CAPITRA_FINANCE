using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_RUNS: copy of FINOVA GL_POSTING_RUNS (APEX), all columns kept.
/// </summary>
public class GlPostingRun : AuditableEntity
{
    public long PostingRunId { get; set; }

    public long LedgerId { get; set; }

    public long LedgerPeriodId { get; set; }

    public string PostingRunNo { get; set; } = string.Empty;

    /// <summary>One of: MANUAL, SCHEDULED, AUTO, REPOST, REVERSAL.</summary>
    public string PostingRunType { get; set; } = "MANUAL";

    /// <summary>One of: NEW, RUNNING, COMPLETED, COMPLETED_WITH_ERRORS, FAILED, CANCELLED.</summary>
    public string RunStatus { get; set; } = "NEW";

    public long? SourceId { get; set; }

    public long? SubmittedBatchId { get; set; }

    public string? StartedBy { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal TotalHeaders { get; set; }

    public int PostedHeaders { get; set; }

    public int ErrorHeaders { get; set; }

    public decimal TotalLines { get; set; }

    public decimal TotalAccountedDr { get; set; }

    public decimal TotalAccountedCr { get; set; }

    public string? ErrorMessage { get; set; }

    public long? PostingProfileId { get; set; }

    public long? QueueId { get; set; }

    public string? BalanceUpdateMode { get; set; } = "STANDARD";

    public string? ValidationStatus { get; set; } = "NOT_VALIDATED";
}
