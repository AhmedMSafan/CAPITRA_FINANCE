using System.ComponentModel.DataAnnotations;
using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Sec;

public sealed record SecUserDto(
    string Id, string UserName, string? Email, string? FullName, string? EmployeeNo, string? Department, string? JobTitle, bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record CreateSecUserRequest(
    [Required] string UserName, [Required] string Password, string? Email, string? FullName, string? EmployeeNo, string? Department,
    string? JobTitle, bool IsActive, IReadOnlyList<string>? Roles);

public sealed record UpdateSecUserRequest(string? Email, string? FullName, string? EmployeeNo, string? Department, string? JobTitle, bool IsActive);

public sealed record SetSecUserRolesRequest(IReadOnlyList<string>? Roles);

public sealed record ResetSecUserPasswordRequest([Required] string NewPassword);

public sealed record SecOperationResult(bool Success, string Message, string? UserId = null);

/// <summary>User management (from the FINOVA_FINANCEING ADMIN module): users, their roles and password resets.</summary>
[ApiController]
[Route("api/sec/users")]
[HasPermission(Permissions.Sec.UserAdmin)]
public sealed class SecUserController(UserManager<SecUser> users, RoleManager<SecRole> roles) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<SecUserDto>> List(CancellationToken cancellationToken)
    {
        var rows = await users.Users.OrderBy(x => x.UserName).ToListAsync(cancellationToken);
        var result = new List<SecUserDto>();
        foreach (var user in rows)
            result.Add(await MapAsync(user));
        return result;
    }

    [HttpGet("{userId}")]
    public async Task<SecUserDto> Get(string userId) => await MapAsync(await FindAsync(userId));

    [HttpPost]
    public async Task<SecOperationResult> Create(CreateSecUserRequest request)
    {
        var userName = request.UserName.Trim();
        if (await users.FindByNameAsync(userName) is not null)
            throw new ConflictException("A user with this username already exists.");

        var wanted = await CheckRolesAsync(request.Roles);
        var user = new SecUser
        {
            UserName = userName,
            Email = Clean(request.Email),
            FullName = Clean(request.FullName),
            EmployeeNo = Clean(request.EmployeeNo),
            Department = Clean(request.Department),
            JobTitle = Clean(request.JobTitle),
            IsActive = request.IsActive,
        };
        Ensure(await users.CreateAsync(user, request.Password), "password");
        if (wanted.Count > 0)
        {
            var added = await users.AddToRolesAsync(user, wanted);
            if (!added.Succeeded)
            {
                await users.DeleteAsync(user);
                Ensure(added, "roles");
            }
        }

        return new SecOperationResult(true, $"User '{user.UserName}' created successfully.", user.Id);
    }

    [HttpPut("{userId}")]
    public async Task<SecOperationResult> Update(string userId, UpdateSecUserRequest request)
    {
        var user = await FindAsync(userId);
        user.Email = Clean(request.Email);
        user.FullName = Clean(request.FullName);
        user.EmployeeNo = Clean(request.EmployeeNo);
        user.Department = Clean(request.Department);
        user.JobTitle = Clean(request.JobTitle);
        user.IsActive = request.IsActive;
        Ensure(await users.UpdateAsync(user), "user");
        return new SecOperationResult(true, $"User '{user.UserName}' updated successfully.");
    }

    [HttpPut("{userId}/roles")]
    public async Task<SecOperationResult> SetRoles(string userId, SetSecUserRolesRequest request)
    {
        var user = await FindAsync(userId);
        var wanted = await CheckRolesAsync(request.Roles);
        var current = await users.GetRolesAsync(user);
        var remove = current.Except(wanted, StringComparer.OrdinalIgnoreCase).ToList();
        var add = wanted.Except(current, StringComparer.OrdinalIgnoreCase).ToList();
        if (remove.Count > 0)
            Ensure(await users.RemoveFromRolesAsync(user, remove), "roles");
        if (add.Count > 0)
        {
            var added = await users.AddToRolesAsync(user, add);
            if (!added.Succeeded && remove.Count > 0)
                await users.AddToRolesAsync(user, remove);
            Ensure(added, "roles");
        }

        return new SecOperationResult(true, $"Roles for user '{user.UserName}' updated successfully.");
    }

    [HttpPost("{userId}/reset-password")]
    public async Task<SecOperationResult> ResetPassword(string userId, ResetSecUserPasswordRequest request)
    {
        var user = await FindAsync(userId);
        var token = await users.GeneratePasswordResetTokenAsync(user);
        Ensure(await users.ResetPasswordAsync(user, token, request.NewPassword), "newPassword");
        return new SecOperationResult(true, $"Password for user '{user.UserName}' reset successfully.");
    }

    private async Task<SecUser> FindAsync(string userId) =>
        await users.FindByIdAsync(userId) ?? throw new NotFoundException("User not found.");

    private async Task<SecUserDto> MapAsync(SecUser user)
    {
        var names = await users.GetRolesAsync(user);
        return new SecUserDto(user.Id, user.UserName ?? string.Empty, user.Email, user.FullName, user.EmployeeNo, user.Department, user.JobTitle,
            user.IsActive, names.Order(StringComparer.Ordinal).ToList());
    }

    private async Task<List<string>> CheckRolesAsync(IReadOnlyList<string>? requested)
    {
        var wanted = (requested ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var missing = new List<string>();
        foreach (var name in wanted)
            if (!await roles.RoleExistsAsync(name))
                missing.Add($"Role '{name}' does not exist.");
        if (missing.Count > 0)
            throw new BusinessValidationException(new Dictionary<string, string[]> { ["roles"] = [.. missing] });
        return wanted;
    }

    private static void Ensure(IdentityResult result, string field)
    {
        if (!result.Succeeded)
            throw new BusinessValidationException(new Dictionary<string, string[]> { [field] = result.Errors.Select(e => e.Description).ToArray() });
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
