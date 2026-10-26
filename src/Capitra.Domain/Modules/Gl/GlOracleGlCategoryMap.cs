using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_CATEGORY_MAPS: copy of FINOVA GL_ORACLE_GL_CATEGORY_MAP (APEX), all columns kept.
/// </summary>
public class GlOracleGlCategoryMap : AuditableEntity
{
    public long CategoryMapId { get; set; }

    public long ExternalSystemId { get; set; }

    public string FinovaCategoryCode { get; set; } = string.Empty;

    public string OracleJeCategoryName { get; set; } = string.Empty;

    public string? OracleCategoryKey { get; set; }

    public bool IsActive { get; set; } = true;
}
