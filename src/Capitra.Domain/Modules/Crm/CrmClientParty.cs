using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_CLIENT_PARTIES: an owner, manager or authorised signatory of a customer, with its screening (sanctions, blacklists, PEP).
/// Part of the customer's compliance in Customer 360, so every business line uses the same people (Ahmed, 26 Nov 2026; until then
/// they were kept per Consumer Finance supplier application).
/// </summary>
public class CrmClientParty : AuditableEntity
{
    public long PartyId { get; set; }

    public long ClientId { get; set; }

    /// <summary>OWNER, MANAGER or SIGNATORY.</summary>
    public string PartyTypeCode { get; set; } = "OWNER";

    public string FullName { get; set; } = string.Empty;

    public string? NationalIdNo { get; set; }

    public string? Nationality { get; set; }

    /// <summary>The share of an owner, in %.</summary>
    public decimal? OwnershipPct { get; set; }

    /// <summary>CLEAR or MATCH; null until the person is screened.</summary>
    public string? ScreeningResultCode { get; set; }

    public string? ScreenedBy { get; set; }

    public DateTime? ScreenedAt { get; set; }
}
