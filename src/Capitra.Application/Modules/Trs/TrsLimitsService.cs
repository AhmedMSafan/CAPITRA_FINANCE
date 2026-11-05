using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Trs;

public sealed record TrsOptionDto(string Value, string Label);

public sealed record TrsSublimitNodeDto(
    long SublimitId, long TrancheId, string SublimitName, decimal LimitAmount, string PoolTypeCode, string StatusCode, string? Notes,
    IReadOnlyList<string> BusinessLines, decimal Outstanding, decimal Reserved, decimal Available);

public sealed record TrsTrancheNodeDto(
    long TrancheId, long FacilityId, string TrancheName, decimal TrancheAmount, string StatusCode, DateOnly? ReleasedDate, DateOnly? MaturityDate,
    string? Notes, decimal Outstanding, decimal Reserved, decimal Available, IReadOnlyList<TrsSublimitNodeDto> Sublimits);

public sealed record TrsFacilityNodeDto(
    long FacilityId, long BankId, string FacilityRef, string FacilityTypeCode, string CurrencyCode, decimal ApprovedLimit, string InterestRateType,
    string? BaseRateCode, decimal? MarginBps, decimal? AllInRatePct, DateOnly StartDate, DateOnly MaturityDate, string FacilityStatus, bool IsRevolving,
    decimal Outstanding, decimal Reserved, decimal Available, decimal NotReleased, IReadOnlyList<TrsTrancheNodeDto> Tranches);

/// <summary>The highest exposure under a concentration rule right now; Level is OK, WARN or BREACH.</summary>
public sealed record TrsRuleUsageDto(string SubjectName, decimal Exposure, decimal? Pct, string Level);

public sealed record TrsBankRuleDto(
    long RuleId, long BankId, long? FacilityId, string? FacilityRef, string RuleTypeCode, string SubjectTypeCode, long? SubjectClientId,
    string? SubjectValue, string SubjectLabel, decimal? LimitPct, decimal? LimitAmount, decimal? WarningPct, int? MaxTenorDays, string BasisCode,
    string BreachActionCode, string StatusCode, string ChangedBy, DateTime ChangedAt, string? VerifiedBy, DateTime? VerifiedAt, string? Notes,
    bool CanVerify, string? VerifyBlockedReason, TrsRuleUsageDto? Highest);

public sealed record TrsBankNodeDto(
    long BankId, string BankCode, string BankName, decimal ApprovedLimit, decimal Outstanding, decimal Reserved, decimal Available,
    IReadOnlyList<TrsFacilityNodeDto> Facilities, IReadOnlyList<TrsBankRuleDto> Rules);

public sealed record TrsLimitsDto(IReadOnlyList<TrsBankNodeDto> Banks);

public sealed record TrsSubjectOptionsDto(IReadOnlyList<TrsClientOptionDto> Clients, IReadOnlyList<string> Groups);

public sealed record TrsClientOptionDto(long ClientId, string ClientNumber, string ClientName);

public sealed record TrsLimitsOptionsDto(
    IReadOnlyList<TrsOptionDto> BusinessLines, IReadOnlyList<TrsOptionDto> FacilityTypes, IReadOnlyList<TrsOptionDto> RuleTypes,
    IReadOnlyList<TrsOptionDto> SubjectTypes, IReadOnlyList<TrsOptionDto> Bases, IReadOnlyList<TrsOptionDto> BreachActions,
    TrsSubjectOptionsDto Subjects);

public sealed record TrsFacilityFormDto(
    long BankId, string FacilityRef, string FacilityTypeCode, string CurrencyCode, decimal ApprovedLimit, string InterestRateType,
    string? BaseRateCode, decimal? MarginBps, decimal? AllInRatePct, DateOnly StartDate, DateOnly MaturityDate, bool IsRevolving, string FacilityStatus);

public sealed record TrsTrancheFormDto(string TrancheName, decimal TrancheAmount, string StatusCode, DateOnly? ReleasedDate, DateOnly? MaturityDate,
    string? Notes);

public sealed record TrsSublimitFormDto(string SublimitName, decimal LimitAmount, string PoolTypeCode, string StatusCode, IReadOnlyList<string>? BusinessLines,
    string? Notes);

public sealed record TrsBankRuleFormDto(
    long BankId, long? FacilityId, string RuleTypeCode, string SubjectTypeCode, long? SubjectClientId, string? SubjectValue, decimal? LimitPct,
    decimal? LimitAmount, decimal? WarningPct, int? MaxTenorDays, string BasisCode, string BreachActionCode, string? Notes);

public sealed record TrsNotesDto(string? Notes);

