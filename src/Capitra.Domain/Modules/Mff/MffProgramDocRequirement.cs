using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROGRAM_DOC_REQUIREMENTS: copy of FINOVA MFF_PROGRAM_DOC_REQUIREMENTS (APEX), all columns kept.
/// </summary>
public class MffProgramDocRequirement : AuditableEntity
{
    public long RequirementId { get; set; }

    public long ProgramVersionId { get; set; }

    public string DocumentTypeCode { get; set; } = string.Empty;

    public string? PartyRoleCode { get; set; }

    public string? EmploymentTypeCode { get; set; }

    public bool IsRequired { get; set; } = true;

    public int? ValidityDays { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? NotesAr { get; set; }
}
