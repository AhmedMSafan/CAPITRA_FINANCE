using System.Security.Claims;
using Capitra.Application.Abstractions;

namespace Capitra.Api.Security;

/// <summary>Reads the current user from the validated JWT. Request bodies are never trusted for identity.</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => IsAuthenticated ? Principal!.FindFirstValue(CapitraClaimTypes.UserId) : null;

    public string? UserName => IsAuthenticated ? Principal!.FindFirstValue(CapitraClaimTypes.UserName) : null;
}
