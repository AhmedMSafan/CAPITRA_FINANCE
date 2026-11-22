using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// Screening by member companies (FINOVA's Member Company Screening Portal). A user linked to a member company screens a person or
/// company against the Final List; the inquiry, its purpose, its result and the list version are kept. Each company sees only its
/// own inquiries; federation auditors see all of them. Unlike the prototype, nothing about the list is shown before an inquiry is
/// made and recorded (no look-up by register number while typing).
/// </summary>
public sealed class ScrInquiryService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, IScrUserProfiles profiles, ScrStaffService staff)
{
    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    /// <summary>The member company of the signed-in user; refused for a user of no company or of a suspended one.</summary>
    public async Task<ScrMemberCompany> MyCompanyAsync(CancellationToken ct)
    {
        var profile = await profiles.ByUserNameAsync(Me, ct);
        if (profile?.MemberCompanyId is not { } companyId)
            throw new BusinessRuleException("Screening is for users of a member company.");
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().FirstOrDefaultAsync(c => c.MemberCompanyId == companyId, ct)
            ?? throw new BusinessRuleException("Your member company was not found.");
        if (company.StatusCode != "ACTIVE")
            throw new BusinessRuleException("Your member company is suspended. Contact the federation.");
        return company;
    }

    public async Task<ScrInquiryDto> ScreenAsync(ScrInquiryRequest r, CancellationToken ct)
    {
        var receivedAt = clock.UtcNow;
        var company = await MyCompanyAsync(ct);
        return await ScreenForAsync(company, Me, currentUser.SessionId, "PORTAL", null, receivedAt, r, ct);
    }

