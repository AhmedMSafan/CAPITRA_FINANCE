using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_POSTING_VALIDATION_SETS: copy of FINOVA GL_POSTING_VALIDATION_SETS (APEX), all columns kept.
/// </summary>
public class GlPostingValidationSet : AuditableEntity
{
    public long ValidationSetId { get; set; }

    public string ValidationSetCode { get; set; } = string.Empty;

    public string ValidationSetName { get; set; } = string.Empty;

    public long? LedgerId { get; set; }

    public string? SourceCode { get; set; }

    public string? CategoryCode { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
