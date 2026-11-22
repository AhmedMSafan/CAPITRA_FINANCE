namespace Capitra.Application.Modules.Scr;

/// <summary>What is screened: a person or company, its name, identity number and, if known, birth date and nationality.</summary>
public sealed record ScrSubject(string PartyTypeCode, string Name, string IdentityNo, DateOnly? BirthDate = null, string? NationalityCode = null);

/// <summary>
/// A Final List entry shown for the subject, with how far each point agrees: the identity number (EXACT, or PARTIAL with the characters
/// different and its similarity), the name (best similarity against the Arabic, English and other names), the birth
/// date and the nationality (SAME, DIFFERENT or NOT_GIVEN).
/// </summary>
public sealed record ScrHit(
    ScrRegistryEntry Entry, string MatchBasisCode, decimal NameScorePct, string MatchedName,
    string IdMatchCode, decimal IdScorePct, int IdDigitsDifferent, string BirthMatchCode, string NationalityMatchCode);

/// <summary>
/// Screens one subject against the Final List. No verdict: the Federation shows the facts and the member company decides.
/// <list type="bullet">
/// <item>An entry whose identity number is exactly the same (once spaces, dashes and leading zeros are left out) is always shown,
/// whatever the name: the name's level is shown next to it.</item>
/// <item>When the user lets fewer identity characters agree (1-13), an entry whose number is only partly the same is shown when
/// at least that many characters agree AND the name scores at least the user's minimum (30-100, default
/// <see cref="DefaultNameMin"/>) against the entry's Arabic or English name or another name.</item>
/// <item>Each point gets its level: identity number and name as a percentage, birth date and nationality same or different.</item>
/// <item>Result: FOUND when at least one entry is shown, else NOT_FOUND. Inquiries made before 23 Nov 2026 keep the results of
/// the time (MATCH, POSSIBLE_MATCH, PREVIOUSLY_LISTED, NO_MATCH).</item>
/// </list>
/// </summary>
public static class ScrMatcher
{
    public const string Found = "FOUND";
    public const string NotFound = "NOT_FOUND";

    /// <summary>The least name similarity when the user sets none, and the range the user may choose from.</summary>
    public const int DefaultNameMin = 80;
    public const int NameMinFloor = 30;

    /// <summary>The identity characters that must agree when the user sets none: all of them.</summary>
    public const int DefaultIdDigits = 14;


    public static List<ScrHit> Screen(ScrSubject subject, IEnumerable<ScrRegistryEntry> list, int nameMinPct = DefaultNameMin, int idMinDigits = DefaultIdDigits)
    {
        var id = ScrNames.Identity(subject.IdentityNo);
        var hits = new List<ScrHit>();
        foreach (var e in list.Where(e => e.PartyTypeCode == subject.PartyTypeCode))
        {
            var (idCode, idPct, idDiff) = CompareIdentity(id, ScrNames.Identity(e.IdentityNo));
            var (score, name) = new[] { e.NameAr, e.NameEn }.Concat(e.Aliases).Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => (Score: ScrNames.Score(subject.Name, n), Name: n!))
                .OrderByDescending(x => x.Score).FirstOrDefault();
            // the same identity number is always shown; a partly same one needs enough characters and a name alike enough
            var other = ScrNames.Identity(e.IdentityNo);
            var agree = Math.Max(id.Length, other.Length) - idDiff;
            if (id.Length == 0)
                continue;
            if (idCode != "EXACT" && (agree < Math.Min(idMinDigits, Math.Max(id.Length, other.Length)) || score < nameMinPct))
                continue;
            const string basis = "ID_AND_NAME";
            var birth = subject.BirthDate is not { } b ? "NOT_GIVEN" : e.BirthDate is not { } eb ? "NOT_ON_LIST" : b == eb ? "SAME" : "DIFFERENT";
            var nat = string.IsNullOrWhiteSpace(subject.NationalityCode) ? "NOT_GIVEN"
                : string.IsNullOrWhiteSpace(e.NationalityCode) ? "NOT_ON_LIST"
                : string.Equals(subject.NationalityCode.Trim(), e.NationalityCode.Trim(), StringComparison.OrdinalIgnoreCase) ? "SAME" : "DIFFERENT";
            hits.Add(new ScrHit(e, basis, score, name ?? e.NameAr, idCode, idPct, idDiff, birth, nat));
        }

        return [.. hits.OrderBy(h => h.IdMatchCode == "EXACT" ? 0 : h.IdMatchCode == "PARTIAL" ? 1 : 2)
            .ThenByDescending(h => h.NameScorePct).ThenBy(h => h.Entry.StatusCode == ScrRegistry.Lifted)];
    }

    /// <summary>EXACT, PARTIAL (some characters different) or DIFFERENT (nothing to compare), the similarity (0-100) and the characters different.</summary>
    public static (string Code, decimal Pct, int Diff) CompareIdentity(string a, string b)
    {
        if (a.Length == 0 || b.Length == 0)
            return ("DIFFERENT", 0m, Math.Max(a.Length, b.Length));
        if (a == b)
            return ("EXACT", 100m, 0);
        var sim = ScrNames.Similarity(a, b);
        var diff = (int)Math.Round((1 - sim) * Math.Max(a.Length, b.Length));
        var pct = Math.Round((decimal)(sim * 100), 2, MidpointRounding.AwayFromZero);
        return ("PARTIAL", pct, diff);
    }

    public static string Result(IReadOnlyCollection<ScrHit> hits) => hits.Count > 0 ? Found : NotFound;
}
