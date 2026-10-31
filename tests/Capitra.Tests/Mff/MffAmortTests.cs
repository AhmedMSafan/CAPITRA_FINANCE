using Capitra.Application.Modules.Mff.Packages;
using Xunit;

namespace Capitra.Tests.Mff;

/// <summary>MFF_AMORT_PKG.LEVEL_PAYMENT, against the values Oracle gives.</summary>
public sealed class MffAmortTests
{
    [Theory]
    [InlineData(1000000, 8, 240, 8364.40)]
    [InlineData(500000, 0, 120, 4166.67)]
    [InlineData(0, 8, 240, 0)]
    [InlineData(100000, 8, 0, 0)]
    public void LevelPayment(decimal principal, decimal rate, int term, decimal expected) =>
        Assert.Equal(expected, MffAmortPackage.LevelPayment(principal, rate, term));
}
