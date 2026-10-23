using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_USER_CLAIM: claim held by a user.</summary>
public class SecUserClaim : IdentityUserClaim<string>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
