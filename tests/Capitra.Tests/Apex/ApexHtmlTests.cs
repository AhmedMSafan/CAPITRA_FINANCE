using Capitra.Application.Modules.Apex;
using Xunit;

namespace Capitra.Tests.Apex;

public class ApexHtmlTests
{
    [Theory]
    [InlineData(1234567.891, "FM999G999G999G990D00", "1,234,567.89")]
    [InlineData(0, "FM999G999G999G990D00", "0.00")]
    [InlineData(68.88, "FM999G990D00", "68.88")]
    [InlineData(2.5, "FM999G990D0", "2.5")]
    [InlineData(2, "FM999G990D0", "2.0")]
    [InlineData(1234, "FM999G990", "1,234")]
    [InlineData(0, "FM999G999G990", "0")]
    [InlineData(-1500.5, "FM999G990D00", "-1,500.50")]
    [InlineData(0.456, "FM990D00", "0.46")]
    [InlineData(1.25, "FM990D0", "1.3")]
    public void ToChar_formats_like_oracle(double value, string mask, string expected) =>
        Assert.Equal(expected, ApexHtml.ToChar((decimal)value, mask));

    [Theory]
    [InlineData(1_250_000_000, "1.3B")]
    [InlineData(7_500_000, "7.5M")]
    [InlineData(4_650, "4.7K")]
    [InlineData(950, "950")]
    public void Money_scales(double value, string expected) => Assert.Equal(expected, ApexHtml.Money((decimal)value));

    [Fact]
    public void Initials_take_first_and_last_word() => Assert.Equal("NU", ApexHtml.Initials("Nile Urban"));
}
