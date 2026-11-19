using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>Member companies, kept by the federation. Their user accounts are made in <c>ScrCompanyUsersController</c>.</summary>
public sealed class ScrCompanyService(ICapitraDbContext db, IScrCompanyUserCounts users)
{
    public async Task<IReadOnlyList<ScrCompanyDto>> ListAsync(CancellationToken ct)
    {
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().OrderBy(c => c.CompanyCode).ToListAsync(ct);
        var inquiries = await db.Set<ScrInquiry>().AsNoTracking().GroupBy(i => i.MemberCompanyId)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var userCounts = await users.CountsAsync(ct);
        return [.. companies.Select(c => ToDto(c, userCounts.GetValueOrDefault(c.MemberCompanyId), inquiries.GetValueOrDefault(c.MemberCompanyId)))];
    }

    public async Task<ScrCompanyDto> GetAsync(long id, CancellationToken ct) =>
        (await ListAsync(ct)).FirstOrDefault(c => c.MemberCompanyId == id) ?? throw new NotFoundException($"Member company {id} was not found.");

    public async Task<ScrCompanyDto> SaveAsync(long? id, ScrCompanyRequest r, CancellationToken ct)
    {
        var e = new Dictionary<string, string[]>();
        var code = ScrValidation.Code(r.CompanyCode);
        var status = ScrValidation.Code(r.StatusCode is null ? "ACTIVE" : r.StatusCode);
        if (code.Length is < 2 or > 30 || code.Any(ch => !(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_')))
            e["companyCode"] = ["Use 2 to 30 letters, digits, - or _."];
        if (ScrValidation.Clean(r.NameEn) is null)
            e["nameEn"] = ["Enter the English name."];
        if (ScrValidation.Clean(r.NameAr) is null)
            e["nameAr"] = ["Enter the Arabic name."];
        if (status is not ("ACTIVE" or "SUSPENDED"))
            e["statusCode"] = ["Choose active or suspended."];
        if (await db.Set<ScrMemberCompany>().AnyAsync(c => c.CompanyCode == code && c.MemberCompanyId != (id ?? 0), ct))
            e["companyCode"] = [$"{code} is already used."];
        ScrValidation.Throw(e);

        var c = id is { } x
            ? await db.Set<ScrMemberCompany>().FirstOrDefaultAsync(m => m.MemberCompanyId == x, ct) ?? throw new NotFoundException($"Member company {x} was not found.")
            : new ScrMemberCompany();
        c.CompanyCode = code;
        c.NameEn = r.NameEn!.Trim();
        c.NameAr = r.NameAr!.Trim();
        c.LicenceNo = ScrValidation.Clean(r.LicenceNo);
        c.ContactEmail = ScrValidation.Clean(r.ContactEmail);
        c.StatusCode = status;
        c.Notes = ScrValidation.Clean(r.Notes);
        if (id is null)
            db.Add(c);
        await db.SaveChangesAsync(ct);
        return await GetAsync(c.MemberCompanyId, ct);
    }

    private static ScrCompanyDto ToDto(ScrMemberCompany c, int users, int inquiries) =>
        new(c.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr, c.LicenceNo, c.ContactEmail, c.StatusCode, c.Notes, users, inquiries, c.CreatedBy);
}

/// <summary>How many user accounts each member company has (the identity tables live in Infrastructure).</summary>
public interface IScrCompanyUserCounts
{
    Task<IReadOnlyDictionary<long, int>> CountsAsync(CancellationToken ct);
}
