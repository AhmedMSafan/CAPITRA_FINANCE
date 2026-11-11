using Capitra.Api.Security;
using Capitra.Application.Abstractions;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

public sealed record WfeDirectoryRoleDto(string Name, int Members);

public sealed record WfeDirectoryUserDto(string UserName, string? FullName, IReadOnlyList<string> Roles);

public sealed record WfeDirectoryDto(IReadOnlyList<WfeDirectoryRoleDto> Roles, IReadOnlyList<WfeDirectoryUserDto> Users);

/// <summary>The roles and active users the designer offers when a step is assigned or escalated.</summary>
[ApiController]
[Route("api/wfe/directory")]
public sealed class WfeDirectoryController(IUserDirectory directory) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public async Task<WfeDirectoryDto> Get(CancellationToken ct)
    {
        var users = await directory.ActiveUsersAsync(ct);
        // every role, with how many active users are in it (a role with none still shows, so a step can be assigned before people join)
        var names = (await directory.RoleNamesAsync(ct)).Concat(users.SelectMany(u => u.Roles)).Distinct(StringComparer.OrdinalIgnoreCase);
        var roles = names.Select(n => new WfeDirectoryRoleDto(n, users.Count(u => u.Roles.Contains(n, StringComparer.OrdinalIgnoreCase))))
            .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
        return new WfeDirectoryDto(roles, users.Select(u => new WfeDirectoryUserDto(u.UserName, u.FullName, u.Roles)).ToList());
    }
}
