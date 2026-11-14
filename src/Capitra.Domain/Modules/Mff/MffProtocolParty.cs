using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROTOCOL_PARTIES: copy of FINOVA MFF_PROTOCOL_PARTIES (APEX), all columns kept.
/// </summary>
public class MffProtocolParty : AuditableEntity
{
    public long ProtocolPartyId { get; set; }

    public long ProtocolId { get; set; }

    public long OrgId { get; set; }

    public string? PartyRole { get; set; }

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public bool IsActive { get; set; } = true;
}
