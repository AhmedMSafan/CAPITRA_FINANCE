using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_PROFILE_MODULES: copy of FINOVA GL_CLOSE_PROFILE_MODULES (APEX), all columns kept.
/// </summary>
public class GlCloseProfileModule : AuditableEntity
{
    public long ProfileModuleId { get; set; }

    public long CloseProfileId { get; set; }

    public string SourceModule { get; set; } = string.Empty;

    public string SourceModuleNameEn { get; set; } = string.Empty;

    public string? SourceModuleNameAr { get; set; }

    public bool IsMandatory { get; set; } = true;

    public bool IsPostingRequired { get; set; } = true;

    public bool IsReconciliationRequired { get; set; }

    public string? ExpectedStatusCode { get; set; } = "POSTED";

    public int DisplaySeq { get; set; } = 10;

    public bool IsActive { get; set; } = true;
}