    /// <summary>
    /// Screens for a company: from the Screening Portal (its signed-in user) or from the company's own system through the API (its API
    /// client). Both are kept, billed and audited the same way; the channel and the API client are kept with the inquiry.
    /// </summary>
    public async Task<ScrInquiryDto> ScreenForAsync(ScrMemberCompany company, string requestedBy, string? sessionId, string channel, long? apiClientId,
        DateTime receivedAt, ScrInquiryRequest r, CancellationToken ct)
    {
        var e = new Dictionary<string, string[]>();
        var type = ScrValidation.Code(r.SubjectTypeCode);
        var idType = ScrValidation.Code(r.IdentityTypeCode);
        var idNo = (r.IdentityNo ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var name = ScrValidation.Clean(r.SubjectName);
        var purpose = ScrValidation.Code(r.PurposeCode);
        var nationality = ScrValidation.Clean(r.NationalityCode)?.ToUpperInvariant();
        if (!ScrValidation.PartyTypes.Contains(type))
            e["subjectTypeCode"] = ["Choose person or company."];
        if (name is null)
            e["subjectName"] = ["Enter the full name."];
        ScrValidation.Identity(e, type, idType, idNo);
        ScrValidation.Nationality(e, nationality);
        if (!ScrValidation.Purposes.Contains(purpose))
            e["purposeCode"] = ["Choose the purpose."];
        else if (purpose == "OTHER" && ScrValidation.Clean(r.PurposeText) is null)
            e["purposeText"] = ["Describe the purpose."];
        ScrValidation.Throw(e);

        var (entries, version, _) = await staff.ListAsync(ct);
        var hits = ScrMatcher.Screen(new ScrSubject(type, name!, idNo, type == "PERSON" ? r.BirthDate : null), entries);
        var inquiry = new ScrInquiry
        {
            InquiryRef = "PENDING", MemberCompanyId = company.MemberCompanyId, RequestedBy = requestedBy, SubjectTypeCode = type, SubjectName = name!,
            IdentityTypeCode = idType, IdentityNo = idNo, BirthDate = type == "PERSON" ? r.BirthDate : null, NationalityCode = nationality,
            PurposeCode = purpose, PurposeText = ScrValidation.Clean(r.PurposeText), CaseReference = ScrValidation.Clean(r.CaseReference),
            ResultCode = ScrMatcher.Result(hits), RegistryVersionNo = version, ScreenedAt = clock.UtcNow,
            SessionId = sessionId, ReceivedAt = receivedAt, ChannelCode = channel, ApiClientId = apiClientId,
            Hits = [.. hits.Select(h => new ScrInquiryHit
            {
                PartyId = h.Entry.PartyId, DecisionId = h.Entry.DecisionId, MatchBasisCode = h.MatchBasisCode, NameScorePct = h.NameScorePct,
                MatchedName = h.MatchedName, EntryStatusCode = h.Entry.StatusCode,
            })],
        };

        await using var tx = await db.BeginTransactionAsync(ct);
        db.Add(inquiry);
        await db.SaveChangesAsync(ct);
        inquiry.InquiryRef = $"SCR-{inquiry.ScreenedAt.Year}-{inquiry.InquiryId.ToString("D6", CultureInfo.InvariantCulture)}";
        // the result is ready to send back: the time it took is kept with the inquiry (evidence of the service given)
        inquiry.RespondedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ToDto(inquiry, company.CompanyCode, hits.ToDictionary(h => (h.Entry.PartyId, h.Entry.DecisionId), h => h.Entry));
    }

    /// <summary>The signed-in company user's inquiries, newest first.</summary>
    public async Task<IReadOnlyList<ScrInquiryDto>> MineAsync(CancellationToken ct)
    {
        var company = await MyCompanyAsync(ct);
        return await ListAsync(db.Set<ScrInquiry>().Where(i => i.MemberCompanyId == company.MemberCompanyId), ct);
    }

    /// <summary>One inquiry: the company's own, or any for a federation auditor.</summary>
    public async Task<ScrInquiryDto> GetAsync(long inquiryId, bool anyCompany, CancellationToken ct)
    {
        var q = db.Set<ScrInquiry>().Where(i => i.InquiryId == inquiryId);
        if (!anyCompany)
        {
            var company = await MyCompanyAsync(ct);
            q = q.Where(i => i.MemberCompanyId == company.MemberCompanyId);
        }

        var found = await ListAsync(q, ct);
        return found.Count > 0 ? found[0] : throw new NotFoundException($"Inquiry {inquiryId} was not found.");
    }

    /// <summary>A company's inquiry by its reference (the API), or null.</summary>
    public async Task<ScrInquiryDto?> ByRefAsync(long companyId, string inquiryRef, CancellationToken ct)
    {
        var r = (inquiryRef ?? string.Empty).Trim().ToUpperInvariant();
        var found = await ListAsync(db.Set<ScrInquiry>().Where(i => i.MemberCompanyId == companyId && i.InquiryRef == r), ct);
        return found.Count > 0 ? found[0] : null;
    }

    /// <summary>A company's inquiries screened from one day to another (Cairo days), newest first, at most <paramref name="take"/> (the API).</summary>
    public Task<IReadOnlyList<ScrInquiryDto>> OfCompanyAsync(long companyId, DateOnly? from, DateOnly? to, int take, CancellationToken ct)
    {
        var q = db.Set<ScrInquiry>().Where(i => i.MemberCompanyId == companyId);
        if (from is { } f)
        {
            var start = ScrBilling.CairoStartUtc(f);
            q = q.Where(i => i.ScreenedAt >= start);
        }
        if (to is { } t)
        {
            var end = ScrBilling.CairoStartUtc(t.AddDays(1));
            q = q.Where(i => i.ScreenedAt < end);
        }
        return ListAsync(q, ct, Math.Clamp(take, 1, 500));
    }

    /// <summary>Every company's inquiries (federation audit), optionally of one company or one result.</summary>
    public Task<IReadOnlyList<ScrInquiryDto>> AllAsync(long? companyId, string? result, CancellationToken ct)
    {
        var q = db.Set<ScrInquiry>().AsQueryable();
        if (companyId is { } c)
            q = q.Where(i => i.MemberCompanyId == c);
        if (ScrValidation.Clean(result)?.ToUpperInvariant() is { } r)
            q = q.Where(i => i.ResultCode == r);
        return ListAsync(q, ct);
    }

    private async Task<IReadOnlyList<ScrInquiryDto>> ListAsync(IQueryable<ScrInquiry> q, CancellationToken ct, int take = 1000)
    {
        var list = await q.AsNoTracking().Include(i => i.Hits).OrderByDescending(i => i.InquiryId).Take(take).ToListAsync(ct);
        var companyIds = list.Select(i => i.MemberCompanyId).Distinct().ToList();
        var codes = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => companyIds.Contains(c.MemberCompanyId)).ToDictionaryAsync(c => c.MemberCompanyId, c => c.CompanyCode, ct);
        var partyIds = list.SelectMany(i => i.Hits.Select(h => h.PartyId)).Distinct().ToList();
        // a hit shows the entry as it was listed; the parties and decisions are read once for all of them
        var parties = await db.Set<ScrParty>().AsNoTracking().Where(p => partyIds.Contains(p.PartyId)).ToDictionaryAsync(p => p.PartyId, ct);
        var decisionIds = list.SelectMany(i => i.Hits.Select(h => h.DecisionId)).Distinct().ToList();
        var decisions = await db.Set<ScrDecision>().AsNoTracking().Where(d => decisionIds.Contains(d.DecisionId)).ToDictionaryAsync(d => d.DecisionId, ct);
        var entries = new Dictionary<(long, long), ScrRegistryEntry>();
        foreach (var h in list.SelectMany(i => i.Hits))
        {
            if (entries.ContainsKey((h.PartyId, h.DecisionId)) || !parties.TryGetValue(h.PartyId, out var p) || !decisions.TryGetValue(h.DecisionId, out var d))
                continue;
            entries[(h.PartyId, h.DecisionId)] = new ScrRegistryEntry(p.PartyId, d.DecisionId, d.DecisionRef, d.OfficialNo, p.PartyTypeCode, p.NameAr, p.NameEn, [],
                p.IdentityTypeCode, p.IdentityNo, p.NationalityCode, p.BirthDate, d.ScopeText, d.EffectiveDate, null, h.EntryStatusCode, d.LetterId, null);
        }

        return [.. list.Select(i => ToDto(i, codes.GetValueOrDefault(i.MemberCompanyId) ?? "-", entries))];
    }

    private static ScrInquiryDto ToDto(ScrInquiry i, string companyCode, IReadOnlyDictionary<(long, long), ScrRegistryEntry> entries) => new(
        i.InquiryId, i.InquiryRef, companyCode, i.RequestedBy, i.SubjectTypeCode, i.SubjectName, i.IdentityTypeCode, i.IdentityNo, i.BirthDate,
        i.NationalityCode, i.PurposeCode, i.PurposeText, i.CaseReference, i.ResultCode, i.RegistryVersionNo, i.ScreenedAt,
        [.. i.Hits.Select(h => entries.TryGetValue((h.PartyId, h.DecisionId), out var e)
            ? new ScrInquiryHitDto(h.MatchBasisCode, h.NameScorePct, h.MatchedName, h.EntryStatusCode, e.PartyTypeCode, e.NameAr, e.NameEn,
                e.IdentityTypeCode, e.IdentityNo, e.NationalityCode, e.DecisionRef, e.OfficialNo, e.ScopeText, e.ListedFrom, e.LiftedOn)
            : null).OfType<ScrInquiryHitDto>()], i.ChannelCode, i.ReceivedAt, i.RespondedAt);
}
