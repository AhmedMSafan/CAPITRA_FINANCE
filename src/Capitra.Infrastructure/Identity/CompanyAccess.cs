using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Scr;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Identity;

/// <summary>
/// Whether a member company's accounts may be used today: the company is active and its licence in force covers today. Checked at
/// sign-in and on every request (through the user access check), so a suspension or an expired licence ends sessions at once.
/// </summary>
public sealed class CompanyAccess(CapitraDbContext db, IClock clock)
{
    /// <summary>Null when the company's users may work; otherwise why not, in words for the sign-in page.</summary>
    public async Task<string?> BlockReasonAsync(long memberCompanyId, CancellationToken ct)
    {
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => c.MemberCompanyId == memberCompanyId)
            .Select(c => new { c.StatusCode }).FirstOrDefaultAsync(ct);
        if (company is null)
            return "The member company was not found.";
        if (company.StatusCode != "ACTIVE")
            return "Your company's access is suspended. Contact the Federation.";
        var licence = await CurrentLicenceAsync(memberCompanyId, ct);
        var today = DateOnly.FromDateTime(clock.UtcNow);
        if (licence is null)
            return "Your company has no licence. Contact the Federation.";
        if (today < licence.ValidFrom)
            return $"Your company's licence starts on {licence.ValidFrom:yyyy-MM-dd}.";
        if (licence.ValidTo is { } to && today > to)
            return $"Your company's licence ended on {to:yyyy-MM-dd}. Contact the Federation to renew it.";
        return null;
    }

    /// <summary>The licence in force: the company's newest licence row.</summary>
    public Task<ScrCompanyLicence?> CurrentLicenceAsync(long memberCompanyId, CancellationToken ct) =>
        db.Set<ScrCompanyLicence>().AsNoTracking().Where(l => l.MemberCompanyId == memberCompanyId)
            .OrderByDescending(l => l.LicenceId).FirstOrDefaultAsync(ct);
}
