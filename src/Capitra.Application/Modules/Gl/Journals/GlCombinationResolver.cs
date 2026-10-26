using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>
/// Turns the segment values of a journal line into an account combination, creating it the first time it is used
/// (APEX GL_POSTING_API_PKG.resolve_line and GL_COA_COMBINATION_PKG). Works for any number of segments.
/// Call it inside a transaction: a new combination is saved straight away.
/// </summary>
internal sealed class GlCombinationResolver
{
    private readonly ICapitraDbContext _db;
    private readonly GlCoaStructure _structure;
    private readonly List<GlCoaSegment> _segments;
    private readonly Dictionary<long, Dictionary<string, GlCoaSegmentValue>> _values;
    private readonly Dictionary<string, GlAccount> _accounts;
    private readonly Dictionary<string, GlAccountCombination> _created = [];

    private GlCombinationResolver(
        ICapitraDbContext db,
        GlCoaStructure structure,
        List<GlCoaSegment> segments,
        Dictionary<long, Dictionary<string, GlCoaSegmentValue>> values,
        Dictionary<string, GlAccount> accounts)
    {
        _db = db;
        _structure = structure;
        _segments = segments;
        _values = values;
        _accounts = accounts;
    }

    public GlCoaStructure Structure => _structure;

    public static async Task<GlCombinationResolver> CreateAsync(ICapitraDbContext db, long coaId, CancellationToken cancellationToken)
    {
        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstAsync(x => x.CoaId == coaId, cancellationToken);
        var segments = await db.Set<GlCoaSegment>().AsNoTracking()
            .Where(x => x.CoaId == coaId && x.IsEnabled).OrderBy(x => x.SegmentNum).ToListAsync(cancellationToken);
        var segmentIds = segments.Select(x => x.CoaSegmentId).ToList();
        var values = (await db.Set<GlCoaSegmentValue>().AsNoTracking()
                .Where(x => segmentIds.Contains(x.CoaSegmentId)).ToListAsync(cancellationToken))
            .GroupBy(x => x.CoaSegmentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.ValueCode, StringComparer.Ordinal));
        var accounts = await db.Set<GlAccount>().AsNoTracking()
            .Where(x => x.CoaId == coaId).ToDictionaryAsync(x => x.AccountCode, StringComparer.Ordinal, cancellationToken);

