using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Capitra.Api.Security;

/// <summary>
/// Request limits (fixed one-minute windows), set in configuration "RateLimits": the Screening API per API client, its token request
/// and the sign-in pages per network address, and the portal's screening per user. Past the limit the answer is 429 with Retry-After.
/// </summary>
public static class ScrRateLimits
{
    public const string ApiPolicy = "scr-api";
    public const string TokenPolicy = "scr-api-token";
    public const string PortalPolicy = "scr-portal";
    public const string SignInPolicy = "sign-in";

    public static void Add(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RateLimits");
        int Limit(string key, int fallback) => section.GetValue(key, fallback);
        var api = Limit("ScrApiPerMinute", 60);
        var token = Limit("ScrTokenPerMinute", 10);
        var portal = Limit("ScrPortalPerMinute", 30);
        var signIn = Limit("SignInPerMinute", 30);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var after))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(after.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "rate_limited", message = "Too many requests. Wait a minute and try again." }, ct);
            };
            options.AddPolicy(ApiPolicy, http => Window("client:" + (http.User.FindFirstValue(ScrApiAuth.ClientClaim) ?? Ip(http)), api));
            options.AddPolicy(TokenPolicy, http => Window("ip:" + Ip(http), token));
            options.AddPolicy(PortalPolicy, http => Window("user:" + (http.User.Identity?.Name ?? Ip(http)), portal));
            options.AddPolicy(SignInPolicy, http => Window("ip:" + Ip(http), signIn));
        });
    }

    private static RateLimitPartition<string> Window(string key, int permits) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = Math.Max(1, permits), Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true,
        });

    private static string Ip(HttpContext http) => http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
