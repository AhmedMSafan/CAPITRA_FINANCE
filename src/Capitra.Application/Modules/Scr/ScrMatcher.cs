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
/// <item>An entry is shown when at least the user's number of identity characters agree (1-14, default all of them: exactly the
/// same number, once spaces, dashes and leading zeros are left out) OR the name scores at least the user's minimum (30-100,
/// default <see cref="DefaultNameMin"/>) against the entry's Arabic or English name or another name. Both levels are shown.</item>
/// <item>The basis says what reached the user's level: ID_AND_NAME, ID or NAME.</item>
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
            // enough identity characters agree OR the name is at least as alike as chosen
            var other = ScrNames.Identity(e.IdentityNo);
            var agree = Math.Max(id.Length, other.Length) - idDiff;
            var idOk = id.Length > 0 && other.Length > 0 && agree >= Math.Min(idMinDigits, Math.Max(id.Length, other.Length));
            var nameOk = score >= nameMinPct;
            if (!idOk && !nameOk)
                continue;
            var basis = idOk && nameOk ? "ID_AND_NAME" : idOk ? "ID" : "NAME";
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
