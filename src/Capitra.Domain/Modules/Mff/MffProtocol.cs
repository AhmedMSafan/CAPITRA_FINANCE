using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROTOCOLS: copy of FINOVA MFF_PROTOCOLS (APEX), all columns kept.
/// </summary>
public class MffProtocol : AuditableEntity
{
    public long ProtocolId { get; set; }

    public string ProtocolCode { get; set; } = string.Empty;

    public string ProtocolNameAr { get; set; } = string.Empty;

    public long FundOrgId { get; set; }

    public DateOnly? SignedDate { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string? LegalReference { get; set; }

    public string? SourceReference { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
