using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_ACTION_HIST: copy of FINOVA GL_CLOSE_ACTION_HISTORY (APEX), all columns kept.
/// </summary>
public class GlCloseActionHistory : AuditableEntity
{
    public long CloseActionId { get; set; }

    public long CloseRunId { get; set; }

    public long? CloseRunTaskId { get; set; }

    public string ActionCode { get; set; } = string.Empty;

    public string? ActionName { get; set; }

    public string? ActionFromStatus { get; set; }

    public string? ActionToStatus { get; set; }

    public string ActionBy { get; set; } = string.Empty;

    public DateTime ActionAt { get; set; }

    public string? ActionReason { get; set; }

    public string? SourceModule { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }
}
