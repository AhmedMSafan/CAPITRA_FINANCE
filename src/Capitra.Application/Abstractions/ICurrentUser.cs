namespace Capitra.Application.Abstractions;

/// <summary>The user behind the current request, always read from the login token.</summary>
public interface ICurrentUser
{
    string? UserId { get; }

    string? UserName { get; }

    /// <summary>The user's full name (AI_USER_FULL_NAME in the copied APEX pages).</summary>
    string? FullName => null;

    bool IsAuthenticated { get; }

    /// <summary>The names of the user's roles, from the token.</summary>
    IReadOnlyCollection<string> Roles { get; }
}
