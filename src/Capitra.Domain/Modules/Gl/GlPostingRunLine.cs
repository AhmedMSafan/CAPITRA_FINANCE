using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_RUN_LINES: copy of FINOVA GL_POSTING_RUN_LINES (APEX), all columns kept.
/// </summary>
public class GlPostingRunLine : AuditableEntity
{
    public long PostingRunLineId { get; set; }

    public long PostingRunId { get; set; }

    public long JournalHeaderId { get; set; }

    /// <summary>One of: PENDING, POSTED, ERROR, SKIPPED.</summary>
    public string PostingStatus { get; set; } = "PENDING";

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
