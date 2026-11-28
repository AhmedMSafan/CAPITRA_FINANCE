using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_PROFILE_STEPS: copy of FINOVA GL_CLOSE_PROFILE_STEPS (APEX), all columns kept.
/// </summary>
public class GlCloseProfileStep : AuditableEntity
{
    public long CloseStepId { get; set; }

    public long CloseProfileId { get; set; }

    public string StepCode { get; set; } = string.Empty;

    public string StepNameEn { get; set; } = string.Empty;

    public string? StepNameAr { get; set; }

    public string StepGroupCode { get; set; } = string.Empty;

    /// <summary>One of: VALIDATION, TASK, GENERATION, APPROVAL, POSTING, RECONCILIATION, REPORT, LOCK, YEAR_END_TRANSFER, OPENING_BALANCE.</summary>
    public string StepType { get; set; } = string.Empty;

    public string? SourceModule { get; set; }

    public string? DependencyStepCode { get; set; }

    public bool IsMandatory { get; set; } = true;

    public bool IsManualCompleteAllowed { get; set; } = true;

    public bool IsAutoExecute { get; set; }

    public string? ValidationSetCode { get; set; }

    public string? ResponsibleRoleCode { get; set; }

    public string? ResponsibleUserName { get; set; }

    public int? ExpectedDurationHours { get; set; }

    public int? EscalationAfterHours { get; set; }

    public int DisplaySeq { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
