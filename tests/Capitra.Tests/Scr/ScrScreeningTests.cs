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
    public void An_entry_is_shown_when_the_ID_or_the_name_reaches_the_level()
    {
        var entry = Entry("أحمد محمد علي", "29801011234567");
        // same ID, same name
        var (hits, result) = Screen("احمد محمد على", "29801011234567", entry);
        Assert.Equal(ScrMatcher.Found, result);
        Assert.Equal(("EXACT", 100m, "ID_AND_NAME"), (Assert.Single(hits).IdMatchCode, hits[0].IdScorePct, hits[0].MatchBasisCode));
        // a mistyped ID with the same name is shown by the name
        Assert.Equal("NAME", Assert.Single(Screen("أحمد محمد علي", "29801011234568", entry).Hits).MatchBasisCode);
        // the same ID with another name is shown by the ID, with a low name level
        var h = Assert.Single(Screen("Someone Else", "29801011234567", entry).Hits);
        Assert.Equal("ID", h.MatchBasisCode);
        Assert.True(h.NameScorePct < ScrMatcher.DefaultNameMin);
        // neither reaches the level: not shown
        Assert.Equal(ScrMatcher.NotFound, Screen("Someone Else", "11111111111111", entry).Result);
        // Salah Ahmed with the ID of Salah Fouad Ramadan: shown
        var salah = Entry("Salah Fouad Ramadan", "26212130193458");
        Assert.Equal("ID", Assert.Single(ScrMatcher.Screen(new ScrSubject("PERSON", "Salah Ahmed", "26212130193458"), [salah])).MatchBasisCode);
    }

    [Fact]
    public void The_user_chooses_how_many_ID_characters_must_agree()
    {
        var entry = Entry("أحمد محمد علي", "29801011234567");
        var s = new ScrSubject("PERSON", "Someone Else", "29801011234568");
        Assert.Empty(ScrMatcher.Screen(s, [entry]));
        var h = Assert.Single(ScrMatcher.Screen(s, [entry], idMinDigits: 13));
        Assert.Equal(("PARTIAL", 1, 92.86m, "ID"), (h.IdMatchCode, h.IdDigitsDifferent, h.IdScorePct, h.MatchBasisCode));
        Assert.Empty(ScrMatcher.Screen(new ScrSubject("PERSON", "Someone Else", "29801011234999"), [entry], idMinDigits: 13));
    }

    [Fact]
    public void The_user_chooses_how_alike_the_name_must_be()
    {
        var entry = Entry("Ahmed Mohamed Ali", "29801011234567");
        var s = new ScrSubject("PERSON", "Ahmed Mohammed Aly", "11111111111111");
        var score = ScrNames.Score(s.Name, entry.NameAr);
        Assert.Equal("NAME", Assert.Single(ScrMatcher.Screen(s, [entry], (int)Math.Floor(score))).MatchBasisCode);
        Assert.Empty(ScrMatcher.Screen(s, [entry], (int)Math.Floor(score) + 1));
    }

    [Fact]
    public void Lifted_and_suspended_entries_are_shown_with_their_status()
    {
        Assert.Equal("LIFTED", Screen("أحمد محمد علي", "29801011234567", Entry("أحمد محمد علي", "29801011234567", "LIFTED")).Hits[0].Entry.StatusCode);
        Assert.Equal(ScrMatcher.Found, Screen("أحمد محمد علي", "29801011234567", Entry("أحمد محمد علي", "29801011234567", "SUSPENDED")).Result);
    }

    [Fact]
    public void A_foreigner_is_screened_by_passport()
    {
        var entry = Entry("حسن محمود عبدالعزيز", "A1230455") with { IdentityTypeCode = "PASSPORT", NationalityCode = "SD" };
        var h = Assert.Single(ScrMatcher.Screen(new ScrSubject("PERSON", "حسن محمود عبد العزيز", "a1230455", null, "SD"), [entry]));
        Assert.Equal(("EXACT", "SAME"), (h.IdMatchCode, h.NationalityMatchCode));
    }

    [Fact]
    public void Aliases_and_English_names_are_screened_too()
    {
        var entry = Entry("شركة الشرق للاستثمار", "29584", type: "COMPANY", en: "Al Sharq Investment Co", aliases: ["East Investments"]);
        Assert.Equal(ScrMatcher.Found, Screen(new ScrSubject("COMPANY", "East Investments", "29584"), entry).Result);
        Assert.Equal(ScrMatcher.Found, Screen(new ScrSubject("COMPANY", "Al-Sharq Investment Co.", "29584"), entry).Result);
        // a company is never matched against people
        Assert.Equal(ScrMatcher.NotFound, Screen(new ScrSubject("PERSON", "East Investments", "29584"), entry).Result);
    }

    [Fact]
    public void The_birth_date_and_nationality_are_compared_not_used_to_hide()
    {
        var entry = Entry("أحمد محمد علي", "29801011234567", birth: new DateOnly(1998, 1, 1));
        var other = Screen(new ScrSubject("PERSON", "أحمد محمد علي", "29801011234567", new DateOnly(2000, 1, 1), "EG"), entry).Hits;
        Assert.Equal(("DIFFERENT", "SAME"), (Assert.Single(other).BirthMatchCode, other[0].NationalityMatchCode));
        Assert.Equal("SAME", Screen(new ScrSubject("PERSON", "أحمد محمد علي", "29801011234567", new DateOnly(1998, 1, 1)), entry).Hits[0].BirthMatchCode);
        Assert.Equal(("NOT_GIVEN", "NOT_GIVEN"), (Screen("أحمد محمد علي", "29801011234567", entry).Hits[0].BirthMatchCode,
            Screen("أحمد محمد علي", "29801011234567", entry).Hits[0].NationalityMatchCode));
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

        decisions[3].StatusCode = "PUBLISHED";
        Assert.Equal("SUSPENDED", ScrRegistry.Derive(decisions, Parties).Single(e => e.PartyId == 101).StatusCode);
    }

    // ---------------------------------------------------------------- seals

    [Fact]
    public void A_seal_changes_with_any_change_and_chains_to_the_one_before()
    {
        static ScrInquiry Inquiry() => new()
        {
            InquiryRef = "SCR-2026-000010", MemberCompanyId = 3, RequestedBy = "nfc.kyc1", ChannelCode = "PORTAL", SubjectTypeCode = "PERSON",
            SubjectName = "Ahmed Ali", IdentityTypeCode = "NATIONAL_ID", IdentityNo = "29801011234567", PurposeCode = "ONBOARDING", ResultCode = "MATCH",
            RegistryVersionNo = 7, ScreenedAt = new DateTime(2026, 11, 22, 9, 0, 0, DateTimeKind.Utc).AddTicks(1234567),
            Hits = [new() { PartyId = 100, DecisionId = 1, MatchBasisCode = "ID_AND_NAME", NameScorePct = 100m, MatchedName = "أحمد علي", EntryStatusCode = "ACTIVE" }],
        };
        var seal = ScrInquirySeal.Hash(Inquiry(), "abc");
        Assert.Equal(64, seal.Length);
        Assert.Equal(seal, ScrInquirySeal.Hash(Inquiry(), "abc"));
        // the time as the database keeps it (microseconds) seals the same
        var read = Inquiry();
        read.ScreenedAt = ScrInquirySeal.Micro(read.ScreenedAt);
        Assert.Equal(seal, ScrInquirySeal.Hash(read, "abc"));
        Assert.NotEqual(seal, ScrInquirySeal.Hash(Inquiry(), "abd"));
        var changed = Inquiry();
        changed.ResultCode = "NO_MATCH";
        Assert.NotEqual(seal, ScrInquirySeal.Hash(changed, "abc"));
        var hit = Inquiry();
        hit.Hits[0].NameScorePct = 99.5m;
        Assert.NotEqual(seal, ScrInquirySeal.Hash(hit, "abc"));
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
