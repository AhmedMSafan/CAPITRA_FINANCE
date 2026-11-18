using Capitra.Application.Modules.Scr;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Identity;

/// <summary>Screening's view of the users: the person (employee number) and member company behind an account.</summary>
public sealed class ScrUserProfiles(CapitraDbContext db) : IScrUserProfiles, IScrCompanyUserCounts
{
    public async Task<ScrUserProfile?> ByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userName.Trim().ToUpperInvariant();
        return await db.Users.AsNoTracking().Where(u => u.NormalizedUserName == normalized)
            .Select(u => new ScrUserProfile(u.Id, u.UserName ?? u.Id, u.FullName, u.EmployeeNo, u.MemberCompanyId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<long, int>> CountsAsync(CancellationToken ct) =>
        await db.Users.AsNoTracking().Where(u => u.MemberCompanyId != null).GroupBy(u => u.MemberCompanyId!.Value)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
