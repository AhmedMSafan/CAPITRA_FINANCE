using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_ROLE_CLAIM: claim held by a role (permissions).</summary>
public class SecRoleClaim : IdentityRoleClaim<string>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
