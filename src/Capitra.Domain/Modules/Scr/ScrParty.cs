using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>SCR_PARTIES: a person or company that decisions are about.</summary>
public class ScrParty : AuditableEntity
{
    public long PartyId { get; set; }

    /// <summary>One of: PERSON, COMPANY.</summary>
    public string PartyTypeCode { get; set; } = "PERSON";

    public string NameAr { get; set; } = string.Empty;

    public string? NameEn { get; set; }

    /// <summary>One of: NATIONAL_ID (Egyptian, 14 digits), PASSPORT, COMMERCIAL_REGISTER, OTHER.</summary>
    public string IdentityTypeCode { get; set; } = "NATIONAL_ID";

    public string IdentityNo { get; set; } = string.Empty;

    /// <summary>ISO 3166 alpha-2, EG by default.</summary>
    public string NationalityCode { get; set; } = "EG";

    public DateOnly? BirthDate { get; set; }

    public string? Notes { get; set; }

    public List<ScrPartyAlias> Aliases { get; set; } = [];
}

/// <summary>SCR_PARTY_ALIASES: another name the party is known by (other spelling, former name, trade name). Screened like the name.</summary>
public class ScrPartyAlias : AuditableEntity
{
    public long PartyAliasId { get; set; }

    public long PartyId { get; set; }

    public string AliasName { get; set; } = string.Empty;
}
