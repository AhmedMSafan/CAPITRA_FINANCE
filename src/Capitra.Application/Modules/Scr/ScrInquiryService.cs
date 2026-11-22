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
        var nameMin = r.NameMatchMinPct ?? ScrMatcher.DefaultNameMin;
        if (nameMin is < ScrMatcher.NameMinFloor or > 100)
            e["nameMatchMinPct"] = [$"Choose a name match from {ScrMatcher.NameMinFloor}% to 100%."];
        var idMin = r.IdMatchMinDigits ?? ScrMatcher.DefaultIdDigits;
        if (idMin is < 1 or > ScrMatcher.DefaultIdDigits)
            e["idMatchMinDigits"] = [$"Choose from 1 to {ScrMatcher.DefaultIdDigits} identity characters."];
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

        // the licence's yearly quota (licence years from its start day), the portal and the API together
        if (await QuotaAsync(company.MemberCompanyId, ct) is { Quota: { } quota } use && use.Used >= quota)
            throw new ScrQuotaExceededException(
                $"Your company has used its yearly quota of {quota:#,0} inquiries for {use.From:yyyy-MM-dd} to {use.To:yyyy-MM-dd}. Contact the Federation.");

        var (entries, version, _) = await staff.ListAsync(ct);
        var hits = ScrMatcher.Screen(new ScrSubject(type, name!, idNo, type == "PERSON" ? r.BirthDate : null, nationality), entries, nameMin, idMin);
        var inquiry = new ScrInquiry
        {
            InquiryRef = "PENDING", MemberCompanyId = company.MemberCompanyId, RequestedBy = requestedBy, SubjectTypeCode = type, SubjectName = name!,
            IdentityTypeCode = idType, IdentityNo = idNo, BirthDate = type == "PERSON" ? r.BirthDate : null, NationalityCode = nationality,
            PurposeCode = purpose, PurposeText = ScrValidation.Clean(r.PurposeText), CaseReference = ScrValidation.Clean(r.CaseReference),
            ResultCode = ScrMatcher.Result(hits), RegistryVersionNo = version, NameMatchMinPct = nameMin, IdMatchMinDigits = idMin, ScreenedAt = ScrInquirySeal.Micro(clock.UtcNow),
            SessionId = sessionId, ReceivedAt = ScrInquirySeal.Micro(receivedAt), ChannelCode = channel, ApiClientId = apiClientId,
            Hits = [.. hits.Select(h => new ScrInquiryHit
            {
                PartyId = h.Entry.PartyId, DecisionId = h.Entry.DecisionId, MatchBasisCode = h.MatchBasisCode, NameScorePct = Capitra.Application.Common.Money.Round(h.NameScorePct, 6),
                MatchedName = h.MatchedName, EntryStatusCode = h.Entry.StatusCode, IdMatchCode = h.IdMatchCode,
                IdScorePct = Capitra.Application.Common.Money.Round(h.IdScorePct, 6), IdDigitsDifferent = h.IdDigitsDifferent,
                BirthMatchCode = h.BirthMatchCode, NationalityMatchCode = h.NationalityMatchCode,
            })],
        };

        await using var tx = await db.BeginTransactionAsync(ct);
        // one inquiry at a time is sealed, so each is chained to the one before it
        await db.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(7300101)", ct);
        db.Add(inquiry);
        await db.SaveChangesAsync(ct);
        inquiry.InquiryRef = $"SCR-{inquiry.ScreenedAt.Year}-{inquiry.InquiryId.ToString("D6", CultureInfo.InvariantCulture)}";
        // the result is ready to send back: the time it took is kept with the inquiry (evidence of the service given)
        inquiry.RespondedAt = ScrInquirySeal.Micro(clock.UtcNow);
        inquiry.PrevHash = await db.Set<ScrInquiry>().AsNoTracking().Where(i => i.RecordHash != null).OrderByDescending(i => i.InquiryId)
            .Select(i => i.RecordHash).FirstOrDefaultAsync(ct);
        inquiry.RecordHash = ScrInquirySeal.Hash(inquiry, inquiry.PrevHash);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ToDto(inquiry, company, hits.ToDictionary(h => (h.Entry.PartyId, h.Entry.DecisionId), h => h.Entry));
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

    /// <summary>
    /// The licence's yearly inquiry quota and what is used of it: the licence year runs from the licence's start day, a year at a time;
    /// null when the company has no licence covering today. Quota is null when the licence has none.
    /// </summary>
    public async Task<ScrQuotaDto?> QuotaAsync(long companyId, CancellationToken ct)
    {
        var licence = await db.Set<ScrCompanyLicence>().AsNoTracking().Where(l => l.MemberCompanyId == companyId).OrderByDescending(l => l.LicenceId)
            .FirstOrDefaultAsync(ct);
        var today = ScrBilling.CairoDate(clock.UtcNow);
        if (licence is null || today < licence.ValidFrom)
            return null;
        var years = today.Year - licence.ValidFrom.Year;
        var from = licence.ValidFrom.AddYears(years);
        if (from > today)
            from = licence.ValidFrom.AddYears(years - 1);
        var to = from.AddYears(1).AddDays(-1);
        var start = ScrBilling.CairoStartUtc(from);
        var end = ScrBilling.CairoStartUtc(to.AddDays(1));
        var used = await db.Set<ScrInquiry>().AsNoTracking().CountAsync(i => i.MemberCompanyId == companyId && i.ScreenedAt >= start && i.ScreenedAt < end, ct);
        return new ScrQuotaDto(licence.YearlyInquiryQuota, used, licence.YearlyInquiryQuota is { } q ? Math.Max(0, q - used) : null, from, to);
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
        // FOUND / NOT_FOUND also take the results of inquiries made before 23 Nov 2026
        string[]? codes = ScrValidation.Clean(result)?.ToUpperInvariant() switch
        {
            null => null,
            ScrMatcher.Found => [ScrMatcher.Found, "MATCH", "POSSIBLE_MATCH", "PREVIOUSLY_LISTED"],
            ScrMatcher.NotFound => [ScrMatcher.NotFound, "NO_MATCH"],
            var r => [r],
        };
        if (codes is not null)
            q = q.Where(i => codes.Contains(i.ResultCode));
        return ListAsync(q, ct);
    }

    private async Task<IReadOnlyList<ScrInquiryDto>> ListAsync(IQueryable<ScrInquiry> q, CancellationToken ct, int take = 1000)
    {
        var list = await q.AsNoTracking().Include(i => i.Hits).OrderByDescending(i => i.InquiryId).Take(take).ToListAsync(ct);
        var companyIds = list.Select(i => i.MemberCompanyId).Distinct().ToList();
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => companyIds.Contains(c.MemberCompanyId)).ToDictionaryAsync(c => c.MemberCompanyId, ct);
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

        return [.. list.Select(i => ToDto(i, companies.GetValueOrDefault(i.MemberCompanyId), entries))];
    }

    private static ScrInquiryDto ToDto(ScrInquiry i, ScrMemberCompany? company, IReadOnlyDictionary<(long, long), ScrRegistryEntry> entries) => new(
        i.InquiryId, i.InquiryRef, company?.CompanyCode ?? "-", i.RequestedBy, i.SubjectTypeCode, i.SubjectName, i.IdentityTypeCode, i.IdentityNo, i.BirthDate,
        i.NationalityCode, i.PurposeCode, i.PurposeText, i.CaseReference, i.ResultCode, i.RegistryVersionNo, i.ScreenedAt,
        [.. i.Hits.Select(h => entries.TryGetValue((h.PartyId, h.DecisionId), out var e)
            ? new ScrInquiryHitDto(h.MatchBasisCode, h.NameScorePct, h.MatchedName, h.EntryStatusCode, e.PartyTypeCode, e.NameAr, e.NameEn,
                e.IdentityTypeCode, e.IdentityNo, e.NationalityCode, e.DecisionRef, e.OfficialNo, e.ScopeText, e.ListedFrom, e.LiftedOn,
                e.BirthDate, h.IdMatchCode, h.IdScorePct, h.IdDigitsDifferent, h.BirthMatchCode, h.NationalityMatchCode)
            : null).OfType<ScrInquiryHitDto>()], i.ChannelCode, i.ReceivedAt, i.RespondedAt, i.RecordHash, i.MemberCompanyId, company?.NameEn, company?.NameAr, i.NameMatchMinPct, i.IdMatchMinDigits);

    // ---- seals ------------------------------------------------------------------------------------------------------------------

    /// <summary>Checks one inquiry's seal: its hash recomputed and its link to the sealed inquiry before it (a company user: own only).</summary>
    public async Task<ScrInquirySealDto> SealAsync(long inquiryId, bool anyCompany, CancellationToken ct)
    {
        var q = db.Set<ScrInquiry>().AsNoTracking().Include(i => i.Hits).Where(i => i.InquiryId == inquiryId);
        if (!anyCompany)
        {
            var company = await MyCompanyAsync(ct);
            q = q.Where(i => i.MemberCompanyId == company.MemberCompanyId);
        }
        var i = await q.FirstOrDefaultAsync(ct) ?? throw new NotFoundException($"Inquiry {inquiryId} was not found.");
        if (i.RecordHash is null)
            return new ScrInquirySealDto(i.InquiryRef, false, false, null, null, "Made before inquiries were sealed: no seal to check.", clock.UtcNow);
        var before = await db.Set<ScrInquiry>().AsNoTracking().Where(x => x.InquiryId < inquiryId && x.RecordHash != null).OrderByDescending(x => x.InquiryId)
            .Select(x => x.RecordHash).FirstOrDefaultAsync(ct);
        var valid = ScrInquirySeal.Hash(i, i.PrevHash) == i.RecordHash && i.PrevHash == before;
        return new ScrInquirySealDto(i.InquiryRef, true, valid, i.RecordHash, i.PrevHash,
            valid ? "The seal is intact: the inquiry is as it was made." : "The seal does not match: the inquiry or the chain was changed.", clock.UtcNow);
    }

    /// <summary>Checks the whole chain, oldest first (federation audit).</summary>
    public async Task<ScrSealCheckDto> SealCheckAsync(CancellationToken ct)
    {
        var broken = new List<string>();
        var unsealed = new List<string>();
        int checkedCount = 0, unsealedBefore = 0;
        string? prev = null, first = null, last = null;
        long after = 0;
        while (true)
        {
            var batch = await db.Set<ScrInquiry>().AsNoTracking().Include(i => i.Hits).Where(i => i.InquiryId > after).OrderBy(i => i.InquiryId).Take(500).ToListAsync(ct);
            if (batch.Count == 0)
                break;
            foreach (var i in batch)
            {
                if (i.RecordHash is null)
                {
                    if (first is null) unsealedBefore++;
                    else unsealed.Add(i.InquiryRef);
                    continue;
                }
                checkedCount++;
                first ??= i.InquiryRef;
                last = i.InquiryRef;
                if (i.PrevHash != prev || ScrInquirySeal.Hash(i, i.PrevHash) != i.RecordHash)
                    broken.Add(i.InquiryRef);
                prev = i.RecordHash;
            }
            after = batch[^1].InquiryId;
        }
        return new ScrSealCheckDto(checkedCount, broken.Count, [.. broken.Take(100)], unsealed.Count, [.. unsealed.Take(100)], unsealedBefore, first, last, clock.UtcNow);
    }
}
