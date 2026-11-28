using System.Text;
using Capitra.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Capitra.Api.Security;

public static class SecurityServiceCollectionExtensions
{
    /// <summary>JWT bearer authentication, with login required on every endpoint unless marked [AllowAnonymous].</summary>
    public static IServiceCollection AddCapitraSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddMemoryCache();
        services.AddSingleton<UserAccessCache>();
        services.AddScoped<IUserAccessSource, IdentityUserAccessSource>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = CapitraClaimTypes.UserName,
                    RoleClaimType = CapitraClaimTypes.Role,
                };
                // The token is checked against the user's current access on every request (kept 30 seconds): a deactivated user, a
                // removed role or a reset password stops working within that time, not when the token expires.
                bearer.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirst(CapitraClaimTypes.UserId)?.Value;
                        if (context.Principal is null || string.IsNullOrEmpty(userId))
                        {
                            context.Fail("The token has no user.");
                            return;
                        }

                        var services = context.HttpContext.RequestServices;
                        var access = await services.GetRequiredService<UserAccessCache>()
                            .GetAsync(userId, services.GetRequiredService<IUserAccessSource>(), context.HttpContext.RequestAborted);
                        if (UserAccessCheck.Apply(context.Principal, access) is { } failure)
                            context.Fail(failure);
                    },
                };
            });

        // the Screening API's own token audience, for member companies' systems
        ScrApiAuth.Add(services);
        ScrRateLimits.Add(services, configuration);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
