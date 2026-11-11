namespace Capitra.Application.Abstractions;

/// <summary>An active user as other users see them: for picking a delegate or a next assignee.</summary>
public sealed record DirectoryUser(
    string UserId, string UserName, string? FullName, IReadOnlyList<string> Roles, string? Department = null, string? EmployeeNo = null);

/// <summary>Reads the users and their roles (the identity tables live in Infrastructure).</summary>
public interface IUserDirectory
{
    Task<IReadOnlyList<DirectoryUser>> ActiveUsersAsync(CancellationToken ct);

    /// <summary>Every role's name, with or without members.</summary>
    Task<IReadOnlyList<string>> RoleNamesAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<string>>([]);
}
