using System.Security.Claims;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Capitra.Api.Security;

/// <summary>
/// What a user may do right now, read from the database: active or not, the security stamp, the roles and the permissions of those
/// roles. A null list keeps what the token carries (only the test host does that).
/// </summary>
public sealed record UserAccess(bool IsActive, string? SecurityStamp, IReadOnlyList<string>? Roles, IReadOnlyList<string>? Permissions)
{
    /// <summary>Trusts the token as issued: for the in-memory test host, which has no database.</summary>
    public static readonly UserAccess TrustToken = new(true, null, null, null);
}

/// <summary>Reads a user's current access; null when the user does not exist.</summary>
public interface IUserAccessSource
{
    Task<UserAccess?> GetAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>The user, roles and role permissions as ASP.NET Identity keeps them (the same reading as login).</summary>
public sealed class IdentityUserAccessSource(UserManager<SecUser> users, RoleManager<SecRole> roles, CompanyAccess companies) : IUserAccessSource
{
    public async Task<UserAccess?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null)
            return null;

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

        // a member company's user works only while the company is active and its licence is in force
        var active = user.IsActive && (user.MemberCompanyId is not { } companyId || await companies.BlockReasonAsync(companyId, cancellationToken) is null);
        return new UserAccess(active, user.SecurityStamp, roleNames, [.. permissions]);
    }
}

/// <summary>
/// Keeps each user's access for a short time, so a deactivated user, a changed role or a reset password takes effect within
/// <see cref="Lifetime"/> instead of when the token expires. User and role administration calls <see cref="Clear"/> to apply
/// its change at once.
/// </summary>
public sealed class UserAccessCache(IMemoryCache cache)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(30);

    private long _generation;

    public async Task<UserAccess?> GetAsync(string userId, IUserAccessSource source, CancellationToken cancellationToken)
    {
        var key = $"user-access:{Interlocked.Read(ref _generation)}:{userId}";
        if (cache.TryGetValue(key, out UserAccess? access))
            return access;
        access = await source.GetAsync(userId, cancellationToken);
        cache.Set(key, access, Lifetime);
        return access;
    }

    /// <summary>Forgets every user's access (role permissions affect many users, so one generation for all is simplest).</summary>
    public void Clear() => Interlocked.Increment(ref _generation);
}

/// <summary>Checks a validated token against the user's current access and puts the current roles and permissions on the principal.</summary>
public static class UserAccessCheck
{
    /// <summary>The claim that carries the security stamp the token was issued with.</summary>
    public const string SecurityStampClaim = "sst";

    /// <summary>Null when the request may go on; otherwise why it may not.</summary>
    public static string? Apply(ClaimsPrincipal principal, UserAccess? access)
    {
        if (access is null)
            return "The user no longer exists.";
        if (!access.IsActive)
            return "The user is deactivated.";
        if (access.SecurityStamp is not null && principal.FindFirstValue(SecurityStampClaim) is { } stamp && stamp != access.SecurityStamp)
            return "The user's password or sign-in was reset; sign in again.";

        if (principal.Identity is not ClaimsIdentity identity)
            return null;
        if (access.Roles is not null)
            Replace(identity, CapitraClaimTypes.Role, access.Roles);
        if (access.Permissions is not null)
            Replace(identity, Permissions.ClaimType, access.Permissions);
        return null;
    }

    private static void Replace(ClaimsIdentity identity, string type, IEnumerable<string> values)
    {
        foreach (var claim in identity.FindAll(type).ToList())
            identity.RemoveClaim(claim);
        identity.AddClaims(values.Select(v => new Claim(type, v)));
    }
}
