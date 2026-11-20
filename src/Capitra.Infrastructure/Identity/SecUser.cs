using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_USERS: application user (ASP.NET Core Identity).</summary>
public class SecUser : IdentityUser, IAuditable
{
    public string? FullName { get; set; }

    public string? EmployeeNo { get; set; }

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>The member company a screening user belongs to (SCR_MEMBER_COMPANIES); null for federation and CAPITRA staff.</summary>
    public long? MemberCompanyId { get; set; }

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// The password was set by an administrator (a temporary one): the user must choose their own before doing anything else.
    /// Until then a sign-in only allows changing the password.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>When the user last chose their own password.</summary>
    public DateTime? PasswordChangedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
