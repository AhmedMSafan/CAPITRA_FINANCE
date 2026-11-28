using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_EXCEPTIONS: copy of FINOVA GL_POSTING_EXCEPTIONS (APEX), all columns kept.
/// </summary>
public class GlPostingException : AuditableEntity
{
    public long PostingExceptionId { get; set; }

    public long? PostingRunId { get; set; }

    public long? QueueId { get; set; }

    public long? JournalBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public long? JournalLineId { get; set; }

    public string ExceptionCode { get; set; } = string.Empty;

    /// <summary>One of: ELIGIBILITY, VALIDATION, LOCK, BALANCE, SOURCE_NOTIFY, ROLLBACK, REBUILD, POSTING.</summary>
    public string ExceptionType { get; set; } = "POSTING";

    /// <summary>One of: ERROR, WARNING, INFO.</summary>
    public string SeverityCode { get; set; } = "ERROR";

    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>One of: OPEN, ASSIGNED, IN_PROGRESS, RESOLVED, WAIVED, CANCELLED.</summary>
    public string ResolutionStatus { get; set; } = "OPEN";

    public string? AssignedTo { get; set; }

    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolutionNotes { get; set; }
}
