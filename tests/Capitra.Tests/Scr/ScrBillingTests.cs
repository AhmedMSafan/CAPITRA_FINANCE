using Capitra.Application.Modules.Scr;
using Xunit;

namespace Capitra.Tests.Scr;

public sealed class ScrBillingTests
{
    [Theory]
    [InlineData("MONTHLY", "2026-11-20", "2026-11-01", "2026-11-30", "2026-11")]
    [InlineData("MONTHLY", "2028-02-10", "2028-02-01", "2028-02-29", "2028-02")]
    [InlineData("QUARTERLY", "2026-11-20", "2026-10-01", "2026-12-31", "2026-Q4")]
    [InlineData("QUARTERLY", "2026-01-01", "2026-01-01", "2026-03-31", "2026-Q1")]
    [InlineData("YEARLY", "2026-11-20", "2026-01-01", "2026-12-31", "2026")]
    public void A_day_falls_in_its_calendar_period(string cycle, string day, string from, string to, string code)
    {
        var (f, t) = ScrBilling.PeriodOf(cycle, DateOnly.Parse(day, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(DateOnly.Parse(from, System.Globalization.CultureInfo.InvariantCulture), f);
        Assert.Equal(DateOnly.Parse(to, System.Globalization.CultureInfo.InvariantCulture), t);
        Assert.Equal(code, ScrBilling.PeriodCode(cycle, f));
        Assert.Equal(cycle, ScrBilling.CycleOf(f, t));
    }

    [Fact]
    public void Cairo_days_start_two_hours_before_UTC_midnight_in_winter()
    {
        Assert.Equal(new DateTime(2026, 11, 30, 22, 0, 0, DateTimeKind.Utc), ScrBilling.CairoStartUtc(new DateOnly(2026, 12, 1)));
        Assert.Equal(new DateOnly(2026, 12, 1), ScrBilling.CairoDate(new DateTime(2026, 11, 30, 22, 30, 0, DateTimeKind.Utc)));
    }

    [Theory]
    [InlineData("2.345", "2.35")]
    [InlineData("2.344", "2.34")]
    [InlineData("-2.345", "-2.35")]
    public void Money_rounds_half_away_from_zero(string value, string expected) =>
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            ScrBilling.Money(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));

    [Fact]
    public void Inquiries_are_priced_at_the_rate_of_the_band_they_fall_in()
    {
        // first 100 free, 101-1000 at 2.50, 1001 and up at 2.00
        var schedule = ScrBilling.Schedule(100, 0m, [new ScrTierDto(101, 1000, 2.50m), new ScrTierDto(1001, null, 2.00m)]);
        var bands = ScrBilling.Price(schedule, 1250);
        Assert.Equal([100, 900, 250], bands.Select(b => b.Quantity));
        Assert.Equal(["FREE", "TIER", "TIER"], bands.Select(b => b.BandTypeCode));
        Assert.Equal(2250m + 500m, bands.Sum(b => b.Amount));
        Assert.Equal([60], ScrBilling.Price(schedule, 60).Select(b => b.Quantity));
        Assert.Equal(0m, ScrBilling.Price(schedule, 60).Sum(b => b.Amount));
        Assert.Empty(ScrBilling.Price(schedule, 0));
    }

    [Fact]
    public void Without_tiers_every_inquiry_after_the_free_ones_pays_the_flat_fee()
    {
        var bands = ScrBilling.Price(ScrBilling.Schedule(10, 3.25m, []), 14);
        Assert.Equal([("FREE", 10, 0m), ("FLAT", 4, 13.00m)], bands.Select(b => (b.BandTypeCode, b.Quantity, b.Amount)));
        Assert.Equal(13.00m, ScrBilling.Price(ScrBilling.Schedule(0, 3.25m, []), 4).Single().Amount);
    }

    [Theory]
    [InlineData(100, 101, 1000, 1001, true)]   // right after the free inquiries, no gap, last open
    [InlineData(100, 1, 1000, 1001, false)]    // overlaps the free inquiries
    [InlineData(0, 1, 1000, 1002, false)]      // gap between tiers
    public void Tiers_follow_the_free_inquiries_without_gaps(int free, int from1, int to1, int from2, bool valid)
    {
        var e = new Dictionary<string, string[]>();
        ScrBilling.ValidateTiers(e, free, [new ScrTierDto(from1, to1, 2m), new ScrTierDto(from2, null, 1m)]);
        Assert.Equal(valid, e.Count == 0);
    }

    [Fact]
    public void The_last_tier_has_no_end()
    {
        var e = new Dictionary<string, string[]>();
        ScrBilling.ValidateTiers(e, 0, [new ScrTierDto(1, 100, 2m)]);
        Assert.True(e.ContainsKey("tiers"));
    }
}
