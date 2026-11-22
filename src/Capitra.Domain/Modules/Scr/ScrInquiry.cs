using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// SCR_INQUIRIES: one screening by a member company's user against the Final List, with its purpose, its result and the list
/// version it was checked against. Kept for audit; a company sees only its own.
/// </summary>
public class ScrInquiry : AuditableEntity
{
    public long InquiryId { get; set; }

    /// <summary>SCR-2026-000001.</summary>
    public string InquiryRef { get; set; } = string.Empty;

    public long MemberCompanyId { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    /// <summary>One of: PERSON, COMPANY.</summary>
    public string SubjectTypeCode { get; set; } = "PERSON";

    public string SubjectName { get; set; } = string.Empty;

    /// <summary>One of: NATIONAL_ID, PASSPORT, COMMERCIAL_REGISTER, OTHER.</summary>
    public string IdentityTypeCode { get; set; } = "NATIONAL_ID";

    public string IdentityNo { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public string? NationalityCode { get; set; }

    /// <summary>One of: ONBOARDING, TRANSACTION, PERIODIC_REVIEW, OTHER.</summary>
    public string PurposeCode { get; set; } = "ONBOARDING";

    public string? PurposeText { get; set; }

    public string? CaseReference { get; set; }

    /// <summary>One of: NO_MATCH, POSSIBLE_MATCH (name only, for review), MATCH, PREVIOUSLY_LISTED (lifted entries only).</summary>
    public string ResultCode { get; set; } = "NO_MATCH";

    public int RegistryVersionNo { get; set; }

    public DateTime ScreenedAt { get; set; }

    /// <summary>The sign-in session the inquiry was made in (SEC_SIGN_INS.SESSION_ID); empty for inquiries made before sessions were kept.</summary>
    public string? SessionId { get; set; }

    /// <summary>When the request reached the service.</summary>
    public DateTime? ReceivedAt { get; set; }

    /// <summary>When the result was ready to send back.</summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>How the inquiry came: PORTAL (a user in the Screening Portal) or API (the company's own system, SCR_API_CLIENTS).</summary>
    public string ChannelCode { get; set; } = "PORTAL";

    /// <summary>The API client that made it (API inquiries only).</summary>
    public long? ApiClientId { get; set; }

    /// <summary>The seal of the inquiry before it in the chain (null for the first sealed one).</summary>
    public string? PrevHash { get; set; }

    /// <summary>SHA-256 (hex) of the inquiry's evidence and PrevHash; once set, the database refuses any change (ScrInquirySeal).</summary>
    public string? RecordHash { get; set; }

    public List<ScrInquiryHit> Hits { get; set; } = [];
}

/// <summary>SCR_INQUIRY_HITS: a Final List entry an inquiry matched, and how.</summary>
public class ScrInquiryHit : AuditableEntity
{
    public long InquiryHitId { get; set; }

    public long InquiryId { get; set; }

    public long PartyId { get; set; }

    public long DecisionId { get; set; }

    /// <summary>One of: ID_AND_NAME, ID_ONLY, NAME_ONLY.</summary>
    public string MatchBasisCode { get; set; } = "ID_AND_NAME";

    /// <summary>Best name similarity, 0-100.</summary>
    public decimal NameScorePct { get; set; }

    public string MatchedName { get; set; } = string.Empty;

    /// <summary>The entry's status when screened: ACTIVE, SUSPENDED or LIFTED.</summary>
    public string EntryStatusCode { get; set; } = "ACTIVE";
}
