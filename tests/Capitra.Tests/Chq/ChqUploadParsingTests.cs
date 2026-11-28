using Capitra.Application.Modules.Chq.Packages;
using Xunit;

namespace Capitra.Tests.Chq;

/// <summary>Reading cheque upload workbooks: numbers and dates as Excel and people write them.</summary>
public sealed class ChqUploadParsingTests
{
    [Theory]
    [InlineData("1,234.50", "1234.50")]
    [InlineData(" -12 ", "-12")]
    [InlineData(".5", "0.5")]
    [InlineData("100.1234", "100.1234")]
    public void Numbers(string text, string expected) =>
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), ChqUploadPackage.ParseNumber(text));

    [Theory]
    [InlineData("abc")]
    [InlineData("1.23456")]
    [InlineData("")]
    [InlineData(null)]
    public void Not_numbers(string? text) => Assert.Null(ChqUploadPackage.ParseNumber(text));

    [Theory]
    [InlineData("45658", 2025, 1, 1)]       // Excel serial day
    [InlineData("31/12/2025", 2025, 12, 31)] // DD/MM/YYYY
    [InlineData("05-06-2025", 2025, 6, 5)]   // DD-MM-YYYY wins over MM-DD
    [InlineData("2025-12-31", 2025, 12, 31)] // YYYY-MM-DD
    [InlineData("12/31/2025", 2025, 12, 31)] // MM/DD/YYYY when the day cannot be a month
    public void Dates(string text, int y, int m, int d) => Assert.Equal(new DateOnly(y, m, d), ChqUploadPackage.ParseExcelDate(text));

    [Theory]
    [InlineData("31/02/2025")]
    [InlineData("next week")]
    [InlineData("  ")]
    public void Not_dates(string text) => Assert.Null(ChqUploadPackage.ParseExcelDate(text));

    [Fact]
    public void Line_breaks_become_spaces() => Assert.Equal("Bank A", ChqUploadPackage.CleanValue(" Bank\nA \r"));
}
