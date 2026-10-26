using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROGRAM_PARAMS: copy of FINOVA MFF_PROGRAM_PARAMS (APEX), all columns kept.
/// </summary>
public class MffProgramParam : AuditableEntity
{
    public long ParamId { get; set; }

    public long ProgramVersionId { get; set; }

    public long? FinancierId { get; set; }

    public string ParamCode { get; set; } = string.Empty;

    public string ParamNameAr { get; set; } = string.Empty;

    public decimal? ValueNumber { get; set; }

    public string? ValueText { get; set; }

    public string? UnitCode { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? SourceReference { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