/// <summary>
/// Treasury Bank Limits (agreed with Ahmed, 5 Nov 2026): each bank's facilities as a tree of facility, tranches (what the bank released)
/// and sublimits (which business lines a slice may fund), with live usage from TRS_SUBLIMIT_MOVEMENTS, and the bank's rules (concentration,
/// exclusions, tenor). What can be drawn at any node is the smallest of what is free at that node and at every node above it. A bank
/// rule is checked only once a second user has verified it.
/// </summary>
public sealed class TrsLimitsService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    public static readonly IReadOnlyList<TrsOptionDto> BusinessLines =
    [
        new("FACTORING", "Factoring"), new("REVERSE_FACTORING", "Reverse Factoring"), new("LEASING", "Leasing"),
        new("MORTGAGE", "Mortgage"), new("SUBSIDIZED_MORTGAGE", "Subsidized Mortgage"),
    ];

    public static readonly IReadOnlyList<TrsOptionDto> FacilityTypes =
    [
        new("RCF", "Revolving credit"), new("TERM_LOAN", "Term loan"), new("OVERDRAFT", "Overdraft"), new("WORKING_CAPITAL", "Working capital"),
        new("REVOLVING", "Revolving"), new("MIXED", "Mixed"),
    ];

    public static readonly IReadOnlyList<TrsOptionDto> RuleTypes =
        [new("CONCENTRATION", "Concentration"), new("EXCLUDE", "Exclude"), new("MAX_TENOR", "Maximum tenor")];

    public static readonly IReadOnlyList<TrsOptionDto> SubjectTypes =
        [new("DEBTOR", "Debtor"), new("SELLER", "Seller"), new("DEBTOR_GROUP", "Debtor group"), new("INVOICE", "Invoice")];

    public static readonly IReadOnlyList<TrsOptionDto> Bases = [new("FUNDED_POOL", "Funded pool"), new("FACILITY_LIMIT", "Facility limit")];

    public static readonly IReadOnlyList<TrsOptionDto> BreachActions =
        [new("BLOCK", "Block"), new("OVERRIDE", "Needs override"), new("WARN", "Warn")];

    private static readonly string[] TrancheStatuses = ["DRAFT", "ACTIVE", "CLOSED"];
    private static readonly string[] SublimitStatuses = ["ACTIVE", "SUSPENDED", "CLOSED"];
    private static readonly string[] PoolTypes = ["DEDICATED", "SHARED"];
    private static readonly string[] FacilityStatuses = ["TERM_SHEET", "ACTIVE", "SUSPENDED", "UNDER_RENEWAL", "EXPIRED", "CLOSED"];
    private static readonly string[] RateTypes = ["FIXED", "FLOATING"];

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    // ---------------------------------------------------------------- read

    public async Task<TrsLimitsOptionsDto> OptionsAsync(CancellationToken cancellationToken)
    {
        var clients = await db.Set<CrmClient>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ClientName)
            .Select(x => new TrsClientOptionDto(x.ClientId, x.ClientNumber, x.ClientName)).ToListAsync(cancellationToken);
        var groups = await db.Set<CrmClient>().AsNoTracking().Where(x => x.GroupName != null && x.GroupName != "").Select(x => x.GroupName!)
            .Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        return new TrsLimitsOptionsDto(BusinessLines, FacilityTypes, RuleTypes, SubjectTypes, Bases, BreachActions, new TrsSubjectOptionsDto(clients, groups));
    }

    public async Task<TrsLimitsDto> GetAsync(CancellationToken cancellationToken)
    {
        var banks = await db.Set<TrsBank>().AsNoTracking().OrderBy(x => x.BankNameEn).ToListAsync(cancellationToken);
        var facilities = await db.Set<TrsCreditFacility>().AsNoTracking().Where(x => x.FacilityStatus != "CLOSED")
            .OrderBy(x => x.FacilityRef).ToListAsync(cancellationToken);
        var tranches = await db.Set<TrsFacilityTranche>().AsNoTracking().OrderBy(x => x.TrancheName).ToListAsync(cancellationToken);
        var sublimits = await db.Set<TrsFacilitySublimit>().AsNoTracking().OrderBy(x => x.SublimitName).ToListAsync(cancellationToken);
        var scopes = (await db.Set<TrsSublimitScope>().AsNoTracking().ToListAsync(cancellationToken)).ToLookup(x => x.SublimitId, x => x.ScopeValue);
        var usage = await db.Set<TrsSublimitMovement>().AsNoTracking().GroupBy(x => x.SublimitId)
            .Select(g => new { SublimitId = g.Key, Out = g.Sum(x => x.OutstandingChange), Res = g.Sum(x => x.ReservedChange) })
            .ToDictionaryAsync(x => x.SublimitId, x => (x.Out, x.Res), cancellationToken);
        var rules = await db.Set<TrsBankRule>().AsNoTracking().Where(x => x.StatusCode != "INACTIVE").OrderBy(x => x.RuleId).ToListAsync(cancellationToken);

        var facilityNodes = facilities.Select(f => FacilityNode(f, tranches, sublimits, scopes, usage)).ToList();
        var ruleDtos = await RulesAsync(rules, facilities, sublimits, cancellationToken);

        var result = banks.Select(b =>
        {
            var fs = facilityNodes.Where(x => x.BankId == b.BankId).ToList();
            return new TrsBankNodeDto(b.BankId, b.BankCode, b.BankNameEn, fs.Sum(x => x.ApprovedLimit), fs.Sum(x => x.Outstanding), fs.Sum(x => x.Reserved),
                fs.Sum(x => x.Available), fs, [.. ruleDtos.Where(x => x.BankId == b.BankId)]);
        }).Where(x => x.Facilities.Count > 0 || x.Rules.Count > 0).ToList();
        return new TrsLimitsDto(result);
    }

    private static TrsFacilityNodeDto FacilityNode(TrsCreditFacility f, List<TrsFacilityTranche> tranches, List<TrsFacilitySublimit> sublimits,
        ILookup<long, string> scopes, Dictionary<long, (decimal Out, decimal Res)> usage)
    {
        var fSubs = sublimits.Where(x => x.FacilityId == f.FacilityId).ToList();
        decimal Out(IEnumerable<TrsFacilitySublimit> xs) => xs.Sum(x => usage.GetValueOrDefault(x.SublimitId).Out);
        decimal Res(IEnumerable<TrsFacilitySublimit> xs) => xs.Sum(x => usage.GetValueOrDefault(x.SublimitId).Res);

        var fOut = Out(fSubs);
        var fRes = Res(fSubs);
        var fAvail = f.FacilityStatus == "ACTIVE" ? Math.Max(0, f.ApprovedLimit - fOut - fRes) : 0;

        var tNodes = tranches.Where(x => x.FacilityId == f.FacilityId).Select(t =>
        {
            var tSubs = fSubs.Where(x => x.TrancheId == t.TrancheId).ToList();
            var tOut = Out(tSubs);
            var tRes = Res(tSubs);
            var tAvail = t.StatusCode == "ACTIVE" ? Math.Min(Math.Max(0, t.TrancheAmount - tOut - tRes), fAvail) : 0;
            var sNodes = tSubs.Select(s =>
            {
                var (sOut, sRes) = usage.GetValueOrDefault(s.SublimitId);
                var sAvail = s.StatusCode == "ACTIVE" ? Math.Min(Math.Max(0, s.LimitAmount - sOut - sRes), tAvail) : 0;
                return new TrsSublimitNodeDto(s.SublimitId, s.TrancheId, s.SublimitName, s.LimitAmount, s.PoolTypeCode, s.StatusCode, s.Notes,
                    [.. scopes[s.SublimitId].Order()], sOut, sRes, sAvail);
            }).ToList();
            return new TrsTrancheNodeDto(t.TrancheId, t.FacilityId, t.TrancheName, t.TrancheAmount, t.StatusCode, t.ReleasedDate, t.MaturityDate, t.Notes,
                tOut, tRes, tAvail, sNodes);
        }).ToList();

        // only what is released in an ACTIVE tranche can be drawn; the rest of the facility is waiting for the bank
        var released = tNodes.Where(x => x.StatusCode == "ACTIVE").Sum(x => x.TrancheAmount);
        var drawable = Math.Min(fAvail, tNodes.Sum(x => x.Available));
        var notReleased = Math.Max(0, f.ApprovedLimit - released);
        return new TrsFacilityNodeDto(f.FacilityId, f.BankId, f.FacilityRef, f.FacilityTypeCode, f.CurrencyCode, f.ApprovedLimit, f.InterestRateType,
            f.BaseRateCode, f.MarginBps, f.AllInRatePct, f.StartDate, f.MaturityDate, f.FacilityStatus, f.IsRevolving, fOut, fRes, drawable, notReleased, tNodes);
    }

    private async Task<List<TrsBankRuleDto>> RulesAsync(List<TrsBankRule> rules, List<TrsCreditFacility> facilities, List<TrsFacilitySublimit> sublimits,
        CancellationToken cancellationToken)
    {
        if (rules.Count == 0) return [];
        var refs = facilities.ToDictionary(x => x.FacilityId, x => x.FacilityRef);
        var subjectIds = rules.Where(x => x.SubjectClientId is not null).Select(x => x.SubjectClientId!.Value).ToHashSet();

        // exposure by debtor and seller per sublimit; a debtor's group comes from its customer record
        var exposures = await db.Set<TrsSublimitMovement>().AsNoTracking()
            .Where(x => x.DebtorClientId != null || x.SellerClientId != null)
            .GroupBy(x => new { x.SublimitId, x.DebtorClientId, x.SellerClientId })
            .Select(g => new { g.Key.SublimitId, g.Key.DebtorClientId, g.Key.SellerClientId, Amount = g.Sum(x => x.OutstandingChange + x.ReservedChange) })
            .ToListAsync(cancellationToken);
        var poolBySub = await db.Set<TrsSublimitMovement>().AsNoTracking().GroupBy(x => x.SublimitId)
            .Select(g => new { g.Key, Amount = g.Sum(x => x.OutstandingChange + x.ReservedChange) }).ToDictionaryAsync(x => x.Key, x => x.Amount, cancellationToken);
        var clientIds = exposures.SelectMany(x => new[] { x.DebtorClientId, x.SellerClientId }).Where(x => x is not null).Select(x => x!.Value)
            .Concat(subjectIds).ToHashSet();
        var clients = await db.Set<CrmClient>().AsNoTracking().Where(x => clientIds.Contains(x.ClientId))
            .Select(x => new { x.ClientId, x.ClientName, x.GroupName }).ToDictionaryAsync(x => x.ClientId, cancellationToken);
        var subFacility = sublimits.ToDictionary(x => x.SublimitId, x => x.FacilityId);

        return [.. rules.Select(r =>
        {
            var inScope = facilities.Where(f => f.BankId == r.BankId && (r.FacilityId is null || f.FacilityId == r.FacilityId)).Select(f => f.FacilityId).ToHashSet();
            var subject = r.SubjectTypeCode switch
            {
                "INVOICE" => "Invoice tenor",
                "DEBTOR_GROUP" => r.SubjectValue is null ? "Any single debtor group" : $"Debtor group: {r.SubjectValue}",
                _ => r.SubjectClientId is long id
                    ? $"{(r.SubjectTypeCode == "SELLER" ? "Seller" : "Debtor")}: {(clients.TryGetValue(id, out var c) ? c.ClientName : id.ToString())}"
                    : r.SubjectTypeCode == "SELLER" ? "Any single seller" : "Any single debtor",
            };

            TrsRuleUsageDto? highest = null;
            if (r.RuleTypeCode == "CONCENTRATION")
            {
                var rows = exposures.Where(x => subFacility.TryGetValue(x.SublimitId, out var fid) && inScope.Contains(fid));
                IEnumerable<(string Key, string Name, decimal Amount)> bySubject = r.SubjectTypeCode switch
                {
                    "SELLER" => rows.Where(x => x.SellerClientId is not null).GroupBy(x => x.SellerClientId!.Value)
                        .Select(g => (g.Key.ToString(), clients.TryGetValue(g.Key, out var c) ? c.ClientName : g.Key.ToString(), g.Sum(x => x.Amount))),
                    "DEBTOR_GROUP" => rows.Where(x => x.DebtorClientId is not null && clients.TryGetValue(x.DebtorClientId.Value, out var c) && c.GroupName != null)
                        .GroupBy(x => clients[x.DebtorClientId!.Value].GroupName!).Select(g => (g.Key, g.Key, g.Sum(x => x.Amount))),
                    _ => rows.Where(x => x.DebtorClientId is not null).GroupBy(x => x.DebtorClientId!.Value)
                        .Select(g => (g.Key.ToString(), clients.TryGetValue(g.Key, out var c) ? c.ClientName : g.Key.ToString(), g.Sum(x => x.Amount))),
                };
                if (r.SubjectClientId is long sid) bySubject = bySubject.Where(x => x.Key == sid.ToString());
                if (r.SubjectValue is string sv && r.SubjectTypeCode == "DEBTOR_GROUP") bySubject = bySubject.Where(x => x.Key == sv);
                var top = bySubject.OrderByDescending(x => x.Amount).FirstOrDefault();
                if (top.Name is not null && top.Amount > 0)
                {
                    var denominator = r.BasisCode == "FACILITY_LIMIT"
                        ? facilities.Where(f => inScope.Contains(f.FacilityId)).Sum(f => f.ApprovedLimit)
                        : poolBySub.Where(x => subFacility.TryGetValue(x.Key, out var fid) && inScope.Contains(fid)).Sum(x => x.Value);
                    decimal? pct = denominator > 0 ? Math.Round(top.Amount / denominator * 100, 1) : null;
                    var level = (r.LimitPct, r.LimitAmount) switch
                    {
                        (decimal lp, _) when pct > lp => "BREACH",
                        (_, decimal la) when top.Amount > la => "BREACH",
                        _ when r.WarningPct is decimal wp && pct >= wp => "WARN",
                        _ => "OK",
                    };
                    highest = new TrsRuleUsageDto(top.Name, top.Amount, pct, level);
                }
            }

            var (canVerify, blocked) = r.StatusCode != "PENDING"
                ? (false, "The rule is already verified.")
                : SameUser(r.ChangedBy) ? (false, "You made the last change; another user must verify it.") : (true, (string?)null);
            return new TrsBankRuleDto(r.RuleId, r.BankId, r.FacilityId, r.FacilityId is long fid2 ? refs.GetValueOrDefault(fid2) : null, r.RuleTypeCode,
                r.SubjectTypeCode, r.SubjectClientId, r.SubjectValue, subject, r.LimitPct, r.LimitAmount, r.WarningPct, r.MaxTenorDays, r.BasisCode,
                r.BreachActionCode, r.StatusCode, r.ChangedBy, r.ChangedAt, r.VerifiedBy, r.VerifiedAt, r.Notes, canVerify, blocked, highest);
        })];
    }

    // ---------------------------------------------------------------- facilities

    public async Task<long> SaveFacilityAsync(long? facilityId, TrsFacilityFormDto dto, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        void Err(string field, string message) => errors[field] = [message];
        var reference = dto.FacilityRef?.Trim() ?? "";
        if (reference.Length == 0) Err("facilityRef", "Enter the facility reference.");
        if (!await db.Set<TrsBank>().AnyAsync(x => x.BankId == dto.BankId, cancellationToken)) Err("bankId", "Choose the bank.");
        if (FacilityTypes.All(x => x.Value != dto.FacilityTypeCode)) Err("facilityTypeCode", "Choose the facility type.");
        if (string.IsNullOrWhiteSpace(dto.CurrencyCode) || dto.CurrencyCode.Trim().Length != 3) Err("currencyCode", "Enter the 3-letter currency code.");
        if (dto.ApprovedLimit <= 0) Err("approvedLimit", "Enter the approved limit.");
        if (!RateTypes.Contains(dto.InterestRateType)) Err("interestRateType", "Choose fixed or floating.");
        if (dto.MaturityDate <= dto.StartDate) Err("maturityDate", "The maturity date must be after the start date.");
        if (!FacilityStatuses.Contains(dto.FacilityStatus)) Err("facilityStatus", "Choose the status.");
        if (reference.Length > 0 && await db.Set<TrsCreditFacility>().AnyAsync(x => x.FacilityRef == reference && x.FacilityId != facilityId, cancellationToken))
            Err("facilityRef", "Another facility has this reference.");

        TrsCreditFacility? f = null;
        if (facilityId is long id)
        {
            f = await db.Set<TrsCreditFacility>().SingleOrDefaultAsync(x => x.FacilityId == id, cancellationToken)
                ?? throw new NotFoundException($"Facility {id} was not found.");
            var tranchesTotal = await db.Set<TrsFacilityTranche>().Where(x => x.FacilityId == id && x.StatusCode != "CLOSED")
                .SumAsync(x => (decimal?)x.TrancheAmount, cancellationToken) ?? 0;
            if (dto.ApprovedLimit < tranchesTotal) Err("approvedLimit", $"The tranches already add up to {tranchesTotal:N0}; the limit cannot be lower.");
        }
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        if (f is null)
        {
            f = new TrsCreditFacility { CompanyId = 1, IsActive = true };
            db.Set<TrsCreditFacility>().Add(f);
        }
        f.BankId = dto.BankId;
        f.FacilityRef = reference;
        f.FacilityTypeCode = dto.FacilityTypeCode;
        f.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
        f.ApprovedLimit = dto.ApprovedLimit;
        f.InterestRateType = dto.InterestRateType;
        f.BaseRateCode = Blank(dto.BaseRateCode);
        f.MarginBps = dto.MarginBps;
        f.AllInRatePct = dto.AllInRatePct;
        f.StartDate = dto.StartDate;
        f.MaturityDate = dto.MaturityDate;
        f.IsRevolving = dto.IsRevolving;
        f.FacilityStatus = dto.FacilityStatus;
        f.AvailableBalance = Math.Max(0, f.ApprovedLimit - f.OutstandingBalance - f.ReservedBalance);
        await db.SaveChangesAsync(cancellationToken);
        return f.FacilityId;
    }

    // ---------------------------------------------------------------- tranches

    public async Task<long> SaveTrancheAsync(long facilityId, long? trancheId, TrsTrancheFormDto dto, CancellationToken cancellationToken)
    {
        var f = await db.Set<TrsCreditFacility>().AsNoTracking().SingleOrDefaultAsync(x => x.FacilityId == facilityId, cancellationToken)
            ?? throw new NotFoundException($"Facility {facilityId} was not found.");
        var errors = new Dictionary<string, string[]>();
        void Err(string field, string message) => errors[field] = [message];
        var name = dto.TrancheName?.Trim() ?? "";
        if (name.Length == 0) Err("trancheName", "Enter the tranche name.");
        if (dto.TrancheAmount <= 0) Err("trancheAmount", "Enter the tranche amount.");
        if (!TrancheStatuses.Contains(dto.StatusCode)) Err("statusCode", "Choose the status.");
        if (dto.StatusCode == "ACTIVE" && dto.ReleasedDate is null) Err("releasedDate", "Enter the date the bank released the tranche.");
        if (dto.MaturityDate is DateOnly m && dto.ReleasedDate is DateOnly r && m <= r) Err("maturityDate", "The maturity date must be after the release date.");
        if (name.Length > 0 && await db.Set<TrsFacilityTranche>().AnyAsync(x => x.FacilityId == facilityId && x.TrancheName == name && x.TrancheId != trancheId,
                cancellationToken))
            Err("trancheName", "This facility already has a tranche with this name.");

        var others = await db.Set<TrsFacilityTranche>().Where(x => x.FacilityId == facilityId && x.TrancheId != trancheId && x.StatusCode != "CLOSED")
            .SumAsync(x => (decimal?)x.TrancheAmount, cancellationToken) ?? 0;
        if (dto.StatusCode != "CLOSED" && others + dto.TrancheAmount > f.ApprovedLimit)
            Err("trancheAmount", $"The tranches would add up to {others + dto.TrancheAmount:N0}, above the facility limit of {f.ApprovedLimit:N0}.");

        TrsFacilityTranche? t = null;
        if (trancheId is long id)
        {
            t = await db.Set<TrsFacilityTranche>().SingleOrDefaultAsync(x => x.TrancheId == id && x.FacilityId == facilityId, cancellationToken)
                ?? throw new NotFoundException($"Tranche {id} was not found.");
            var subs = await db.Set<TrsFacilitySublimit>().Where(x => x.TrancheId == id && x.StatusCode != "CLOSED")
                .Select(x => new { x.SublimitId, x.LimitAmount, x.PoolTypeCode }).ToListAsync(cancellationToken);
            var dedicated = subs.Where(x => x.PoolTypeCode == "DEDICATED").Sum(x => x.LimitAmount);
            if (dto.TrancheAmount < dedicated) Err("trancheAmount", $"Its dedicated sublimits add up to {dedicated:N0}; the tranche cannot be smaller.");
            var used = await UsedAsync(subs.Select(x => x.SublimitId), cancellationToken);
            if (dto.TrancheAmount < used) Err("trancheAmount", $"{used:N0} is already used or reserved in this tranche.");
            if (dto.StatusCode != "ACTIVE" && used > 0) Err("statusCode", "The tranche has money drawn or reserved; it must stay Active.");
        }
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        if (t is null)
        {
            t = new TrsFacilityTranche { FacilityId = facilityId };
            db.Set<TrsFacilityTranche>().Add(t);
        }
        t.TrancheName = name;
        t.TrancheAmount = dto.TrancheAmount;
        t.StatusCode = dto.StatusCode;
        t.ReleasedDate = dto.ReleasedDate;
        t.MaturityDate = dto.MaturityDate;
        t.Notes = Blank(dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
        return t.TrancheId;
    }

    // ---------------------------------------------------------------- sublimits

    public async Task<long> SaveSublimitAsync(long trancheId, long? sublimitId, TrsSublimitFormDto dto, CancellationToken cancellationToken)
    {
        var t = await db.Set<TrsFacilityTranche>().AsNoTracking().SingleOrDefaultAsync(x => x.TrancheId == trancheId, cancellationToken)
            ?? throw new NotFoundException($"Tranche {trancheId} was not found.");
        var errors = new Dictionary<string, string[]>();
        void Err(string field, string message) => errors[field] = [message];
        var name = dto.SublimitName?.Trim() ?? "";
        var lines = (dto.BusinessLines ?? []).Select(x => x.Trim().ToUpperInvariant()).Where(x => x.Length > 0).Distinct().ToList();
        if (name.Length == 0) Err("sublimitName", "Enter the sublimit name.");
        if (dto.LimitAmount <= 0) Err("limitAmount", "Enter the sublimit amount.");
        if (!PoolTypes.Contains(dto.PoolTypeCode)) Err("poolTypeCode", "Choose dedicated or shared.");
        if (!SublimitStatuses.Contains(dto.StatusCode)) Err("statusCode", "Choose the status.");
        if (lines.Any(x => BusinessLines.All(o => o.Value != x))) Err("businessLines", "Choose business lines from the list.");
        if (name.Length > 0 && await db.Set<TrsFacilitySublimit>().AnyAsync(x => x.TrancheId == trancheId && x.SublimitName == name && x.SublimitId != sublimitId,
                cancellationToken))
            Err("sublimitName", "This tranche already has a sublimit with this name.");
        if (dto.LimitAmount > t.TrancheAmount) Err("limitAmount", $"A sublimit cannot be above its tranche ({t.TrancheAmount:N0}).");
        if (dto.PoolTypeCode == "DEDICATED" && dto.StatusCode != "CLOSED")
        {
            var others = await db.Set<TrsFacilitySublimit>()
                .Where(x => x.TrancheId == trancheId && x.SublimitId != sublimitId && x.PoolTypeCode == "DEDICATED" && x.StatusCode != "CLOSED")
                .SumAsync(x => (decimal?)x.LimitAmount, cancellationToken) ?? 0;
            if (others + dto.LimitAmount > t.TrancheAmount)
                Err("limitAmount", $"The dedicated sublimits would add up to {others + dto.LimitAmount:N0}, above the tranche ({t.TrancheAmount:N0}).");
        }

        TrsFacilitySublimit? s = null;
        if (sublimitId is long id)
        {
            s = await db.Set<TrsFacilitySublimit>().SingleOrDefaultAsync(x => x.SublimitId == id && x.TrancheId == trancheId, cancellationToken)
                ?? throw new NotFoundException($"Sublimit {id} was not found.");
            var used = await UsedAsync([id], cancellationToken);
            if (dto.LimitAmount < used) Err("limitAmount", $"{used:N0} is already used or reserved in this sublimit.");
            if (dto.StatusCode == "CLOSED" && used > 0) Err("statusCode", "The sublimit has money drawn or reserved; it cannot be closed.");
        }
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        if (s is null)
        {
            s = new TrsFacilitySublimit { FacilityId = t.FacilityId, TrancheId = trancheId };
            db.Set<TrsFacilitySublimit>().Add(s);
        }
        s.SublimitName = name;
        s.LimitAmount = dto.LimitAmount;
        s.PoolTypeCode = dto.PoolTypeCode;
        s.StatusCode = dto.StatusCode;
        s.Notes = Blank(dto.Notes);
        await db.SaveChangesAsync(cancellationToken);

        var current = await db.Set<TrsSublimitScope>().Where(x => x.SublimitId == s.SublimitId).ToListAsync(cancellationToken);
        db.Set<TrsSublimitScope>().RemoveRange(current.Where(x => !lines.Contains(x.ScopeValue)));
        foreach (var line in lines.Where(l => current.All(x => x.ScopeValue != l)))
            db.Set<TrsSublimitScope>().Add(new TrsSublimitScope { SublimitId = s.SublimitId, ScopeTypeCode = "BUSINESS_LINE", ScopeValue = line });
        await db.SaveChangesAsync(cancellationToken);
        return s.SublimitId;
    }

    private async Task<decimal> UsedAsync(IEnumerable<long> sublimitIds, CancellationToken cancellationToken)
    {
        var ids = sublimitIds.ToList();
        if (ids.Count == 0) return 0;
        return await db.Set<TrsSublimitMovement>().Where(x => ids.Contains(x.SublimitId))
            .SumAsync(x => (decimal?)(x.OutstandingChange + x.ReservedChange), cancellationToken) ?? 0;
    }

    // ---------------------------------------------------------------- bank rules (four eyes)

    public async Task<long> SaveRuleAsync(long? ruleId, TrsBankRuleFormDto dto, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        void Err(string field, string message) => errors[field] = [message];
        if (!await db.Set<TrsBank>().AnyAsync(x => x.BankId == dto.BankId, cancellationToken)) Err("bankId", "Choose the bank.");
        if (dto.FacilityId is long fid && !await db.Set<TrsCreditFacility>().AnyAsync(x => x.FacilityId == fid && x.BankId == dto.BankId, cancellationToken))
            Err("facilityId", "The facility is not one of this bank's.");
        if (RuleTypes.All(x => x.Value != dto.RuleTypeCode)) Err("ruleTypeCode", "Choose the rule type.");
        if (SubjectTypes.All(x => x.Value != dto.SubjectTypeCode)) Err("subjectTypeCode", "Choose what the rule applies to.");
        if (Bases.All(x => x.Value != dto.BasisCode)) Err("basisCode", "Choose the basis.");
        if (BreachActions.All(x => x.Value != dto.BreachActionCode)) Err("breachActionCode", "Choose what happens on a breach.");

        var subjectValue = Blank(dto.SubjectValue);
        long? subjectClient = dto.SubjectClientId;
        switch (dto.RuleTypeCode)
        {
            case "MAX_TENOR":
                if (dto.MaxTenorDays is not > 0) Err("maxTenorDays", "Enter the maximum tenor in days.");
                break;
            case "CONCENTRATION":
                if (dto.LimitPct is null && dto.LimitAmount is null) Err("limitPct", "Enter the limit as a percent or as an amount.");
                if (dto.LimitPct is decimal p && (p <= 0 || p > 100)) Err("limitPct", "The percent must be above 0 and at most 100.");
                if (dto.LimitAmount is decimal a && a <= 0) Err("limitAmount", "The amount must be above 0.");
                if (dto.WarningPct is decimal w && (w <= 0 || (dto.LimitPct is decimal lp && w >= lp)))
                    Err("warningPct", "The warning must be above 0 and below the limit.");
                if (dto.SubjectTypeCode == "INVOICE") Err("subjectTypeCode", "A concentration rule applies to a debtor, a seller or a debtor group.");
                break;
            case "EXCLUDE":
                if (dto.SubjectTypeCode == "INVOICE") Err("subjectTypeCode", "An exclusion applies to a debtor, a seller or a debtor group.");
                if (dto.SubjectTypeCode == "DEBTOR_GROUP" ? subjectValue is null : subjectClient is null)
                    Err(dto.SubjectTypeCode == "DEBTOR_GROUP" ? "subjectValue" : "subjectClientId", "Choose who is excluded.");
                break;
        }
        if (dto.RuleTypeCode == "MAX_TENOR") { subjectClient = null; subjectValue = null; }
        else if (dto.SubjectTypeCode == "DEBTOR_GROUP") subjectClient = null;
        else subjectValue = null;
        if (subjectClient is long cid && !await db.Set<CrmClient>().AnyAsync(x => x.ClientId == cid, cancellationToken))
            Err("subjectClientId", "Choose the customer.");
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        TrsBankRule r;
        if (ruleId is long id)
        {
            r = await db.Set<TrsBankRule>().SingleOrDefaultAsync(x => x.RuleId == id, cancellationToken) ?? throw new NotFoundException($"Rule {id} was not found.");
            if (r.StatusCode == "INACTIVE") throw new BusinessRuleException("The rule is inactive.");
        }
        else
        {
            r = new TrsBankRule();
            db.Set<TrsBankRule>().Add(r);
        }
        var concentration = dto.RuleTypeCode == "CONCENTRATION";
        r.BankId = dto.BankId;
        r.FacilityId = dto.FacilityId;
        r.RuleTypeCode = dto.RuleTypeCode;
        r.SubjectTypeCode = dto.RuleTypeCode == "MAX_TENOR" ? "INVOICE" : dto.SubjectTypeCode;
        r.SubjectClientId = subjectClient;
        r.SubjectValue = subjectValue;
        r.LimitPct = concentration ? dto.LimitPct : null;
        r.LimitAmount = concentration ? dto.LimitAmount : null;
        r.WarningPct = concentration ? dto.WarningPct : null;
        r.MaxTenorDays = dto.RuleTypeCode == "MAX_TENOR" ? dto.MaxTenorDays : null;
        r.BasisCode = dto.BasisCode;
        r.BreachActionCode = dto.BreachActionCode;
        r.Notes = Blank(dto.Notes);
        r.StatusCode = "PENDING";
        r.ChangedBy = User;
        r.ChangedAt = clock.UtcNow;
        r.VerifiedBy = null;
        r.VerifiedAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return r.RuleId;
    }

    public async Task VerifyRuleAsync(long ruleId, CancellationToken cancellationToken)
    {
        var r = await db.Set<TrsBankRule>().SingleOrDefaultAsync(x => x.RuleId == ruleId, cancellationToken)
            ?? throw new NotFoundException($"Rule {ruleId} was not found.");
        if (r.StatusCode != "PENDING") throw new BusinessRuleException("The rule is not waiting for verification.");
        if (SameUser(r.ChangedBy)) throw new BusinessRuleException("You made the last change to this rule. Another user must verify it.");
        r.StatusCode = "ACTIVE";
        r.VerifiedBy = User;
        r.VerifiedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateRuleAsync(long ruleId, TrsNotesDto dto, CancellationToken cancellationToken)
    {
        var r = await db.Set<TrsBankRule>().SingleOrDefaultAsync(x => x.RuleId == ruleId, cancellationToken)
            ?? throw new NotFoundException($"Rule {ruleId} was not found.");
        var notes = Blank(dto.Notes) ?? throw BusinessValidationException.For("notes", "Enter why the rule no longer applies.");
        r.StatusCode = "INACTIVE";
        r.Notes = notes;
        r.ChangedBy = User;
        r.ChangedAt = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
