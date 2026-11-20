using Capitra.Application.Abstractions;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Identity;

public sealed class UserDirectory(CapitraDbContext db) : IUserDirectory
{
    public async Task<IReadOnlyList<DirectoryUser>> ActiveUsersAsync(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.UserName)
            .Select(x => new { x.Id, x.UserName, x.FullName, x.Department, x.EmployeeNo }).ToListAsync(ct);
        var roles = await (from ur in db.UserRoles.AsNoTracking()
                           join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
                           select new { ur.UserId, r.Name }).ToListAsync(ct);
        var byUser = roles.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Name!).Where(n => n != null).ToList());
        return users.Select(u => new DirectoryUser(u.Id, u.UserName ?? u.Id, u.FullName, byUser.TryGetValue(u.Id, out var r) ? r : [], u.Department, u.EmployeeNo)).ToList();
    }

    public async Task<IReadOnlyList<string>> RoleNamesAsync(CancellationToken ct) =>
        (await db.Roles.AsNoTracking().Select(x => x.Name).ToListAsync(ct)).OfType<string>().OrderBy(x => x).ToList();
}
