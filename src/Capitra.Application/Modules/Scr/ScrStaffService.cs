using System.Globalization;
using System.Security.Cryptography;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Scr;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// The federation side of screening (FINOVA's EFMF Operations Hub): incoming letters and their scans, parties, decisions and the
/// Federation Final List. A decision is entered as DRAFT and submitted to the CAPITRA workflow <see cref="WorkflowCode"/>; the
/// workflow's review and approval steps (<see cref="ScrDecisionWorkflowHandler"/>) publish it.
/// </summary>
public sealed class ScrStaffService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeRuntime runtime, IScrUserProfiles profiles,
    ScrStepService stepService)
{
    public const string WorkflowCode = "SCR_DECISION_APPROVAL";
    public const long MaxFileBytes = 20 * 1024 * 1024;

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    // ---------------------------------------------------------------- letters

    public async Task<IReadOnlyList<ScrLetterDto>> LettersAsync(string? search, CancellationToken ct)
    {
        var q = db.Set<ScrLetter>().AsNoTracking().Include(l => l.Files).AsQueryable();
        if (ScrValidation.Clean(search) is { } s)
            q = q.Where(l => l.LetterNo.Contains(s) || l.SenderName.Contains(s) || (l.SenderReference != null && l.SenderReference.Contains(s)));
        var letters = await q.OrderByDescending(l => l.LetterId).Take(500).ToListAsync(ct);
        var ids = letters.Select(l => (long?)l.LetterId).ToList();
        var counts = await db.Set<ScrDecision>().AsNoTracking().Where(d => ids.Contains(d.LetterId))
            .GroupBy(d => d.LetterId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key!.Value, x => x.Count, ct);
        return [.. letters.Select(l => ToDto(l, counts.GetValueOrDefault(l.LetterId)))];
    }

    public async Task<ScrLetterDto> CreateLetterAsync(ScrLetterRequest r, CancellationToken ct)
    {
        var e = new Dictionary<string, string[]>();
        var sender = ScrValidation.Clean(r.SenderName);
        var channel = ScrValidation.Code(r.ChannelCode);
        if (sender is null)
            e["senderName"] = ["Enter who sent the letter."];
        if (!ScrValidation.Channels.Contains(channel))
            e["channelCode"] = ["Choose how the letter came."];
        if (r.LetterDate is null)
            e["letterDate"] = ["Enter the letter's date."];
        else if (r.LetterDate > DateOnly.FromDateTime(clock.UtcNow))
            e["letterDate"] = ["The letter's date cannot be in the future."];
        ScrValidation.Throw(e);

        var letter = new ScrLetter
        {
            SenderName = sender!, SenderReference = ScrValidation.Clean(r.SenderReference), ChannelCode = channel, LetterDate = r.LetterDate!.Value,
            ReceivedAt = clock.UtcNow, Notes = ScrValidation.Clean(r.Notes), LetterNo = "PENDING",
        };
        await using var tx = await db.BeginTransactionAsync(ct);
        db.Add(letter);
        await db.SaveChangesAsync(ct);
        letter.LetterNo = "INT-" + letter.LetterId.ToString("D6", CultureInfo.InvariantCulture);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ToDto(letter, 0);
    }

    public async Task<ScrLetterDto> AddLetterFileAsync(long letterId, string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        var letter = await db.Set<ScrLetter>().Include(l => l.Files).FirstOrDefaultAsync(l => l.LetterId == letterId, ct)
            ?? throw new NotFoundException($"Letter {letterId} was not found.");
        if (content.Length == 0 || content.Length > MaxFileBytes)
            throw BusinessValidationException.For("file", "The file is empty or larger than 20 MB.");
        var mime = mimeType.Trim().ToLowerInvariant();
        if (mime is not ("application/pdf" or "image/png" or "image/jpeg" or "image/tiff"))
            throw BusinessValidationException.For("file", "Attach the scan as a PDF, PNG, JPEG or TIFF file.");
        letter.Files.Add(new ScrLetterFile
        {
            FileName = Path.GetFileName(fileName.Trim()), MimeType = mime, FileSizeBytes = content.Length,
            Sha256Hash = Convert.ToHexStringLower(SHA256.HashData(content)), FileBlob = content,
        });
        await db.SaveChangesAsync(ct);
        return ToDto(letter, await db.Set<ScrDecision>().CountAsync(d => d.LetterId == letterId, ct));
    }

    public async Task<ScrLetterFile> LetterFileAsync(long letterFileId, CancellationToken ct) =>
        await db.Set<ScrLetterFile>().AsNoTracking().FirstOrDefaultAsync(f => f.LetterFileId == letterFileId, ct)
        ?? throw new NotFoundException("The file was not found.");

    private static ScrLetterDto ToDto(ScrLetter l, int decisions) => new(
        l.LetterId, l.LetterNo, l.SenderReference, l.SenderName, l.ChannelCode, l.LetterDate, l.ReceivedAt, l.StatusCode, l.Notes,
        [.. l.Files.OrderBy(f => f.LetterFileId).Select(f => new ScrLetterFileDto(f.LetterFileId, f.FileName, f.MimeType, f.FileSizeBytes, f.Sha256Hash))],
        decisions);

    // ---------------------------------------------------------------- parties

    public async Task<IReadOnlyList<ScrPartyDto>> PartiesAsync(string? search, CancellationToken ct)
    {
        var q = db.Set<ScrParty>().AsNoTracking().Include(p => p.Aliases).AsQueryable();
        if (ScrValidation.Clean(search) is { } s)
            q = q.Where(p => p.NameAr.Contains(s) || (p.NameEn != null && p.NameEn.Contains(s)) || p.IdentityNo.Contains(s));
        return [.. (await q.OrderByDescending(p => p.PartyId).Take(500).ToListAsync(ct)).Select(ToDto)];
    }

    public async Task<ScrPartyDto> SavePartyAsync(long? partyId, ScrPartyRequest r, CancellationToken ct)
    {
        var e = new Dictionary<string, string[]>();
        var type = ScrValidation.Code(r.PartyTypeCode);
        var idType = ScrValidation.Code(r.IdentityTypeCode);
        var idNo = (r.IdentityNo ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var nameAr = ScrValidation.Clean(r.NameAr);
        var nationality = ScrValidation.Clean(r.NationalityCode)?.ToUpperInvariant() ?? "EG";
        if (!ScrValidation.PartyTypes.Contains(type))
            e["partyTypeCode"] = ["Choose person or company."];
        if (nameAr is null && ScrValidation.Clean(r.NameEn) is null)
            e["nameAr"] = ["Enter the name (Arabic, English or both)."];
        ScrValidation.Identity(e, type, idType, idNo);
        ScrValidation.Nationality(e, nationality);
        if (r.BirthDate is { } b && (type != "PERSON" || b > DateOnly.FromDateTime(clock.UtcNow)))
            e["birthDate"] = ["A birth date is for a person, and not in the future."];
        var normalized = ScrNames.Identity(idNo);
        if (await db.Set<ScrParty>().AnyAsync(p => p.PartyId != (partyId ?? 0) && p.PartyTypeCode == type && p.IdentityNo == idNo, ct))
            e["identityNo"] = [$"A {type.ToLowerInvariant()} with identity {idNo} is already registered."];
        ScrValidation.Throw(e);

        var party = partyId is { } id
            ? await db.Set<ScrParty>().Include(p => p.Aliases).FirstOrDefaultAsync(p => p.PartyId == id, ct) ?? throw new NotFoundException($"Party {id} was not found.")
            : new ScrParty();
        if (partyId is not null && await db.Set<ScrDecisionParty>().AnyAsync(dp => dp.PartyId == party.PartyId
                && db.Set<ScrDecision>().Any(d => d.DecisionId == dp.DecisionId && d.StatusCode == ScrDecisionStatus.Published), ct)
            && ScrNames.Identity(party.IdentityNo) != normalized)
            throw new BusinessRuleException("The identity of a party on a published decision cannot change; enter a decision instead.");

        party.PartyTypeCode = type;
        party.NameAr = nameAr ?? r.NameEn!.Trim();
        party.NameEn = ScrValidation.Clean(r.NameEn);
        party.IdentityTypeCode = idType;
        party.IdentityNo = idNo;
        party.NationalityCode = nationality;
        party.BirthDate = r.BirthDate;
        party.Notes = ScrValidation.Clean(r.Notes);
        var aliases = (r.Aliases ?? []).Select(ScrValidation.Clean).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        party.Aliases.RemoveAll(a => !aliases.Contains(a.AliasName, StringComparer.OrdinalIgnoreCase));
        foreach (var a in aliases.Where(a => !party.Aliases.Any(x => string.Equals(x.AliasName, a, StringComparison.OrdinalIgnoreCase))))
            party.Aliases.Add(new ScrPartyAlias { AliasName = a });
        if (partyId is null)
            db.Add(party);
        await db.SaveChangesAsync(ct);
        return ToDto(party);
    }

    private static ScrPartyDto ToDto(ScrParty p) => new(
        p.PartyId, p.PartyTypeCode, p.NameAr, p.NameEn, p.IdentityTypeCode, p.IdentityNo, p.NationalityCode, p.BirthDate, p.Notes,
        [.. p.Aliases.Select(a => a.AliasName)]);

    // ---------------------------------------------------------------- decisions

    public async Task<IReadOnlyList<ScrDecisionDto>> DecisionsAsync(string? status, CancellationToken ct)
    {
        var q = db.Set<ScrDecision>().AsNoTracking().Include(d => d.Parties).AsQueryable();
        if (ScrValidation.Clean(status)?.ToUpperInvariant() is { } s)
            q = s == ScrDecisionStatus.Pending ? q.Where(d => ScrDecisionStatus.Open.Contains(d.StatusCode)) : q.Where(d => d.StatusCode == s);
        return await ToDtosAsync(await q.OrderByDescending(d => d.DecisionId).Take(500).ToListAsync(ct), ct);
    }

    public async Task<ScrDecisionDto> DecisionAsync(long id, CancellationToken ct)
    {
        var d = await db.Set<ScrDecision>().AsNoTracking().Include(x => x.Parties).FirstOrDefaultAsync(x => x.DecisionId == id, ct)
            ?? throw new NotFoundException($"Decision {id} was not found.");
        return (await ToDtosAsync([d], ct))[0];
    }

    public async Task<ScrDecisionDto> SaveDecisionAsync(long? decisionId, ScrDecisionRequest r, CancellationToken ct)
    {
        var d = decisionId is { } id
            ? await db.Set<ScrDecision>().Include(x => x.Parties).FirstOrDefaultAsync(x => x.DecisionId == id, ct) ?? throw new NotFoundException($"Decision {id} was not found.")
            : new ScrDecision();
        if (decisionId is not null && d.StatusCode is not (ScrDecisionStatus.Draft or ScrDecisionStatus.Returned))
            throw new BusinessRuleException($"Decision {d.DecisionRef} is {d.StatusCode.ToLowerInvariant().Replace('_', ' ')} and cannot be changed.");

        var type = ScrValidation.Code(r.DecisionTypeCode);
        var partyIds = (r.PartyIds ?? []).Distinct().ToList();
        var e = new Dictionary<string, string[]>();
        if (!ScrDecisionType.All.Contains(type))
            e["decisionTypeCode"] = ["Choose the decision type."];
        if (ScrValidation.Clean(r.OfficialNo) is null)
            e["officialNo"] = ["Enter the official number on the order."];
        if (ScrValidation.Clean(r.ScopeText) is null && type != ScrDecisionType.Lifting)
            e["scopeText"] = ["Enter what the decision covers."];
        if (r.EffectiveDate is null)
            e["effectiveDate"] = ["Enter the date the decision takes effect."];
        if (partyIds.Count == 0)
            e["partyIds"] = ["Add at least one party."];
        if (r.LetterId is { } letterId && !await db.Set<ScrLetter>().AnyAsync(l => l.LetterId == letterId, ct))
            e["letterId"] = ["The letter was not found."];
        if (await db.Set<ScrParty>().CountAsync(p => partyIds.Contains(p.PartyId), ct) != partyIds.Count)
            e["partyIds"] = ["A party was not found."];

        ScrDecision? parent = null;
        if (type == ScrDecisionType.Listing)
        {
            if (r.ParentDecisionId is not null)
                e["parentDecisionId"] = ["A listing has no parent decision."];
        }
        else if (r.ParentDecisionId is not { } parentId)
            e["parentDecisionId"] = ["Choose the listing this decision changes."];
        else
        {
            parent = await db.Set<ScrDecision>().AsNoTracking().Include(x => x.Parties).FirstOrDefaultAsync(x => x.DecisionId == parentId, ct);
            if (parent is null || parent.DecisionTypeCode != ScrDecisionType.Listing || parent.StatusCode != ScrDecisionStatus.Published)
                e["parentDecisionId"] = ["The parent must be a published listing."];
            else if (partyIds.Except(parent.Parties.Select(p => p.PartyId)).Any())
                e["partyIds"] = ["Every party must be one of the listing's parties."];
        }
        ScrValidation.Throw(e);

        d.OfficialNo = r.OfficialNo!.Trim();
        d.DecisionTypeCode = type;
        d.ParentDecisionId = parent?.DecisionId;
        d.LetterId = r.LetterId;
        d.ScopeText = ScrValidation.Clean(r.ScopeText) ?? parent?.ScopeText ?? string.Empty;
        d.EffectiveDate = r.EffectiveDate!.Value;
        d.Parties.RemoveAll(p => !partyIds.Contains(p.PartyId));
        foreach (var pid in partyIds.Where(pid => d.Parties.All(p => p.PartyId != pid)))
            d.Parties.Add(new ScrDecisionParty { PartyId = pid });

        await using var tx = await db.BeginTransactionAsync(ct);
        if (decisionId is null)
        {
            var me = await PersonAsync(ct);
            d.EnteredBy = Me;
            d.EnteredByPerson = me;
            d.DecisionRef = "PENDING";
            db.Add(d);
            await db.SaveChangesAsync(ct);
            d.DecisionRef = "DEC-" + d.DecisionId.ToString("D6", CultureInfo.InvariantCulture);
        }

        if (d.LetterId is { } lid && await db.Set<ScrLetter>().FirstOrDefaultAsync(l => l.LetterId == lid, ct) is { StatusCode: "RECEIVED" } letter)
            letter.StatusCode = "INDEXED";
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await DecisionAsync(d.DecisionId, ct);
    }

    /// <summary>
    /// Run: starts a new SCR_DECISION_APPROVAL workflow instance for a draft or returned decision (from the list of decisions
    /// awaiting approval). Until then the decision shows "Workflow not started".
    /// </summary>
    public async Task<ScrDecisionDto> SubmitAsync(long decisionId, CancellationToken ct)
    {
        var d = await db.Set<ScrDecision>().FirstOrDefaultAsync(x => x.DecisionId == decisionId, ct)
            ?? throw new NotFoundException($"Decision {decisionId} was not found.");
        if (d.StatusCode is not (ScrDecisionStatus.Draft or ScrDecisionStatus.Returned))
            throw new BusinessRuleException($"The workflow of decision {d.DecisionRef} has already been started.");

        await using var tx = await db.BeginTransactionAsync(ct);
        var variables = new Dictionary<string, string?>
        {
            ["entity_id"] = d.DecisionId.ToString(CultureInfo.InvariantCulture),
            ["entity_ref"] = d.DecisionRef,
            ["entity_type"] = "SCR_DECISION",
            ["decision_type"] = d.DecisionTypeCode,
            ["official_no"] = d.OfficialNo,
            ["module_code"] = "SCREENING",
        };
        var instance = await runtime.StartAsync(WorkflowCode, d.DecisionRef, variables, Me, ct);
        d.WorkflowInstanceId = instance.WorkflowInstanceId;
        d.StatusCode = ScrDecisionStatus.InReview;
        d.ReturnReason = null;
        d.ReviewedBy = d.ReviewedByPerson = d.ApprovedBy = d.ApprovedByPerson = null;
        d.ReviewedAt = null;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await DecisionAsync(decisionId, ct);
    }

    public async Task<ScrDecisionDto> CancelAsync(long decisionId, CancellationToken ct)
    {
        var d = await db.Set<ScrDecision>().FirstOrDefaultAsync(x => x.DecisionId == decisionId, ct)
            ?? throw new NotFoundException($"Decision {decisionId} was not found.");
        if (d.StatusCode is not (ScrDecisionStatus.Draft or ScrDecisionStatus.Returned))
            throw new BusinessRuleException("Only a draft or returned decision can be cancelled.");
        d.StatusCode = ScrDecisionStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return await DecisionAsync(decisionId, ct);
    }

    private async Task<string> PersonAsync(CancellationToken ct)
    {
        var p = await profiles.ByUserNameAsync(Me, ct);
        return ScrDuties.PersonKey(p?.EmployeeNo, Me);
    }

    private async Task<List<ScrDecisionDto>> ToDtosAsync(List<ScrDecision> list, CancellationToken ct)
    {
        var partyIds = list.SelectMany(d => d.Parties.Select(p => p.PartyId)).Distinct().ToList();
        var parties = await db.Set<ScrParty>().AsNoTracking().Include(p => p.Aliases).Where(p => partyIds.Contains(p.PartyId)).ToDictionaryAsync(p => p.PartyId, ct);
        var parentIds = list.Select(d => d.ParentDecisionId).OfType<long>().Distinct().ToList();
        var parents = await db.Set<ScrDecision>().AsNoTracking().Where(d => parentIds.Contains(d.DecisionId)).ToDictionaryAsync(d => d.DecisionId, d => d.DecisionRef, ct);
        var letterIds = list.Select(d => d.LetterId).OfType<long>().Distinct().ToList();
        var letters = await db.Set<ScrLetter>().AsNoTracking().Where(l => letterIds.Contains(l.LetterId)).ToDictionaryAsync(l => l.LetterId, l => l.LetterNo, ct);
        var steps = await CurrentStepsAsync(list.Where(d => d.StatusCode is ScrDecisionStatus.InReview or ScrDecisionStatus.AwaitingApproval)
            .Select(d => d.WorkflowInstanceId).OfType<long>(), ct);
        var completed = await stepService.CompletedStepsAsync(list, ct);
        return [.. list.Select(d => new ScrDecisionDto(
            d.DecisionId, d.DecisionRef, d.OfficialNo, d.DecisionTypeCode, d.ParentDecisionId,
            d.ParentDecisionId is { } pid ? parents.GetValueOrDefault(pid) : null, d.LetterId, d.LetterId is { } lid ? letters.GetValueOrDefault(lid) : null,
            d.ScopeText, d.EffectiveDate, d.StatusCode, d.ReturnReason, d.WorkflowInstanceId, d.EnteredBy, d.ReviewedBy, d.ReviewedAt, d.ApprovedBy,
            d.PublishedAt, d.RegistryVersionNo,
            [.. d.Parties.Select(p => parties.TryGetValue(p.PartyId, out var x) ? ToDto(x) : null).OfType<ScrPartyDto>()], d.CreatedAt,
            d.WorkflowInstanceId is { } wid && steps.TryGetValue(wid, out var step) ? step.Name : null, completed.GetValueOrDefault(d.DecisionId),
            d.WorkflowInstanceId is { } hid && steps.TryGetValue(hid, out var held) ? held.Holder : null))];
    }

    /// <summary>
    /// Per workflow instance, its newest step not completed yet (as in Factoring) and who holds it: the user who took the task, else
    /// the user or role it is assigned to.
    /// </summary>
    private async Task<Dictionary<long, (string? Name, string? Holder)>> CurrentStepsAsync(IEnumerable<long> instanceIds, CancellationToken ct)
    {
        var ids = instanceIds.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return [];
        var open = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(x => ids.Contains(x.WorkflowInstanceId) && x.StepStatus != WfeRuntime.Completed && x.ElementType != "startEvent")
            .Select(x => new { x.WorkflowInstanceId, x.StepInstanceId, x.StepName })
            .ToListAsync(ct);
        var holders = await db.Set<WfeInboxItem>().AsNoTracking()
            .Where(x => ids.Contains(x.WorkflowInstanceId) && x.InboxStatus != WfeRuntime.Completed && x.InboxStatus != WfeRuntime.Cancelled)
            .Select(x => new { x.StepInstanceId, Holder = x.ClaimedBy ?? x.AssignedTo ?? x.AssignedRole })
            .ToListAsync(ct);
        return open.GroupBy(x => x.WorkflowInstanceId).ToDictionary(g => g.Key, g =>
        {
            var step = g.OrderByDescending(x => x.StepInstanceId).First();
            return (step.StepName, holders.FirstOrDefault(h => h.StepInstanceId == step.StepInstanceId)?.Holder);
        });
    }

    // ---------------------------------------------------------------- the Final List

    public async Task<List<ScrRegistryEntry>> EntriesAsync(CancellationToken ct) => (await ListAsync(ct)).Entries;

    public async Task<ScrRegistryDto> RegistryAsync(CancellationToken ct)
    {
        var (entries, version, publishedAt) = await ListAsync(ct);
        var pending = await db.Set<ScrDecision>().CountAsync(d => d.StatusCode == ScrDecisionStatus.InReview || d.StatusCode == ScrDecisionStatus.AwaitingApproval, ct);
        return new ScrRegistryDto(version, publishedAt, entries.Count(x => x.StatusCode == ScrRegistry.Active),
            entries.Count(x => x.StatusCode == ScrRegistry.Suspended), entries.Count(x => x.StatusCode == ScrRegistry.Lifted), pending, entries);
    }

    /// <summary>The Final List now, its version and when it was last published (used by the inquiry service too).</summary>
    public async Task<(List<ScrRegistryEntry> Entries, int Version, DateTime? PublishedAt)> ListAsync(CancellationToken ct)
    {
        var decisions = await db.Set<ScrDecision>().AsNoTracking().Include(d => d.Parties)
            .Where(d => d.StatusCode == ScrDecisionStatus.Published).ToListAsync(ct);
        var ids = decisions.SelectMany(d => d.Parties.Select(p => p.PartyId)).Distinct().ToList();
        var parties = await db.Set<ScrParty>().AsNoTracking().Include(p => p.Aliases).Where(p => ids.Contains(p.PartyId)).ToDictionaryAsync(p => p.PartyId, ct);
        var last = await db.Set<ScrRegistryVersion>().AsNoTracking().OrderByDescending(v => v.VersionNo).FirstOrDefaultAsync(ct);
        return (ScrRegistry.Derive(decisions, parties), last?.VersionNo ?? 0, last?.PublishedAt);
    }
}
