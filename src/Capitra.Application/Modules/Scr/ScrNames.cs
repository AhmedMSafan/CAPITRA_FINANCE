using System.Text;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// Name matching for screening, Arabic and English: names are normalised (diacritics, alef/yeh/teh marbuta forms, "عبد ال"
/// joined), split into words, and compared word by word with an edit-distance similarity, so spelling variants, word order and
/// a missing middle name still match. Pure; nothing here reads the database.
/// </summary>
public static class ScrNames
{
    /// <summary>A name scoring at least this (0-100) is a name match.</summary>
    public const decimal MatchThreshold = 85m;

    /// <summary>Two words are the same word from this similarity (0-1) on.</summary>
    private const double WordThreshold = 0.82;

    public static string Normalize(string? name)
    {
        var sb = new StringBuilder();
        foreach (var raw in (name ?? string.Empty).Trim().ToLowerInvariant())
        {
            var c = raw switch
            {
                'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',
                'ى' => 'ي',
                'ئ' => 'ي',
                'ؤ' => 'و',
                'ة' => 'ه',
                _ => raw,
            };
            // tashkeel, superscript alef and tatweel carry no letter
            if (c is >= 'ً' and <= 'ٟ' or 'ٰ' or 'ـ')
                continue;
            sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        var words = sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        // "عبد العزيز" and "عبدالعزيز", "abdel aziz" and "abdelaziz", are one name
        for (var i = 0; i < words.Count - 1; i++)
        {
            if (words[i] is "عبد" or "abd" or "abdel" or "abdul" or "abdal" || (words[i] == "ابو" && words[i + 1].StartsWith("ال", StringComparison.Ordinal)))
            {
                words[i] += words[i + 1];
                words.RemoveAt(i + 1);
            }
        }

        return string.Join(' ', words);
    }

    public static IReadOnlyList<string> Words(string? name) =>
        Normalize(name).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// How alike two names are, 0-100: the share of the shorter name's words found (alike) in the longer one, weighted by how
    /// alike they are, times a small penalty when the longer name has many more words. Word order does not matter.
    /// </summary>
    public static decimal Score(string? a, string? b)
    {
        var x = Words(a);
        var y = Words(b);
        if (x.Count == 0 || y.Count == 0)
            return 0;
        var (shorter, longer) = x.Count <= y.Count ? (x, y) : (y, x);
        var used = new bool[longer.Count];
        double total = 0;
        foreach (var w in shorter)
        {
            var best = -1;
            double bestScore = 0;
            for (var j = 0; j < longer.Count; j++)
            {
                if (used[j])
                    continue;
                var s = Similarity(w, longer[j]);
                if (s > bestScore)
                {
                    bestScore = s;
                    best = j;
                }
            }

            if (best >= 0 && bestScore >= WordThreshold)
            {
                used[best] = true;
                total += bestScore;
            }
        }

        var coverage = total / shorter.Count;
        // a one-word query matching one word of a four-word name is weak evidence
        var lengthFactor = shorter.Count == 1 && longer.Count > 1 ? 0.7 : 1 - 0.05 * Math.Max(0, longer.Count - shorter.Count - 1);
        return Math.Round((decimal)(coverage * lengthFactor * 100), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>1 - Levenshtein distance / longer length.</summary>
    public static double Similarity(string a, string b)
    {
        if (a == b)
            return 1;
        if (a.Length == 0 || b.Length == 0)
            return 0;
        var prev = new int[b.Length + 1];
        var cur = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
            prev[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            cur[0] = i;
            for (var j = 1; j <= b.Length; j++)
                cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (prev, cur) = (cur, prev);
        }

        return 1 - (double)prev[b.Length] / Math.Max(a.Length, b.Length);
    }

    /// <summary>An identity number as compared: letters and digits only, upper case, without leading zeros.</summary>
    public static string Identity(string? value)
    {
        var s = new string((value ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        var trimmed = s.TrimStart('0');
        return trimmed.Length == 0 ? (s.Length == 0 ? s : "0") : trimmed;
    }
}
