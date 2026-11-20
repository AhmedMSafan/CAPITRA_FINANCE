namespace Capitra.Application.Modules.Scr;

/// <summary>What is screened: a person or company, its name, identity number and, if known, birth date.</summary>
public sealed record ScrSubject(string PartyTypeCode, string Name, string IdentityNo, DateOnly? BirthDate = null);

/// <summary>A Final List entry the subject matched, how, and how alike the names are.</summary>
public sealed record ScrHit(ScrRegistryEntry Entry, string MatchBasisCode, decimal NameScorePct, string MatchedName);

/// <summary>
/// Screens one subject against the Final List. Fixes the FINOVA prototype, which reported only exact identity matches:
/// <list type="bullet">
/// <item>The identity number matches when equal once spaces, dashes and leading zeros are left out (any identity type: a passport
/// number or a national ID).</item>
/// <item>The name matches when it scores at least <see cref="ScrNames.MatchThreshold"/> against the entry's Arabic or English
/// name or an alias. A name-only match whose birth date differs from the entry's is dropped.</item>
/// <item>Result: MATCH when a listed (active or suspended) entry matches on identity; POSSIBLE_MATCH when it matches on the name
/// only, to be reviewed; PREVIOUSLY_LISTED when only lifted entries match; else NO_MATCH.</item>
/// </list>
/// </summary>
public static class ScrMatcher
{
    public const string NoMatch = "NO_MATCH";
    public const string PossibleMatch = "POSSIBLE_MATCH";
    public const string Match = "MATCH";
    public const string PreviouslyListed = "PREVIOUSLY_LISTED";

    public static List<ScrHit> Screen(ScrSubject subject, IEnumerable<ScrRegistryEntry> list)
    {
        var id = ScrNames.Identity(subject.IdentityNo);
        var hits = new List<ScrHit>();
        foreach (var e in list.Where(e => e.PartyTypeCode == subject.PartyTypeCode))
        {
            var byId = id.Length > 0 && ScrNames.Identity(e.IdentityNo) == id;
            var (score, name) = new[] { e.NameAr, e.NameEn }.Concat(e.Aliases).Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => (Score: ScrNames.Score(subject.Name, n), Name: n!))
                .OrderByDescending(x => x.Score).FirstOrDefault();
            var byName = score >= ScrNames.MatchThreshold;
            if (byName && !byId && subject.BirthDate is { } b && e.BirthDate is { } eb && b != eb)
                byName = false;
            if (!byId && !byName)
                continue;
            var basis = byId && byName ? "ID_AND_NAME" : byId ? "ID_ONLY" : "NAME_ONLY";
            hits.Add(new ScrHit(e, basis, score, name ?? e.NameAr));
        }

        return [.. hits.OrderBy(h => h.Entry.StatusCode == ScrRegistry.Lifted).ThenBy(h => h.MatchBasisCode == "NAME_ONLY").ThenByDescending(h => h.NameScorePct)];
    }

    public static string Result(IReadOnlyCollection<ScrHit> hits)
    {
        var listed = hits.Where(h => h.Entry.StatusCode != ScrRegistry.Lifted).ToList();
        if (listed.Any(h => h.MatchBasisCode != "NAME_ONLY"))
            return Match;
        if (listed.Count > 0)
            return PossibleMatch;
        return hits.Count > 0 ? PreviouslyListed : NoMatch;
    }
}