        return new GlCombinationResolver(db, structure, segments, values, accounts);
    }

    /// <summary>
    /// Fills the defaults, checks every value and returns the combination, or the reason it cannot be used.
    /// A FIXED default always wins; a SUGGEST default fills a blank value only.
    /// </summary>
    public async Task<(GlAccountCombination? Combination, string? Error)> ResolveAsync(
        IReadOnlyDictionary<int, string?>? input, DateOnly onDate, bool manual, CancellationToken cancellationToken)
    {
        if (!_structure.IsActive)
            return (null, $"Account structure {_structure.CoaCode} is inactive.");

        var chosen = new List<(GlCoaSegment Segment, string? Code)>();
        var missing = new List<string>();

        foreach (var segment in _segments)
        {
            var code = GlRules.Code(input is not null && input.TryGetValue(segment.SegmentNum, out var v) ? v : null);
            var fallback = GlRules.Code(segment.DefaultValueCode);
            if (fallback is not null && (segment.DefaultMode == GlCodes.DefaultModeFixed || code is null))
                code = fallback;

            if (code is null && (segment.IsRequired || segment.SegmentNum == _structure.AccountSegmentNum))
                missing.Add(segment.SegmentNameEn.ToLowerInvariant());
            chosen.Add((segment, code));
        }

        if (missing.Count > 0)
            return (null, $"Missing {string.Join(", ", missing)}.");

        GlAccount? account = null;
        foreach (var (segment, code) in chosen)
        {
            if (code is null)
                continue;

            if (segment.SegmentNum == _structure.AccountSegmentNum)
            {
                if (!_accounts.TryGetValue(code, out account))
                    return (null, $"Account {code} does not exist.");
                if (!account.IsActive || !Effective(account.EffectiveStartDate, account.EffectiveEndDate, onDate))
                    return (null, $"Account {code} is inactive on {onDate:dd MMM yyyy}.");
                if (account.IsSummary || !account.IsPostingAllowed)
                    return (null, $"Account {code} is a summary account and does not allow posting.");
                if (manual && !account.IsManualJournalAllowed)
                    return (null, $"Account {code} does not allow manual journals.");
                if (!manual && !account.IsSubledgerPostingAllowed)
                    return (null, $"Account {code} does not allow sub-ledger posting.");
                if (account.AccountSegmentValueId is null)
                    return (null, $"Account {code} has no value in the account segment.");
                continue;
            }

            if (!_values.TryGetValue(segment.CoaSegmentId, out var list) || !list.TryGetValue(code, out var value))
                return (null, $"{segment.SegmentNameEn} {code} does not exist.");
            if (!value.IsEnabled || !Effective(value.EffectiveStartDate, value.EffectiveEndDate, onDate))
                return (null, $"{segment.SegmentNameEn} {code} is disabled on {onDate:dd MMM yyyy}.");
            if (value.IsSummary || !value.IsPostingAllowed)
                return (null, $"{segment.SegmentNameEn} {code} is a summary value and does not allow posting.");
        }

        var needs = new List<string>();
        if (account!.IsBranchRequired && Blank(_structure.BranchSegmentNum)) needs.Add("a branch");
        if (account.IsCostCenterRequired && Blank(_structure.CostCenterSegmentNum)) needs.Add("a cost centre");
        if (account.IsBusinessLineRequired && Blank(_structure.BusinessLineSegmentNum)) needs.Add("a business line");
        if (needs.Count > 0)
            return (null, $"Account {account.AccountCode} needs {string.Join(" and ", needs)}.");

        var concatenated = string.Join(_structure.DelimiterChar, chosen.Select(x => x.Code ?? string.Empty));

        if (_created.TryGetValue(concatenated, out var pending))
            return (pending, null);

        var existing = await _db.Set<GlAccountCombination>()
            .FirstOrDefaultAsync(x => x.CoaId == _structure.CoaId && x.ConcatenatedAccount == concatenated, cancellationToken);
        if (existing is not null)
            return existing.IsEnabled ? (existing, null) : (null, $"{concatenated} is disabled.");

        if (!_structure.IsDynamicCombinationAllowed)
            return (null, $"{concatenated} does not exist, and structure {_structure.CoaCode} does not allow new combinations.");

        var combination = new GlAccountCombination
        {
            CoaId = _structure.CoaId,
            ConcatenatedAccount = concatenated,
            AccountId = account.AccountId,
        };
        // Saved at once for its key; the caller's transaction undoes it if the journal is not saved.
        _db.Add(combination);
        await _db.SaveChangesAsync(cancellationToken);
        foreach (var (segment, code) in chosen.Where(x => x.Code is not null))
        {
            var valueId = segment.SegmentNum == _structure.AccountSegmentNum
                ? account.AccountSegmentValueId!.Value
                : _values[segment.CoaSegmentId][code!].SegmentValueId;
            _db.Add(new GlAccountCombinationSegment
            {
                AccountCombinationId = combination.AccountCombinationId,
                CoaSegmentId = segment.CoaSegmentId,
                SegmentNum = segment.SegmentNum,
                SegmentValueId = valueId,
                SegmentValueCode = code!,
            });
        }
        await _db.SaveChangesAsync(cancellationToken);

        _created[concatenated] = combination;
        return (combination, null);

        bool Blank(int? segmentNum) => segmentNum is { } n && chosen.FirstOrDefault(x => x.Segment.SegmentNum == n).Code is null;
    }

    private static bool Effective(DateOnly start, DateOnly? end, DateOnly on) => start <= on && (end is null || end >= on);
}
