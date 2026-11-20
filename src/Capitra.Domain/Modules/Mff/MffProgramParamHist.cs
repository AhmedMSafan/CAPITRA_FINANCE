using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROGRAM_PARAM_HIST: copy of FINOVA MFF_PROGRAM_PARAM_HIST (APEX), all columns kept.
/// </summary>
public class MffProgramParamHist : AuditableEntity
{
    public long ParamHistId { get; set; }

    public long ParamId { get; set; }

    public long ProgramVersionId { get; set; }

    public long? FinancierOrgId { get; set; }

    public string ParamCode { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedAt { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public string? ChangeReasonAr { get; set; }
}
