using Capitra.Application.Modules.Mff.Packages;
using Xunit;

namespace Capitra.Tests.Mff;

/// <summary>The new MFF_CALC_PKG against the Fund calculator's reference cases (MFF_CALCULATION_RUNS 2001-2003, SOURCE_SAMPLE).</summary>
public sealed class MffCalcTests
{
    // income, property, term, growth %, growth years, PTI %, assist | loan, payment, LTV, borrower DP, max property, min property
    [Theory]
    [InlineData(4500, 321943, 27, 7, 27, 11.25, 0, 274956, 506, 85, 46987, 549913, 305507)]
    [InlineData(1500, 206850, 27, 5, 27, 11, 60000, 67913, 165, 33, 78937, 255826, 142126)]
    [InlineData(2000, 206850, 30, 0, 0, 14, 60000, 66413, 280, 32, 80437, 252826, 140459)]
    public void FundReferenceCases(decimal income, decimal property, decimal term, decimal growth, decimal growthYears, decimal pti, decimal assist,
        decimal loan, decimal payment, decimal ltv, decimal dp, decimal maxProp, decimal minProp)
    {
        var o = MffCalcPackage.Calculate(new MffCalcPackage.Inputs(income, property, 3, term, growth, growthYears, pti, assist, 0, 0, 0, 10, 50, 60000));
        Assert.Equal(loan, Math.Round(o.MortgageLoan));
        Assert.Equal(payment, Math.Round(o.InitialPayment));
        Assert.Equal(ltv, Math.Round(o.LtvPct));
        Assert.Equal(dp, Math.Round(o.BorrowerDownpayment));
        Assert.Equal(maxProp, Math.Round(o.MaxPropertyValue));
        Assert.Equal(minProp, Math.Round(o.MinPropertyValue));
        Assert.Equal(assist, o.TotalSubsidy);
        Assert.False(o.IsNegativeAmortization);
    }
}

/// <summary>The new MFF_SERVICING_PKG.generate_installments schedule.</summary>
public sealed class MffServicingTests
{
    [Fact]
    public void LevelScheduleMatchesTheLevelPayment()
    {
        var (initial, rows) = MffServicingPackage.Schedule(1000000, 8, 240, new DateOnly(2026, 12, 1), 0, 0, 0, 0);
        Assert.Equal(MffAmortPackage.LevelPayment(1000000, 8, 240), initial);
        Assert.Equal(240, rows.Count);
        Assert.Equal(0, rows[^1].ClosingPrincipal);
        Assert.Equal(1000000, rows.Sum(x => x.PrincipalDue));
        Assert.Equal(new DateOnly(2046, 11, 1), rows[^1].DueDate);
    }

    [Fact]
    public void GrowingScheduleWithSubsidyRepaysTheLoan()
    {
        var (initial, rows) = MffServicingPackage.Schedule(700000, 8, 240, new DateOnly(2026, 12, 1), 5, 5, 500, 24);
        Assert.Equal(500, rows[23].SubsidyDue);
        Assert.Equal(0, rows[24].SubsidyDue);
        Assert.Equal(Math.Round(initial * 1.05m, 2), rows[12].BorrowerDue);
        Assert.Equal(rows[60].BorrowerDue, rows[100].BorrowerDue);
        Assert.Equal(0, rows[^1].ClosingPrincipal);
        Assert.Equal(700000, rows.Sum(x => x.PrincipalDue));
        Assert.InRange(rows[^1].BorrowerDue!.Value, rows[100].BorrowerDue!.Value - 1, rows[100].BorrowerDue!.Value + 1);
    }
}
