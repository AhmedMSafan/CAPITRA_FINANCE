using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Domain.Modules.Scr;
using Xunit;

namespace Capitra.Tests.Scr;

/// <summary>Name matching, the Final List, screening results and segregation of duties (the FINOVA review findings).</summary>
public sealed class ScrScreeningTests
{
    [Fact]
    public void Arabic_spelling_variants_normalise_alike()
    {
        Assert.Equal(ScrNames.Normalize("أحمد محمد علي"), ScrNames.Normalize("احمد محمد على"));
        Assert.Equal(ScrNames.Normalize("حسن محمود عبد العزيز"), ScrNames.Normalize("حسن محمود عبدالعزيز"));
        Assert.Equal(ScrNames.Normalize("فاطمة"), ScrNames.Normalize("فاطمه"));
        Assert.Equal(ScrNames.Normalize("مُحَمَّد"), ScrNames.Normalize("محمد"));
        Assert.Equal("abdelaziz hassan", ScrNames.Normalize("Abdel-Aziz  Hassan"));
    }

    [Theory]
    [InlineData("أحمد محمد علي", "احمد محمد على", 100)]
    [InlineData("علي محمد أحمد", "أحمد محمد علي", 100)]        // word order
    [InlineData("أحمد علي", "أحمد محمد علي", 100)]             // middle name left out
    [InlineData("Ahmed Mohamed Ali", "Ahmed Mohammed Ali", 95.83)] // one letter
    public void Alike_names_score_high(string a, string b, double expected) =>
        Assert.Equal((decimal)expected, ScrNames.Score(a, b));

    [Theory]
    [InlineData("Hassan Ali", "Ahmed Mohamed Ali")]
    [InlineData("محمد", "أحمد محمد علي")]  // one word of a three-word name
    [InlineData("", "أحمد")]
    public void Different_names_do_not_match(string a, string b) =>
        Assert.True(ScrNames.Score(a, b) < ScrNames.MatchThreshold);

    [Fact]
    public void Identity_numbers_compare_without_spaces_dashes_and_leading_zeros()
    {
        Assert.Equal("29801011234567", ScrNames.Identity("0029801011234567"));
        Assert.Equal("A1230455", ScrNames.Identity("a-123 0455"));
        Assert.Equal("0", ScrNames.Identity("000"));
    }

    private static ScrRegistryEntry Entry(string name, string id, string status = "ACTIVE", string type = "PERSON", string? en = null,
        DateOnly? birth = null, params string[] aliases) =>
        new(1, 10, "DEC-000010", "FRZ-1/2026", type, name, en, aliases, type == "PERSON" ? "NATIONAL_ID" : "COMMERCIAL_REGISTER", id, "EG", birth,
            "Freeze", new(2026, 1, 1), status == "LIFTED" ? new DateOnly(2026, 6, 1) : null, status, null, null);

    private static (List<ScrHit> Hits, string Result) Screen(string name, string id, params ScrRegistryEntry[] list) =>
        Screen(new ScrSubject("PERSON", name, id), list);

    private static (List<ScrHit> Hits, string Result) Screen(ScrSubject s, params ScrRegistryEntry[] list)
    {
        var hits = ScrMatcher.Screen(s, list);
        return (hits, ScrMatcher.Result(hits));
    }

    [Fact]
    public void A_listed_name_with_a_mistyped_ID_is_a_possible_match_not_a_clear()
    {
        // the FINOVA prototype answered "No Match" here
        var (hits, result) = Screen("أحمد محمد علي", "29801011234568", Entry("أحمد محمد علي", "29801011234567"));
        Assert.Equal(ScrMatcher.PossibleMatch, result);
        Assert.Equal("NAME_ONLY", Assert.Single(hits).MatchBasisCode);
    }

    [Fact]
    public void The_listed_ID_matches_whatever_the_name()
    {
        var (hits, result) = Screen("Someone Else", "29801011234567", Entry("أحمد محمد علي", "29801011234567"));
        Assert.Equal(ScrMatcher.Match, result);
        Assert.Equal("ID_ONLY", Assert.Single(hits).MatchBasisCode);
        Assert.Equal("ID_AND_NAME", Screen("احمد محمد على", "29801011234567", Entry("أحمد محمد علي", "29801011234567")).Hits[0].MatchBasisCode);
    }

    [Fact]
    public void A_lifted_entry_is_previously_listed_not_a_match()
    {
        Assert.Equal(ScrMatcher.PreviouslyListed, Screen("أحمد محمد علي", "29801011234567", Entry("أحمد محمد علي", "29801011234567", "LIFTED")).Result);
        Assert.Equal(ScrMatcher.Match, Screen("أحمد محمد علي", "29801011234567", Entry("أحمد محمد علي", "29801011234567", "SUSPENDED")).Result);
    }

    [Fact]
    public void A_foreigner_is_screened_by_passport()
    {
        var entry = Entry("حسن محمود عبدالعزيز", "A1230455") with { IdentityTypeCode = "PASSPORT", NationalityCode = "SD" };
        Assert.Equal(ScrMatcher.Match, Screen("Hassan Mahmoud", "a1230455", entry).Result);
    }

    [Fact]
    public void Aliases_and_English_names_are_screened_too()
    {
        var entry = Entry("شركة الشرق للاستثمار", "29584", type: "COMPANY", en: "Al Sharq Investment Co", aliases: ["East Investments"]);
        Assert.Equal(ScrMatcher.PossibleMatch, Screen(new ScrSubject("COMPANY", "East Investments", "11111"), entry).Result);
        Assert.Equal(ScrMatcher.PossibleMatch, Screen(new ScrSubject("COMPANY", "Al-Sharq Investment Co.", "11111"), entry).Result);
        // a company is never matched against people
        Assert.Equal(ScrMatcher.NoMatch, Screen(new ScrSubject("PERSON", "East Investments", "29584"), entry).Result);
    }

