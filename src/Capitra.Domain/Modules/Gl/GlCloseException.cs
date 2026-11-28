using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_EXCEPTIONS: copy of FINOVA GL_CLOSE_EXCEPTIONS (APEX), all columns kept.
/// </summary>
public class GlCloseException : AuditableEntity
{
    public long CloseExceptionId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseRunTaskId { get; set; }

    public long? ValidationResultId { get; set; }

    public string ExceptionCode { get; set; } = string.Empty;

    public string ExceptionTitle { get; set; } = string.Empty;

    public string? ExceptionDescription { get; set; }

    public string? SourceModule { get; set; }

    /// <summary>One of: INFO, WARNING, ERROR, BLOCKER.</summary>
    public string Severity { get; set; } = "ERROR";

    /// <summary>One of: OPEN, IN_PROGRESS, RESOLVED, WAIVED, CANCELLED.</summary>
    public string ExceptionStatus { get; set; } = "OPEN";

    public bool IsBlocking { get; set; } = true;

    public string? AssignedTo { get; set; }

    public DateTime? TargetResolutionAt { get; set; }

    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolutionNotes { get; set; }

    public bool IsWaiver { get; set; }

    public string? WaiverApprovedBy { get; set; }

    public DateTime? WaiverApprovedAt { get; set; }

    public string? WaiverReason { get; set; }
}
