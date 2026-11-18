using Capitra.Domain.Modules.Scr;

namespace Capitra.Application.Modules.Scr;

/// <summary>One entry of the Federation Final List: a party of a published listing, as its later decisions left it.</summary>
public sealed record ScrRegistryEntry(
    long PartyId, long DecisionId, string DecisionRef, string OfficialNo, string PartyTypeCode, string NameAr, string? NameEn,
    IReadOnlyList<string> Aliases, string IdentityTypeCode, string IdentityNo, string NationalityCode, DateOnly? BirthDate,
    string ScopeText, DateOnly ListedFrom, DateOnly? LiftedOn, string StatusCode, long? LetterId, string? LastDecisionRef);

/// <summary>
/// The Federation Final List, worked out from published decisions (as FINOVA's deriveRegistry did): every party of every published
/// LISTING is an entry; its latest published LIFTING (for that party) makes it LIFTED, else its latest SUSPENSION makes it
/// SUSPENDED, and its latest AMENDMENT gives it the scope. Lifted entries stay on the list as history.
/// </summary>
public static class ScrRegistry
{
    public const string Active = "ACTIVE";
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
                var suspension = Latest(ScrDecisionType.Suspension);
                var amendment = Latest(ScrDecisionType.Amendment);
                var status = lift is not null ? Lifted : suspension is not null ? Suspended : Active;
                var last = new[] { lift, suspension, amendment }.Where(x => x is not null).OrderByDescending(x => x!.RegistryVersionNo ?? 0).FirstOrDefault();
                entries.Add(new ScrRegistryEntry(
                    p.PartyId, listing.DecisionId, listing.DecisionRef, listing.OfficialNo, p.PartyTypeCode, p.NameAr, p.NameEn,
                    [.. p.Aliases.Select(a => a.AliasName)], p.IdentityTypeCode, p.IdentityNo, p.NationalityCode, p.BirthDate,
                    amendment?.ScopeText ?? listing.ScopeText, listing.EffectiveDate, lift?.EffectiveDate, status, listing.LetterId,
                    last?.DecisionRef));
            }
        }

        return entries;
    }
}

public static class ScrDecisionType
{
    public const string Listing = "LISTING";
    public const string Amendment = "AMENDMENT";
    public const string Suspension = "SUSPENSION";
    public const string Lifting = "LIFTING";

    public static readonly string[] All = [Listing, Amendment, Suspension, Lifting];
}

public static class ScrDecisionStatus
{
    public const string Draft = "DRAFT";
    public const string InReview = "IN_REVIEW";
    public const string AwaitingApproval = "AWAITING_APPROVAL";
    public const string Returned = "RETURNED";
    public const string Published = "PUBLISHED";
    public const string Cancelled = "CANCELLED";
}
