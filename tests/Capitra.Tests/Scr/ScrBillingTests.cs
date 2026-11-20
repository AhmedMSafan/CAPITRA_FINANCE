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
}
