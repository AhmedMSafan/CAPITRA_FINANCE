using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_EXPORT_AUDITS: copy of FINOVA GL_ORACLE_GL_EXPORT_AUDIT (APEX), all columns kept.
/// </summary>
public class GlOracleGlExportAudit : AuditableEntity
{
    public long AuditId { get; set; }

    public string AuditObject { get; set; } = string.Empty;

    public long AuditObjectId { get; set; }

    public string ActionCode { get; set; } = string.Empty;

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? ActionMessage { get; set; }

    public string? ActionPayload { get; set; }

    public string? ActionBy { get; set; }

    public DateTime? ActionAt { get; set; }
}
