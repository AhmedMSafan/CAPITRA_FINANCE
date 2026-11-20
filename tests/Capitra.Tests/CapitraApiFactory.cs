using System.Security.Claims;
using Capitra.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Capitra.Tests;

/// <summary>Runs the real API in memory. No database is contacted unless a test calls an endpoint that needs one.</summary>
public sealed class CapitraApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:CapitraDb", "Host=localhost;Database=CAPITRA_TEST_UNUSED");
        builder.UseSetting("Jwt:Key", "test-only-signing-key-0123456789-abcdef");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
        // no database here: every token's user is taken as active with the roles and permissions the token carries
        builder.ConfigureTestServices(services => services.AddScoped<IUserAccessSource, TrustTokenAccessSource>());
    }

    private sealed class TrustTokenAccessSource : IUserAccessSource
    {
        public Task<UserAccess?> GetAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<UserAccess?>(UserAccess.TrustToken);
    }

    public string IssueToken(params string[] permissions) => Issue(false, permissions);

    /// <summary>A token as given after signing in with a temporary password: it only allows changing the password.</summary>
    public string IssueTemporaryPasswordToken(params string[] permissions) => Issue(true, permissions);

    private string Issue(bool mustChangePassword, string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(CapitraClaimTypes.UserId, "test-user"),
            new(CapitraClaimTypes.UserName, "tester"),
        };
        claims.AddRange(permissions.Select(p => new Claim(Capitra.Application.Security.Permissions.ClaimType, p)));
        if (mustChangePassword)
            claims.Add(new Claim(CapitraClaimTypes.PasswordChangeRequired, "1"));
        return Services.GetRequiredService<JwtTokenService>().Issue(claims).Token;
    }
}
