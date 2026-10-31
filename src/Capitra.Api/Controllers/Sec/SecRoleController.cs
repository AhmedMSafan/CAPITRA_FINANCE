using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Api.Controllers.Sec;

public sealed record SecRoleDto(string Id, string Name, string? Description, IReadOnlyList<string> Permissions);

public sealed record CreateSecRoleRequest([Required] string RoleName, string? Description);

public sealed record SetSecRolePermissionsRequest(IReadOnlyList<string>? Permissions);

public sealed record RenameSecRoleRequest([Required] string RoleName);

/// <summary>
/// Role management (from the FINOVA_FINANCEING ADMIN module). Capitra checks permissions, so a role also carries
/// the permission codes it grants; workflow tasks assigned to a role go to every user in it.
/// </summary>
[ApiController]
[Route("api/sec/roles")]
[HasPermission(Permissions.Sec.UserAdmin)]
public sealed class SecRoleController(RoleManager<SecRole> roles, UserManager<SecUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<SecRoleDto>> List(CancellationToken cancellationToken)
    {
        var rows = await roles.Roles.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var result = new List<SecRoleDto>();
        foreach (var role in rows)
            result.Add(await MapAsync(role));
        return result;
    }

    /// <summary>Every permission code a role can be granted.</summary>
    [HttpGet("permissions")]
    public IReadOnlyList<string> Catalog() => [Permissions.All, .. Permissions.Catalog];

    [HttpPost]
    public async Task<SecRoleDto> Create(CreateSecRoleRequest request)
    {
        var name = request.RoleName.Trim();
        if (await roles.RoleExistsAsync(name))
            throw new ConflictException($"Role '{name}' already exists.");
        var role = new SecRole(name) { Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim() };
        Ensure(await roles.CreateAsync(role));
        return await MapAsync(role);
    }

    /// <summary>Changes the role code; only while no user has the role, since users and workflow steps refer to it by name.</summary>
    [HttpPut("{roleId}/name")]
    public async Task<SecRoleDto> Rename(string roleId, RenameSecRoleRequest request)
    {
        var role = await roles.FindByIdAsync(roleId) ?? throw new NotFoundException("Role not found.");
        var name = request.RoleName.Trim();
        if (string.Equals(name, role.Name, StringComparison.Ordinal))
            return await MapAsync(role);
        var inRole = await users.GetUsersInRoleAsync(role.Name ?? string.Empty);
        if (inRole.Count > 0)
            throw new BusinessRuleException($"Role '{role.Name}' has {inRole.Count} user(s); remove them from the role before changing its code.");
        var other = await roles.FindByNameAsync(name);
        if (other != null && other.Id != role.Id)
            throw new ConflictException($"Role '{name}' already exists.");
        Ensure(await roles.SetRoleNameAsync(role, name));
        Ensure(await roles.UpdateAsync(role));
        return await MapAsync(role);
    }

    [HttpPut("{roleId}/permissions")]
    public async Task<SecRoleDto> SetPermissions(string roleId, SetSecRolePermissionsRequest request)
    {
        var role = await roles.FindByIdAsync(roleId) ?? throw new NotFoundException("Role not found.");
        var known = new HashSet<string>([Permissions.All, .. Permissions.Catalog], StringComparer.Ordinal);
        var wanted = (request.Permissions ?? []).Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        var unknown = wanted.Where(x => !known.Contains(x)).ToArray();
        if (unknown.Length > 0)
            throw new BusinessValidationException(new Dictionary<string, string[]> { ["permissions"] = unknown.Select(x => $"Unknown permission '{x}'.").ToArray() });

        var current = (await roles.GetClaimsAsync(role)).Where(c => c.Type == Permissions.ClaimType).ToList();
        foreach (var claim in current.Where(c => !wanted.Contains(c.Value)))
            Ensure(await roles.RemoveClaimAsync(role, claim));
        foreach (var code in wanted.Where(w => current.All(c => c.Value != w)))
            Ensure(await roles.AddClaimAsync(role, new Claim(Permissions.ClaimType, code)));
        return await MapAsync(role);
    }

    private async Task<SecRoleDto> MapAsync(SecRole role)
    {
        var codes = (await roles.GetClaimsAsync(role)).Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).Order(StringComparer.Ordinal).ToList();
        return new SecRoleDto(role.Id, role.Name ?? string.Empty, role.Description, codes);
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BusinessValidationException(new Dictionary<string, string[]> { ["role"] = result.Errors.Select(e => e.Description).ToArray() });
    }
}
