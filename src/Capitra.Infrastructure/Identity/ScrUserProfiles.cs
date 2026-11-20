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

    public async Task<IReadOnlyDictionary<string, string?>> FullNamesAsync(long companyId, CancellationToken ct) =>
        await db.Users.AsNoTracking().Where(u => u.MemberCompanyId == companyId && u.UserName != null)
            .ToDictionaryAsync(u => u.UserName!, u => u.FullName, StringComparer.OrdinalIgnoreCase, ct);

    public async Task<IReadOnlyDictionary<long, ScrCompanySeats>> SeatsAsync(CancellationToken ct)
    {
        var rows = await (
            from u in db.Users.AsNoTracking()
            where u.MemberCompanyId != null
            join ur in db.UserRoles on u.Id equals ur.UserId into urs
            from ur in urs.DefaultIfEmpty()
            join r in db.Roles on ur.RoleId equals r.Id into rs
            from r in rs.DefaultIfEmpty()
            select new { CompanyId = u.MemberCompanyId!.Value, u.UserName, u.Email, u.IsActive, Pending = u.PasswordHash == null, Role = r == null ? null : r.NormalizedName })
            .ToListAsync(ct);
        return rows.GroupBy(x => x.CompanyId).ToDictionary(g => g.Key, g =>
        {
            var admin = g.Where(x => x.Role == "MEMBER_COMPANY_ADMIN").OrderByDescending(x => x.IsActive).FirstOrDefault();
            var users = g.Where(x => x.Role != "MEMBER_COMPANY_ADMIN" && x.IsActive).Select(x => x.UserName).Distinct().Count();
            return new ScrCompanySeats(admin?.UserName, admin?.Email,
                admin is null ? null : !admin.IsActive ? "INACTIVE" : admin.Pending ? "PENDING_ACTIVATION" : "ACTIVE", users);
        });
    }

    public async Task<IReadOnlyDictionary<long, int>> CountsAsync(CancellationToken ct) =>
        await db.Users.AsNoTracking().Where(u => u.MemberCompanyId != null).GroupBy(u => u.MemberCompanyId!.Value)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
