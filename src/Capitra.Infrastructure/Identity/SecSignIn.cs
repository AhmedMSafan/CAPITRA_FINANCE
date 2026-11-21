using Capitra.Domain.Common;

namespace Capitra.Infrastructure.Identity;

/// <summary>
/// SEC_SIGN_INS: every sign-in attempt, successful or not: the user name typed, the account and company it belongs to, when, from
/// which address and device, and the result (or why it was refused). A successful one opens a session: its id travels in the token,
/// so whatever is done in it (screening inquiries) points back here. Never deleted; never holds a password.
/// </summary>
public class SecSignIn : AuditableEntity
{
    public long SignInId { get; set; }

    /// <summary>The session a successful sign-in opened (32 hex characters); empty when refused.</summary>
    public string? SessionId { get; set; }

    /// <summary>The user name as typed.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>The account it belongs to; empty when no such account.</summary>
    public string? UserId { get; set; }

    public long? MemberCompanyId { get; set; }

    public DateTime AttemptedAt { get; set; }

    /// <summary>SUCCESS, UNKNOWN_USER, INACTIVE, LOCKED, BAD_PASSWORD or COMPANY_BLOCKED.</summary>
    public string ResultCode { get; set; } = "SUCCESS";

    /// <summary>The message the user was shown when refused.</summary>
    public string? FailureText { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>When the session's token expires (successful sign-ins).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>When the user signed out; empty if the session simply expired.</summary>
    public DateTime? SignedOutAt { get; set; }
}
