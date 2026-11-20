using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Security;

/// <summary>
/// A user signed in with a temporary password (token claim pwd_chg) may only change it: every other API call is refused with
/// 403 and code PASSWORD_CHANGE_REQUIRED. Changing the password issues a new token without the claim.
/// </summary>
public sealed class PasswordChangeGate(RequestDelegate next)
{
    public const string Code = "PASSWORD_CHANGE_REQUIRED";

    private static readonly string[] Allowed = ["/api/sec/auth/change-password", "/api/sec/auth/me"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim(CapitraClaimTypes.PasswordChangeRequired, "1")
            && path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            && !Allowed.Any(a => path.Equals(a, StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Password change required",
                Detail = "Choose your own password before continuing.",
                Extensions = { ["code"] = Code },
            });
            return;
        }

        await next(context);
    }
}
