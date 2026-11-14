using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_SOURCE_MAPS: copy of FINOVA GL_ORACLE_GL_SOURCE_MAP (APEX), all columns kept.
/// </summary>
public class GlOracleGlSourceMap : AuditableEntity
{
    public long SourceMapId { get; set; }

    public long ExternalSystemId { get; set; }

    public string FinovaSourceCode { get; set; } = string.Empty;

    public string OracleJeSourceName { get; set; } = string.Empty;

    public string? OracleSourceKey { get; set; }

    public bool IsActive { get; set; } = true;
}
