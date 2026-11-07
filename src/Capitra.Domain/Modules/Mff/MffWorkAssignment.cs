using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_WORK_ASSIGNMENTS: copy of FINOVA MFF_WORK_ASSIGNMENTS (APEX), all columns kept.
/// </summary>
public class MffWorkAssignment : AuditableEntity
{
    public long AssignmentId { get; set; }

    public long ApplicationId { get; set; }

    public string AssignmentType { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public string? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string AssignmentStatus { get; set; } = "OPEN";

    public string? Notes { get; set; }
}
