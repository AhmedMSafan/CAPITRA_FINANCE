using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_APPLICATION_PARTIES: copy of FINOVA MFF_APPLICATION_PARTIES (APEX), all columns kept.
/// </summary>
public class MffApplicationParty : AuditableEntity
{
    public long ApplicationPartyId { get; set; }

    public long ApplicationId { get; set; }

    public long PersonId { get; set; }

    public string PartyRoleCode { get; set; } = string.Empty;

    public bool IsIncomeIncluded { get; set; } = true;

    public decimal? OwnershipSharePct { get; set; }
}
