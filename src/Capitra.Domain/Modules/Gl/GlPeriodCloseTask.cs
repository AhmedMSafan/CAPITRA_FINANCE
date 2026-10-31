using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_PERIOD_CLOSE_TASKS: copy of FINOVA GL_PERIOD_CLOSE_TASKS (APEX), all columns kept.
/// </summary>
public class GlPeriodCloseTask : AuditableEntity
{
    public long CloseTaskId { get; set; }

    public string TaskCode { get; set; } = string.Empty;

    public string TaskNameEn { get; set; } = string.Empty;

    public string? TaskNameAr { get; set; }

    /// <summary>One of: GL, LEASING, FACTORING, MORTGAGE, TREASURY, CASH, FA, SYSTEM.</summary>
    public string TaskArea { get; set; } = string.Empty;

    public string? SourceCode { get; set; }

    public bool IsMandatory { get; set; } = true;

    public bool IsAutoCheck { get; set; }

    public string? AutoCheckPackage { get; set; }

    public string? AutoCheckProcedure { get; set; }

    public string? DefaultResponsibleRole { get; set; }

    public int DisplaySeq { get; set; } = 10;

    public bool IsActive { get; set; } = true;
}
