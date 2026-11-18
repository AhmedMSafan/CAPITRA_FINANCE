using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Capitra.Api.Security;
using Capitra.Application.Security;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Capitra.Tests.Security;

/// <summary>A token is checked against the user's current access: deactivation, reset sign-in and role changes apply at once.</summary>
public sealed class UserAccessTests : IClassFixture<CapitraApiFactory>
{
    private readonly CapitraApiFactory _factory;

    public UserAccessTests(CapitraApiFactory factory) => _factory = factory;

    private static ClaimsPrincipal Principal(string? stamp, params string[] permissions)
    {
        var claims = new List<Claim> { new(CapitraClaimTypes.UserId, "u1"), new(CapitraClaimTypes.Role, "OLD_ROLE") };
        if (stamp is not null)
            claims.Add(new Claim(UserAccessCheck.SecurityStampClaim, stamp));
        claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    [Fact]
    public void Unknown_deactivated_or_reset_users_are_refused()
    {
        Assert.NotNull(UserAccessCheck.Apply(Principal("s1"), null));
        Assert.NotNull(UserAccessCheck.Apply(Principal("s1"), new UserAccess(false, "s1", [], [])));
        Assert.NotNull(UserAccessCheck.Apply(Principal("s1"), new UserAccess(true, "s2", [], [])));
        Assert.Null(UserAccessCheck.Apply(Principal("s1"), new UserAccess(true, "s1", [], [])));
    }

    [Fact]
    public void The_current_roles_and_permissions_replace_the_tokens()
    {
        var p = Principal("s1", "OLD.PERMISSION");
        Assert.Null(UserAccessCheck.Apply(p, new UserAccess(true, "s1", ["NEW_ROLE"], ["NEW.PERMISSION"])));
        Assert.Equal(["NEW.PERMISSION"], p.FindAll(Permissions.ClaimType).Select(c => c.Value));
        Assert.Equal(["NEW_ROLE"], p.FindAll(CapitraClaimTypes.Role).Select(c => c.Value));
    }

    [Fact]
    public void Trusting_the_token_keeps_its_claims()
    {
        var p = Principal(null, "KEPT.PERMISSION");
        Assert.Null(UserAccessCheck.Apply(p, UserAccess.TrustToken));
        Assert.Equal(["KEPT.PERMISSION"], p.FindAll(Permissions.ClaimType).Select(c => c.Value));
    }

    private sealed class Fixed(UserAccess? access) : IUserAccessSource
    {
        public Task<UserAccess?> GetAsync(string userId, CancellationToken cancellationToken) => Task.FromResult(access);
    }

    private async Task<HttpStatusCode> LookupsAs(UserAccess? access)
    {
        using var factory = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<IUserAccessSource>(_ => new Fixed(access))));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken(Permissions.All));
        return (await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative))).StatusCode;
    }

    [Fact]
    public async Task A_valid_token_of_a_deactivated_user_is_refused()
    {
        Assert.Equal(HttpStatusCode.OK, await LookupsAs(new UserAccess(true, null, [], [])));
        Assert.Equal(HttpStatusCode.Unauthorized, await LookupsAs(new UserAccess(false, null, [], [Permissions.All])));
        Assert.Equal(HttpStatusCode.Unauthorized, await LookupsAs(null));
    }
}
