using Capitra.Domain.Modules.Scr;

namespace Capitra.Application.Modules.Scr;

/// <summary>One entry of the Federation Final List: a party of a published listing, as its later decisions left it.</summary>
public sealed record ScrRegistryEntry(
    long PartyId, long DecisionId, string DecisionRef, string OfficialNo, string PartyTypeCode, string NameAr, string? NameEn,
    IReadOnlyList<string> Aliases, string IdentityTypeCode, string IdentityNo, string NationalityCode, DateOnly? BirthDate,
    string ScopeText, DateOnly ListedFrom, DateOnly? LiftedOn, string StatusCode, long? LetterId, string? LastDecisionRef,
    string? EnteredBy = null);

/// <summary>
/// The Federation Final List, worked out from published decisions: every party of every published LISTING is an entry; its latest
/// published LIFTING (for that party) makes it LIFTED, and its latest AMENDMENT gives it the scope. A party is on the list or not: the
/// list holds only parties under a formal legal or court decision, so there is no suspension (Ahmed, 22 Nov 2026); SUSPENSION decisions
/// published before that are kept as history and change nothing. Lifted entries stay on the list as history.
/// </summary>
public static class ScrRegistry
{
    public const string Active = "ACTIVE";
    /// <summary>No longer given (no suspension): kept for inquiry hits recorded before.</summary>
    public const string Suspended = "SUSPENDED";
    public const string Lifted = "LIFTED";

    public static List<ScrRegistryEntry> Derive(IReadOnlyList<ScrDecision> decisions, IReadOnlyDictionary<long, ScrParty> parties)
    {
        var published = decisions.Where(d => d.StatusCode == ScrDecisionStatus.Published).ToList();
        var entries = new List<ScrRegistryEntry>();
        foreach (var listing in published.Where(d => d.DecisionTypeCode == ScrDecisionType.Listing).OrderBy(d => d.RegistryVersionNo))
        {
            foreach (var partyId in listing.Parties.Select(p => p.PartyId).Distinct())
            {
                if (!parties.TryGetValue(partyId, out var p))
                    continue;
                ScrDecision? Latest(string type) => published
                    .Where(c => c.ParentDecisionId == listing.DecisionId && c.DecisionTypeCode == type && c.Parties.Any(x => x.PartyId == partyId))
                    .OrderByDescending(c => c.RegistryVersionNo ?? 0).FirstOrDefault();
                var lift = Latest(ScrDecisionType.Lifting);
                var amendment = Latest(ScrDecisionType.Amendment);
                var status = lift is not null ? Lifted : Active;
                var last = new[] { lift, amendment }.Where(x => x is not null).OrderByDescending(x => x!.RegistryVersionNo ?? 0).FirstOrDefault();
                entries.Add(new ScrRegistryEntry(
                    p.PartyId, listing.DecisionId, listing.DecisionRef, listing.OfficialNo, p.PartyTypeCode, p.NameAr, p.NameEn,
                    [.. p.Aliases.Select(a => a.AliasName)], p.IdentityTypeCode, p.IdentityNo, p.NationalityCode, p.BirthDate,
                    amendment?.ScopeText ?? listing.ScopeText, listing.EffectiveDate, lift?.EffectiveDate, status, listing.LetterId,
                    last?.DecisionRef, listing.EnteredBy));
            }
        }

        return entries;
    }
}

public static class ScrDecisionType
{
    public const string Listing = "LISTING";
    public const string Amendment = "AMENDMENT";
    /// <summary>No longer entered (Ahmed, 22 Nov 2026): kept so decisions entered before can be read.</summary>
    public const string Suspension = "SUSPENSION";
    public const string Lifting = "LIFTING";

    /// <summary>The types a new decision may have.</summary>
    public static readonly string[] All = [Listing, Amendment, Lifting];
}

public static class ScrDecisionStatus
{
    public const string Draft = "DRAFT";
    public const string InReview = "IN_REVIEW";
    public const string AwaitingApproval = "AWAITING_APPROVAL";
    public const string Returned = "RETURNED";
    public const string Published = "PUBLISHED";
    public const string Cancelled = "CANCELLED";

    /// <summary>The list "awaiting approval": entered but not yet published, cancelled or published.</summary>
    public const string Pending = "PENDING";

    public static readonly string[] Open = [Draft, Returned, InReview, AwaitingApproval];
}
