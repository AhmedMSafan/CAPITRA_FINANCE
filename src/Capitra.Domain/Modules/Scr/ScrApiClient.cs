using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Scr;

/// <summary>
/// SCR_API_CLIENTS: a member company's system allowed to screen through the Screening API. The Federation creates it; the company's
/// system exchanges its client ID and secret for a short-lived token (POST /api/scr/v1/token). Only a hash of the secret is kept: the
/// secret is shown once, when it is made. API inquiries are kept, billed and audited like the portal's.
/// </summary>
public class ScrApiClient : AuditableEntity
{
    public long ApiClientId { get; set; }

    public long MemberCompanyId { get; set; }

    /// <summary>The public identifier the company's system sends, e.g. scr_nfc_7k2m9q4x.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>What it is for, e.g. "Core banking onboarding".</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>PBKDF2-SHA256 of the secret: iterations.salt.hash (base64).</summary>
    public string SecretHash { get; set; } = string.Empty;

    /// <summary>The secret's last 4 characters, to tell secrets apart.</summary>
    public string SecretHint { get; set; } = string.Empty;

    public DateTime SecretCreatedAt { get; set; }

    /// <summary>ACTIVE or REVOKED (kept; a revoked client's tokens stop working at once).</summary>
    public string StatusCode { get; set; } = "ACTIVE";

    public DateTime? RevokedAt { get; set; }

    public string? RevokedBy { get; set; }

    public DateTime? LastTokenAt { get; set; }

    public string? LastIpAddress { get; set; }

    public string? Notes { get; set; }
}
