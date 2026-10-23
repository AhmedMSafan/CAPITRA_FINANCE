using Capitra.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Capitra.Infrastructure.Identity;

/// <summary>SEC_USER: application user (ASP.NET Core Identity).</summary>
public class SecUser : IdentityUser, IAuditable
{
    public string? FullName { get; set; }

    public string? EmployeeNo { get; set; }

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int RowVersion { get; set; }
}
