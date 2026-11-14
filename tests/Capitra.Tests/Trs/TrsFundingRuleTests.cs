using Capitra.Application.Modules.Trs;
using Xunit;

namespace Capitra.Tests.Trs;

/// <summary>Treasury Disburse: why a sublimit can or cannot fund a disbursement, and what a disbursement is for the bank rules.</summary>
public sealed class TrsFundingRuleTests
{
    [Fact]
    public void A_usable_sublimit_says_what_it_funds()
    {
        Assert.Equal(("Funds every business line.", (string?)null), TrsDisburseService.Availability("ACTIVE", "ACTIVE", "ACTIVE", [], "FACTORING", 100));
        Assert.Equal(("Funds Factoring, Leasing.", (string?)null),
            TrsDisburseService.Availability("ACTIVE", "ACTIVE", "ACTIVE", ["FACTORING", "LEASING"], "FACTORING", 100));
    }

    [Theory]
    [InlineData("SUSPENDED", "ACTIVE", "ACTIVE", "The facility is suspended.")]
    [InlineData("UNDER_RENEWAL", "ACTIVE", "ACTIVE", "The facility is under renewal.")]
    [InlineData("ACTIVE", "DRAFT", "ACTIVE", "The bank has not released this tranche yet.")]
    [InlineData("ACTIVE", "ACTIVE", "SUSPENDED", "The sublimit is suspended.")]
    public void A_status_stops_the_sublimit(string facility, string tranche, string sublimit, string reason) =>
        Assert.Equal(reason, TrsDisburseService.Availability(facility, tranche, sublimit, [], "FACTORING", 100).Unavailable);

    [Fact]
    public void Another_business_line_or_nothing_left_stops_it()
    {
        Assert.Equal("It funds Leasing only.", TrsDisburseService.Availability("ACTIVE", "ACTIVE", "ACTIVE", ["LEASING"], "FACTORING", 100).Unavailable);
        Assert.Equal("Nothing is left to draw.", TrsDisburseService.Availability("ACTIVE", "ACTIVE", "ACTIVE", [], "FACTORING", 0).Unavailable);
    }

    [Fact]
    public void A_profile_gives_its_member_of_each_dimension()
    {
        var p = new TrsFundingProfile("REVERSE_FACTORING", "RF_STD", "SME");
        Assert.Equal("REVERSE_FACTORING", p.Member("BUSINESS_LINE"));
        Assert.Equal("RF_STD", p.Member("PRODUCT"));
        Assert.Equal("SME", p.Member("CLIENT_SEGMENT"));
        Assert.Null(p.Member("DEBTOR"));
        Assert.Equal("REVERSE_FACTORING", TrsFundingProfiles.BusinessLineOf("Reverse Factoring"));
        Assert.Equal("FACTORING", TrsFundingProfiles.BusinessLineOf(null));
        Assert.Equal("REVERSE_FACTORING", TrsFundingProfiles.BusinessLineOf("Factoring", "REVERSE"));  // the product decides, as at Invoice Entry
        Assert.Equal("FACTORING", TrsFundingProfiles.BusinessLineOf("Factoring", "CLASSIC"));
    }

    [Fact]
    public void A_share_cap_on_a_whole_business_line_lets_the_pool_grow()
    {
        // Leasing at most 70% of a funded pool of 1,000,000 that already holds 600,000 of leasing: (0.7·1,000,000 − 600,000)/(1 − 0.7)
        Assert.Equal(333_333m, Math.Floor(TrsDisburseService.PctCap(0.7m, "FUNDED_POOL", 1m, 600_000m, 1_000_000m, 0)!.Value));
    }
}
