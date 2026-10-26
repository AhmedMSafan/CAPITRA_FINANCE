namespace Capitra.Application.Abstractions;

/// <summary>The user behind the current request, always read from the login token.</summary>
public interface ICurrentUser
{
    string? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    /// <summary>The names of the user's roles, from the token.</summary>
    IReadOnlyCollection<string> Roles { get; }
}
