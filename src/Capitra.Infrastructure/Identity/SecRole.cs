using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_ROLE: role. Permissions are granted as role claims (SEC_ROLE_CLAIM).</summary>
public class SecRole : IdentityRole, IAuditable
{
    public SecRole()
    {
    }

    public SecRole(string roleName)
        : base(roleName)
    {
    }

    public string? Description { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
