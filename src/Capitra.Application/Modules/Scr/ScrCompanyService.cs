using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>Member companies, kept by the federation. Their user accounts are made in <c>ScrCompanyUsersController</c>.</summary>
public sealed class ScrCompanyService(ICapitraDbContext db, IScrCompanyUserCounts users, IClock clock)
{
    public const int MaxUsersLimit = 10_000;

    public async Task<IReadOnlyList<ScrCompanyDto>> ListAsync(CancellationToken ct)
    {
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().OrderBy(c => c.CompanyCode).ToListAsync(ct);
        var inquiries = await db.Set<ScrInquiry>().AsNoTracking().GroupBy(i => i.MemberCompanyId)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var userCounts = await users.CountsAsync(ct);
        var seats = await users.SeatsAsync(ct);
        var plans = await db.Set<ScrPricePlan>().AsNoTracking().ToDictionaryAsync(p => p.PricePlanId, p => p.PlanCode, ct);
        var licences = (await db.Set<ScrCompanyLicence>().AsNoTracking().ToListAsync(ct))
            .GroupBy(l => l.MemberCompanyId).ToDictionary(g => g.Key, g => g.MaxBy(l => l.LicenceId)!);
        var today = DateOnly.FromDateTime(clock.UtcNow);
        return [.. companies.Select(c =>
        {
            var l = licences.GetValueOrDefault(c.MemberCompanyId);
            return ToDto(c, userCounts.GetValueOrDefault(c.MemberCompanyId), inquiries.GetValueOrDefault(c.MemberCompanyId)) with
            {
                Licence = l is null ? null : ToDto(l),
                Seats = seats.GetValueOrDefault(c.MemberCompanyId) ?? new ScrCompanySeats(null, null, null, 0),
                LicenceStateCode = l is null ? "NONE" : today < l.ValidFrom ? "NOT_STARTED" : l.ValidTo is { } to && today > to ? "EXPIRED" : "ACTIVE",
                PricePlanCode = c.PricePlanId is { } p ? plans.GetValueOrDefault(p) : null,
            };
        })];
    }

    /// <summary>The company's licences, newest first (the first is the one in force).</summary>
    public async Task<IReadOnlyList<ScrLicenceDto>> LicencesAsync(long companyId, CancellationToken ct) =>
        [.. (await db.Set<ScrCompanyLicence>().AsNoTracking().Where(l => l.MemberCompanyId == companyId).OrderByDescending(l => l.LicenceId).ToListAsync(ct)).Select(ToDto)];

    /// <summary>
    /// A new licence for the company (the newest is in force; earlier ones stay for billing and audit). The users allowed cannot drop
    /// below the users the company has now: deactivate users first.
    /// </summary>
    public async Task<ScrLicenceDto> AddLicenceAsync(long companyId, ScrLicenceRequest r, CancellationToken ct)
    {
        if (!await db.Set<ScrMemberCompany>().AnyAsync(c => c.MemberCompanyId == companyId, ct))
            throw new NotFoundException($"Member company {companyId} was not found.");
        var e = ValidateLicence(r);
        var inUse = (await users.SeatsAsync(ct)).GetValueOrDefault(companyId)?.UsersInUse ?? 0;
        if (r.MaxUsers is { } max && max < inUse)
            e["maxUsers"] = ["The company has more active users than that besides its administrator. Deactivate users before lowering the licence."];
        ScrValidation.Throw(e);
        var l = new ScrCompanyLicence
        {
            MemberCompanyId = companyId, MaxUsers = r.MaxUsers!.Value, ValidFrom = r.ValidFrom!.Value, ValidTo = r.ValidTo,
            YearlyInquiryQuota = r.YearlyInquiryQuota, Notes = ScrValidation.Clean(r.Notes),
        };
        db.Add(l);
        await db.SaveChangesAsync(ct);
        return ToDto(l);
    }

    public static Dictionary<string, string[]> ValidateLicence(ScrLicenceRequest? r)
    {
        var e = new Dictionary<string, string[]>();
        if (r is null)
        {
            e["licence"] = ["Enter the licence."];
            return e;
        }

        if (r.MaxUsers is not { } max || max < 0 || max > MaxUsersLimit)
            e["maxUsers"] = [$"Enter the number of users besides the administrator, 0 to {MaxUsersLimit}."];
        if (r.ValidFrom is null)
            e["validFrom"] = ["Enter the date the licence starts."];
        if (r.ValidTo is { } to && r.ValidFrom is { } from && to < from)
            e["validTo"] = ["The licence cannot end before it starts."];
        if (r.YearlyInquiryQuota is { } q && q <= 0)
            e["yearlyInquiryQuota"] = ["Enter a quota above 0, or leave it empty for no limit."];
        return e;
    }

    private static ScrLicenceDto ToDto(ScrCompanyLicence l) =>
        new(l.LicenceId, l.MaxUsers, l.ValidFrom, l.ValidTo, l.YearlyInquiryQuota, l.Notes, l.CreatedBy, l.CreatedAt);

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
        var cycle = ScrValidation.Code(r.BillingCycleCode is null ? "MONTHLY" : r.BillingCycleCode);
        if (!ScrBilling.Cycles.Contains(cycle))
            e["billingCycleCode"] = ["Choose monthly, quarterly or yearly invoicing."];
        if (r.PricePlanId is { } plan && !await db.Set<ScrPricePlan>().AnyAsync(p => p.PricePlanId == plan, ct))
            e["pricePlanId"] = ["The price plan was not found."];
        if (await db.Set<ScrMemberCompany>().AnyAsync(c => c.CompanyCode == code && c.MemberCompanyId != (id ?? 0), ct))
            e["companyCode"] = ["This company code is already used."];
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
        c.PricePlanId = r.PricePlanId;
        c.BillingCycleCode = cycle;
        if (id is null)
            db.Add(c);
        await db.SaveChangesAsync(ct);
        return await GetAsync(c.MemberCompanyId, ct);
    }

    private static ScrCompanyDto ToDto(ScrMemberCompany c, int users, int inquiries) =>
        new(c.MemberCompanyId, c.CompanyCode, c.NameEn, c.NameAr, c.LicenceNo, c.ContactEmail, c.StatusCode, c.Notes, users, inquiries, c.CreatedBy)
        {
            PricePlanId = c.PricePlanId, BillingCycleCode = c.BillingCycleCode,
        };
}

/// <summary>How many user accounts each member company has (the identity tables live in Infrastructure).</summary>
public interface IScrCompanyUserCounts
{
    Task<IReadOnlyDictionary<long, int>> CountsAsync(CancellationToken ct);

    /// <summary>Per company: its administrator and the seats its other users hold.</summary>
    Task<IReadOnlyDictionary<long, ScrCompanySeats>> SeatsAsync(CancellationToken ct);

    /// <summary>The full names of a company's accounts, by user name (for invoice lines).</summary>
    Task<IReadOnlyDictionary<string, string?>> FullNamesAsync(long companyId, CancellationToken ct);
}
