using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_SETUP_AUDITS: copy of FINOVA GL_SETUP_AUDIT (APEX), all columns kept.
/// </summary>
public class GlSetupAudit : AuditableEntity
{
    public long AuditId { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string EntityKey { get; set; } = string.Empty;

    /// <summary>One of: INSERT, UPDATE, DELETE, ACTIVATE, DEACTIVATE, OPEN_PERIOD, CLOSE_PERIOD, REOPEN_PERIOD.</summary>
    public string ActionCode { get; set; } = string.Empty;

    public string? ChangeSummary { get; set; }

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public long? SourceAppId { get; set; }

    public long? SourcePageId { get; set; }

    public string? SourceSessionId { get; set; }
}
