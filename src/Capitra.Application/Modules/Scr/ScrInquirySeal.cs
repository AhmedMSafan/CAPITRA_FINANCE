using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Capitra.Domain.Modules.Scr;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// The seal of an inquiry: a SHA-256 of what was asked, answered and when, chained to the inquiry before it (PREV_HASH). The database
/// refuses any change to a sealed inquiry or its hits and any deletion; the chain shows any change made around that (a row edited,
/// removed or inserted breaks it from there on). Inquiries made before sealing began (22 Nov 2026) have no seal.
/// </summary>
public static class ScrInquirySeal
{
    public const string Version = "v1";

    /// <summary>From 23 Nov 2026 (results FOUND / NOT_FOUND) the hits' levels are sealed too.</summary>
    public const string Version2 = "v2";

    private static bool IsV2(ScrInquiry i) => i.ResultCode is ScrMatcher.Found or ScrMatcher.NotFound;

    /// <summary>The text that is hashed: every field that is evidence, in a fixed order and format.</summary>
    public static string Canonical(ScrInquiry i, string? prevHash)
    {
        var c = CultureInfo.InvariantCulture;
        var b = new StringBuilder();
        void Add(string? v) => b.Append(v?.Replace("|", "\\|", StringComparison.Ordinal) ?? string.Empty).Append('|');
        Add(IsV2(i) ? Version2 : Version);
        Add(prevHash);
        Add(i.InquiryRef);
        Add(i.MemberCompanyId.ToString(c));
        Add(i.RequestedBy);
        Add(i.ChannelCode);
        Add(i.ApiClientId?.ToString(c));
        Add(i.SubjectTypeCode);
        Add(i.SubjectName);
        Add(i.IdentityTypeCode);
        Add(i.IdentityNo);
        Add(i.BirthDate?.ToString("yyyy-MM-dd", c));
        Add(i.NationalityCode);
        Add(i.PurposeCode);
        Add(i.PurposeText);
        Add(i.CaseReference);
        Add(i.ResultCode);
        Add(i.RegistryVersionNo.ToString(c));
        if (IsV2(i))
        {
            Add(i.NameMatchMinPct?.ToString(c));
            Add(i.IdMatchMinDigits?.ToString(c));
        }

        Add(Utc(i.ScreenedAt));
        Add(i.ReceivedAt is { } r ? Utc(r) : null);
        Add(i.RespondedAt is { } s ? Utc(s) : null);
        foreach (var h in i.Hits.OrderBy(h => h.PartyId).ThenBy(h => h.DecisionId))
            Add(IsV2(i)
                ? string.Create(c, $"{h.PartyId}:{h.DecisionId}:{h.MatchBasisCode}:{h.NameScorePct:0.000000}:{h.EntryStatusCode}:{h.MatchedName}:{h.IdMatchCode}:{h.IdScorePct:0.000000}:{h.IdDigitsDifferent}:{h.BirthMatchCode}:{h.NationalityMatchCode}")
                : string.Create(c, $"{h.PartyId}:{h.DecisionId}:{h.MatchBasisCode}:{h.NameScorePct:0.000000}:{h.EntryStatusCode}:{h.MatchedName}"));
        return b.ToString();
    }

    public static string Hash(ScrInquiry i, string? prevHash) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(i, prevHash))));

    /// <summary>A time as PostgreSQL keeps it (microseconds), so the seal made before saving matches the row read back.</summary>
    public static DateTime Micro(DateTime t) => new(t.Ticks - (t.Ticks % 10), t.Kind);

    /// <summary>The instant to the microsecond (what PostgreSQL keeps), in UTC.</summary>
    private static string Utc(DateTime t)
    {
        var u = DateTime.SpecifyKind(t.Kind == DateTimeKind.Local ? t.ToUniversalTime() : t, DateTimeKind.Utc);
        u = new DateTime(u.Ticks - (u.Ticks % 10), DateTimeKind.Utc);
        return u.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ", CultureInfo.InvariantCulture);
    }
}