    [Fact]
    public void A_name_only_match_with_another_birth_date_is_dropped()
    {
        var entry = Entry("أحمد محمد علي", "29801011234567", birth: new DateOnly(1998, 1, 1));
        Assert.Equal(ScrMatcher.NoMatch, Screen(new ScrSubject("PERSON", "أحمد محمد علي", "30001011234567", new DateOnly(2000, 1, 1)), entry).Result);
        Assert.Equal(ScrMatcher.PossibleMatch, Screen(new ScrSubject("PERSON", "أحمد محمد علي", "30001011234567", new DateOnly(1998, 1, 1)), entry).Result);
        // an identity match stands whatever the birth date
        Assert.Equal(ScrMatcher.Match, Screen(new ScrSubject("PERSON", "x", "29801011234567", new DateOnly(2000, 1, 1)), entry).Result);
    }

    // ---------------------------------------------------------------- the Final List

    private static ScrDecision Decision(long id, string type, string status = "PUBLISHED", long? parent = null, int? version = null, string scope = "Freeze",
        params long[] parties) => new()
    {
        DecisionId = id, DecisionRef = $"DEC-{id:D6}", OfficialNo = $"N-{id}", DecisionTypeCode = type, StatusCode = status, ParentDecisionId = parent,
        RegistryVersionNo = version ?? (int)id, ScopeText = scope, EffectiveDate = new DateOnly(2026, 1, (int)Math.Min(id, 28)),
        Parties = [.. parties.Select(p => new ScrDecisionParty { DecisionId = id, PartyId = p })],
    };

    private static readonly Dictionary<long, ScrParty> Parties = new()
    {
        [100] = new() { PartyId = 100, NameAr = "أحمد محمد علي", IdentityNo = "29801011234567" },
        [101] = new() { PartyId = 101, NameAr = "محمد أحمد حسن", IdentityNo = "28506151234567" },
    };

    [Fact]
    public void The_list_follows_the_published_decisions()
    {
        var decisions = new List<ScrDecision>
        {
            Decision(1, "LISTING", parties: [100, 101]),
            Decision(2, "AMENDMENT", parent: 1, scope: "Freeze of bank accounts only", parties: [101]),
            Decision(3, "LIFTING", parent: 1, parties: [100]),
            Decision(4, "SUSPENSION", status: "AWAITING_APPROVAL", parent: 1, parties: [101]), // not published: no effect
            Decision(5, "LISTING", status: "DRAFT", parties: [100]),                             // not published: no entry
        };
        var list = ScrRegistry.Derive(decisions, Parties);
        Assert.Equal(2, list.Count);
        var ahmed = list.Single(e => e.PartyId == 100);
        Assert.Equal("LIFTED", ahmed.StatusCode);
        Assert.Equal(new DateOnly(2026, 1, 3), ahmed.LiftedOn);
        Assert.Equal("Freeze", ahmed.ScopeText);
        var mohamed = list.Single(e => e.PartyId == 101);
        Assert.Equal("ACTIVE", mohamed.StatusCode);
        Assert.Equal("Freeze of bank accounts only", mohamed.ScopeText);

        // no suspension: a party is on the list or not, so a suspension published before changes nothing
        decisions[3].StatusCode = "PUBLISHED";
        Assert.Equal("ACTIVE", ScrRegistry.Derive(decisions, Parties).Single(e => e.PartyId == 101).StatusCode);
        Assert.DoesNotContain("SUSPENSION", ScrDecisionType.All);
    }

    // ---------------------------------------------------------------- segregation of duties

    [Fact]
    public void Duties_are_checked_on_the_person_not_the_account()
    {
        // one person (employee E-7) with a reviewer account and a publisher account: the FINOVA prototype let them do both
        var entered = ScrDuties.PersonKey("E-1", "mohamed.entry");
        var reviewer = ScrDuties.PersonKey("E-7", "ahmed.fawzy.reviewer");
        var sameAsReviewer = ScrDuties.PersonKey("e-7", "ahmed.fawzy.publisher");
        ScrDuties.CheckReview(entered, reviewer);
        Assert.Throws<BusinessRuleException>(() => ScrDuties.CheckApproval(entered, reviewer, sameAsReviewer));
        Assert.Throws<BusinessRuleException>(() => ScrDuties.CheckReview(entered, ScrDuties.PersonKey("E-1", "mohamed.review")));
        ScrDuties.CheckApproval(entered, reviewer, ScrDuties.PersonKey("E-9", "publisher"));
        // without an employee number, the account is the person
        Assert.NotEqual(ScrDuties.PersonKey(null, "a"), ScrDuties.PersonKey(null, "b"));
    }

    [Fact]
    public void Identity_formats()
    {
        var e = new Dictionary<string, string[]>();
        ScrValidation.Identity(e, "PERSON", "NATIONAL_ID", "29801011234567");
        ScrValidation.Identity(e, "PERSON", "PASSPORT", "A1230455");
        ScrValidation.Identity(e, "COMPANY", "COMMERCIAL_REGISTER", "29584");
        Assert.Empty(e);
        ScrValidation.Identity(e, "PERSON", "NATIONAL_ID", "123");
        Assert.Contains("identityNo", e.Keys);
        e.Clear();
        ScrValidation.Identity(e, "COMPANY", "PASSPORT", "A1230455");
        Assert.Contains("identityTypeCode", e.Keys);
    }
}
