using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_VALIDATION_SETS: copy of FINOVA GL_CLOSE_VALIDATION_SETS (APEX), all columns kept.
/// </summary>
public class GlCloseValidationSet : AuditableEntity
{
    public long ValidationSetId { get; set; }

    public string ValidationSetCode { get; set; } = string.Empty;

    public string ValidationSetNameEn { get; set; } = string.Empty;

    public string? ValidationSetNameAr { get; set; }

    /// <summary>One of: MONTH_END, YEAR_END, QUARTER_END, ADHOC.</summary>
    public string CloseType { get; set; } = string.Empty;

    /// <summary>One of: INFO, WARNING, ERROR, BLOCKER.</summary>
    public string SeverityOnFailure { get; set; } = "ERROR";

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
