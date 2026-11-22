using System.Security.Claims;
using Capitra.Domain.Modules.Scr;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Capitra.Api.Security;

/// <summary>
/// The Screening API's sign-in: a member company's system exchanges its API client's ID and secret for a token of its own audience
/// (CAPITRA_SCR_API), valid an hour. Its tokens are refused by the rest of CAPITRA, and users' tokens by the API. Each request checks
/// (kept 30 seconds) that the client is still active and its company may work (active, licence in force).
/// </summary>
public static class ScrApiAuth
{
    public const string Scheme = "ScrApi";
    public const string Audience = "CAPITRA_SCR_API";
    public const int TokenMinutes = 60;
    public const string ClientClaim = "scr_client";
    public const string CompanyClaim = "scr_company";

    public static long ClientOf(ClaimsPrincipal user) => long.Parse(user.FindFirstValue(ClientClaim)!, System.Globalization.CultureInfo.InvariantCulture);

    public static long CompanyOf(ClaimsPrincipal user) => long.Parse(user.FindFirstValue(CompanyClaim)!, System.Globalization.CultureInfo.InvariantCulture);

    public static void Add(IServiceCollection services)
    {
        services.AddAuthentication().AddJwtBearer(Scheme);
        services.AddOptions<JwtBearerOptions>(Scheme).Configure<IOptions<JwtOptions>>((bearer, jwt) =>
        {
            bearer.MapInboundClaims = false;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt.Value.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = CapitraClaimTypes.UserName,
                RoleClaimType = CapitraClaimTypes.Role,
            };
            bearer.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal;
                    if (principal?.FindFirstValue(ClientClaim) is null || principal.FindFirstValue(CompanyClaim) is null)
                    {
                        context.Fail("The token is not a Screening API token.");
                        return;
                    }
                    var services = context.HttpContext.RequestServices;
                    var cache = services.GetRequiredService<IMemoryCache>();
                    var clientId = ClientOf(principal);
                    var failure = await cache.GetOrCreateAsync($"scr-api-access:{clientId}", async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                        var db = services.GetRequiredService<CapitraDbContext>();
                        var client = await db.Set<ScrApiClient>().AsNoTracking().FirstOrDefaultAsync(c => c.ApiClientId == clientId, context.HttpContext.RequestAborted);
                        if (client is null || client.StatusCode != "ACTIVE")
                            return "The API client is revoked.";
                        return await services.GetRequiredService<CompanyAccess>().BlockReasonAsync(client.MemberCompanyId, context.HttpContext.RequestAborted);
                    });
                    if (failure is not null)
                        context.Fail(failure);
                },
            };
        });
    }

    /// <summary>Forget a client's cached access (revoked or company changed): applies at once.</summary>
    public static void Forget(IMemoryCache cache, long apiClientId) => cache.Remove($"scr-api-access:{apiClientId}");
}
