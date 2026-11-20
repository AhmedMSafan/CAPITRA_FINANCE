using Capitra.Application.Modules.Trs;
using Xunit;

namespace Capitra.Tests.Trs;

/// <summary>The most a disbursement may draw from a bank before one party passes a concentration rule (Treasury Disburse).</summary>
public sealed class TrsDisburseCapTests
{
    [Fact]
    public void Funded_pool_cap_counts_the_drawing_in_the_pool()
    {
        // Bank A: any debtor at most 15% of the funded pool; pool 200M, LG holds 28M, LG is the whole disbursement
        var cap = TrsDisburseService.PctCap(0.15m, "FUNDED_POOL", 1m, 28_000_000m, 200_000_000m, 0m)!.Value;
        Assert.Equal(2_352_941m, Math.Floor(cap));
        Assert.True((28_000_000m + cap) / (200_000_000m + cap) <= 0.15m + 0.000001m);
    }

    [Fact]
    public void Funded_pool_has_no_cap_when_the_party_takes_a_smaller_share_than_the_limit()
    {
        Assert.Null(TrsDisburseService.PctCap(0.15m, "FUNDED_POOL", 0.10m, 28_000_000m, 200_000_000m, 0m));
    }

    [Fact]
    public void Facility_limit_cap_is_the_room_left_divided_by_the_share()
    {
        // 15% of a 100M facility = 15M; LG holds 12M and takes half of the disbursement: 3M of room = 6M of disbursement
        Assert.Equal(6_000_000m, TrsDisburseService.PctCap(0.15m, "FACILITY_LIMIT", 0.5m, 12_000_000m, 0m, 100_000_000m));
    }
}
