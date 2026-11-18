using System.ComponentModel.DataAnnotations;
using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Scr;

public sealed record ScrCompanyUserDto(string UserId, string UserName, string? Email, string? FullName, bool IsActive, string RoleCode, DateTime? LastLoginAt);

public sealed record ScrCompanyUserRequest(
    [Required] string UserName, string? Password, string? Email, string? FullName, string? RoleCode, bool IsActive = true);

public sealed record ScrPasswordRequest([Required] string NewPassword);

/// <summary>Member companies (SCR.COMPANY.ADMIN, federation).</summary>
[ApiController]
[Route("api/scr/companies")]
public sealed class ScrCompaniesController(ScrCompanyService companies) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public Task<IReadOnlyList<ScrCompanyDto>> List(CancellationToken ct) => companies.ListAsync(ct);

    [HttpPost]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public Task<ScrCompanyDto> Create(ScrCompanyRequest request, CancellationToken ct) => companies.SaveAsync(null, request, ct);

    [HttpPut("{companyId:long}")]
    [HasPermission(Permissions.Scr.CompanyAdmin)]
    public Task<ScrCompanyDto> Update(long companyId, ScrCompanyRequest request, CancellationToken ct) => companies.SaveAsync(companyId, request, ct);
}

/// <summary>
/// The user accounts of a member company: they sign in with the ordinary CAPITRA login and can only screen. The federation
/// (SCR.COMPANY.ADMIN) manages any company's accounts; a company administrator (SCR.COMPANY.USERS) only its own company's,
/// and can only give the two company roles.
/// </summary>
[ApiController]
[Route("api/scr/companies/{companyId:long}/users")]
public sealed class ScrCompanyUsersController(UserManager<SecUser> users, IScrUserProfiles profiles, ScrCompanyService companies) : ControllerBase
{
    public const string CompanyUserRole = "SCR_COMPANY_USER";
    public const string CompanyAdminRole = "SCR_COMPANY_ADMIN";

    [HttpGet]
    public async Task<IActionResult> List(long companyId, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var list = await users.Users.AsNoTracking().Where(u => u.MemberCompanyId == companyId).OrderBy(u => u.UserName).ToListAsync(ct);
        var result = new List<ScrCompanyUserDto>();
        foreach (var u in list)
            result.Add(await ToDtoAsync(u));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(long companyId, ScrCompanyUserRequest request, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        await companies.GetAsync(companyId, ct);
        var role = Role(request.RoleCode);
        var userName = request.UserName.Trim();
        if (await users.FindByNameAsync(userName) is not null)
            throw new ConflictException("A user with this user name already exists.");
        if (string.IsNullOrWhiteSpace(request.Password))
            throw BusinessValidationException.For("password", "Enter a first password.");
        var user = new SecUser
        {
            UserName = userName, Email = Clean(request.Email), FullName = Clean(request.FullName), IsActive = request.IsActive, MemberCompanyId = companyId,
        };
        Ensure(await users.CreateAsync(user, request.Password), "password");
        var added = await users.AddToRoleAsync(user, role);
        if (!added.Succeeded)
        {
            await users.DeleteAsync(user);
            Ensure(added, "roleCode");
        }

        return Ok(await ToDtoAsync(user));
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(long companyId, string userId, ScrCompanyUserRequest request, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var user = await FindAsync(companyId, userId);
        user.Email = Clean(request.Email);
        user.FullName = Clean(request.FullName);
        user.IsActive = request.IsActive;
        Ensure(await users.UpdateAsync(user), "user");
        var role = Role(request.RoleCode);
        var current = await users.GetRolesAsync(user);
        if (!current.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            Ensure(await users.RemoveFromRolesAsync(user, current), "roleCode");
            Ensure(await users.AddToRoleAsync(user, role), "roleCode");
        }

        return Ok(await ToDtoAsync(user));
    }

    [HttpPost("{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(long companyId, string userId, ScrPasswordRequest request, CancellationToken ct)
    {
        if (!await AllowedAsync(companyId, ct))
            return Forbid();
        var user = await FindAsync(companyId, userId);
        var token = await users.GeneratePasswordResetTokenAsync(user);
        Ensure(await users.ResetPasswordAsync(user, token, request.NewPassword), "newPassword");
        return Ok(await ToDtoAsync(user));
    }

    /// <summary>The federation for any company; a company administrator for its own only.</summary>
    private async Task<bool> AllowedAsync(long companyId, CancellationToken ct)
    {
        if (ScrClaims.Has(User, Permissions.Scr.CompanyAdmin))
            return true;
        if (!ScrClaims.Has(User, Permissions.Scr.CompanyUsers) || User.Identity?.Name is not { } me)
            return false;
        return (await profiles.ByUserNameAsync(me, ct))?.MemberCompanyId == companyId;
    }

    private async Task<SecUser> FindAsync(long companyId, string userId)
    {
        var user = await users.FindByIdAsync(userId);
        return user is not null && user.MemberCompanyId == companyId ? user : throw new NotFoundException("The user was not found in this company.");
    }

    private static string Role(string? code)
    {
        var role = (code ?? CompanyUserRole).Trim().ToUpperInvariant();
        return role is CompanyUserRole or CompanyAdminRole
            ? role
            : throw BusinessValidationException.For("roleCode", $"A company user is {CompanyUserRole} or {CompanyAdminRole}.");
    }

    private async Task<ScrCompanyUserDto> ToDtoAsync(SecUser u)
    {
        var roles = await users.GetRolesAsync(u);
        return new(u.Id, u.UserName ?? u.Id, u.Email, u.FullName, u.IsActive, roles.Count > 0 ? roles[0] : "-", u.LastLoginAt);
    }

    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static void Ensure(IdentityResult result, string field)
    {
        if (!result.Succeeded)
            throw new BusinessValidationException(new Dictionary<string, string[]> { [field] = [.. result.Errors.Select(e => e.Description)] });
    }
}
