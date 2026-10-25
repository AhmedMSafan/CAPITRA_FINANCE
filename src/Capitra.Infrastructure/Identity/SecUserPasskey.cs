using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_USER_PASSKEYS: WebAuthn passkey registered by a user (Identity schema version 3).</summary>
public class SecUserPasskey : IdentityUserPasskey<string>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
