using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_INTEGRATION_LOGS: copy of FINOVA MFF_INTEGRATION_LOG (APEX), all columns kept.
/// </summary>
public class MffIntegrationLog : AuditableEntity
{
    public long IntegrationLogId { get; set; }

    public string InterfaceName { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public string? ReferenceKey { get; set; }

    public DateTime RequestAt { get; set; }

    public DateTime? ResponseAt { get; set; }

    public string IntegrationLogStatus { get; set; } = string.Empty;

    public int? HttpStatus { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public string? ErrorMessage { get; set; }
}
