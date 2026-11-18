using Capitra.Application.Common;
using Xunit;

namespace Capitra.Tests.Common;

/// <summary>The one rounding rule: half away from zero, where .NET's default rounds half to even.</summary>
public sealed class MoneyTests
{
    [Theory]
    [InlineData("0.125", 2, "0.13")]
    [InlineData("0.135", 2, "0.14")]
    [InlineData("-0.125", 2, "-0.13")]
    [InlineData("2.5", 0, "3")]
    [InlineData("1.00005", 4, "1.0001")]
    [InlineData("1234.5675", 3, "1234.568")]
    public void Halves_go_away_from_zero(string value, int decimals, string expected) =>
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
            Money.Round(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture), decimals));

    [Fact]
    public void Two_decimals_by_default_and_null_stays_null()
    {
        Assert.Equal(10.13m, Money.Round(10.125m));
        Assert.Null(Money.Round((decimal?)null));
        Assert.Equal(0.12m, Math.Round(0.125m, 2)); // what Money.Round replaces
    }
}
