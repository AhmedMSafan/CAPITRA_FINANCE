using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Sec;

public sealed record LoginRequest([Required] string UserName, [Required] string Password);

public sealed record CurrentUserDto(
    string UserId,
    string UserName,
    string? FullName,
    string? EmployeeNo,
    string? Department,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record LoginResponse(string Token, DateTime ExpiresAt, CurrentUserDto User);

[ApiController]
[Route("api/sec/auth")]
public sealed class SecAuthController(
    UserManager<SecUser> users,
    RoleManager<SecRole> roles,
    JwtTokenService tokens,
    IClock clock,
    ICurrentUser currentUser) : ControllerBase
{
    private const string InvalidLogin = "Invalid user name or password.";

    /// <summary>The only anonymous endpoint: exchanges user name and password for a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await users.FindByNameAsync(request.UserName);
        if (user is null || !user.IsActive)
            return Unauthorized(Problem(InvalidLogin));

        if (await users.IsLockedOutAsync(user))
            return Unauthorized(Problem("The account is locked. Try again later."));

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            return Unauthorized(Problem(InvalidLogin));
        }

        await users.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = clock.UtcNow;
        await users.UpdateAsync(user);

        var profile = await BuildProfileAsync(user);
        var claims = new List<Claim>
        {
            new(CapitraClaimTypes.UserId, user.Id),
            new(CapitraClaimTypes.UserName, user.UserName!),
        };
        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim(CapitraClaimTypes.FullName, user.FullName));
        claims.AddRange(profile.Roles.Select(r => new Claim(CapitraClaimTypes.Role, r)));
        claims.AddRange(profile.Permissions.Select(p => new Claim(Permissions.ClaimType, p)));

        var token = tokens.Issue(claims);
        return new LoginResponse(token.Token, token.ExpiresAt, profile);
    }

    /// <summary>The signed-in user's profile, roles and permissions, read fresh from the database.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var user = currentUser.UserId is null ? null : await users.FindByIdAsync(currentUser.UserId);
        if (user is null || !user.IsActive)
            return Unauthorized();

        return await BuildProfileAsync(user);
    }

    private async Task<CurrentUserDto> BuildProfileAsync(SecUser user)
    {
        var roleNames = (await users.GetRolesAsync(user)).Order(StringComparer.Ordinal).ToList();

        var permissions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var roleName in roleNames)
        {
            var role = await roles.FindByNameAsync(roleName);
            if (role is null)
                continue;
            foreach (var claim in await roles.GetClaimsAsync(role))
                if (claim.Type == Permissions.ClaimType)
                    permissions.Add(claim.Value);
        }

        return new CurrentUserDto(user.Id, user.UserName!, user.FullName, user.EmployeeNo, user.Department, roleNames, [.. permissions]);
    }

    private static ProblemDetails Problem(string detail) =>
        new() { Status = StatusCodes.Status401Unauthorized, Title = "Login failed", Detail = detail };
}
