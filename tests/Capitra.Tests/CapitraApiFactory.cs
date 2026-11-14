using System.Security.Claims;
using Capitra.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
    }

    public string IssueToken(params string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(CapitraClaimTypes.UserId, "test-user"),
            new(CapitraClaimTypes.UserName, "tester"),
        };
        claims.AddRange(permissions.Select(p => new Claim(Capitra.Application.Security.Permissions.ClaimType, p)));
        return Services.GetRequiredService<JwtTokenService>().Issue(claims).Token;
    }
}
