using Capitra.Application.Modules.Fct;
using Xunit;

namespace Capitra.Tests.Fct;

/// <summary>The factoring pricing calculator (FactoringPricingService): advance rate, discount rate and income.</summary>
public sealed class FctPricingTests
{
    private static FctCalculationRequest Request(string band = "MEDIUM", decimal baseAdvance = 85, decimal dilution = 3, decimal concentration = 30) => new(
        RequestedLimit: 1_000_000, ExpectedVolume: 12_000_000, AverageTenorDays: 120, NumDebtors: 20, EstTransactionsPerYear: 400,
        RiskBand: band, BaseAdvanceRatio: baseAdvance, DilutionRate: dilution, RiskBufferOverride: null, LargestConcentration: concentration,
        CostOfFundsRate: 15, OperatingSpread: 2, SetupFeeFixed: 1000, SetupFeePct: 1, CommissionPct: 0.5m,
        HandlingFee: 25, DebtorFee: 100, DebtorMinimum: 5000);

    [Fact]
    public void A_medium_risk_offer()
    {
        var r = FctApplicationService.Calculate(Request(), 0.50m);
        Assert.Equal(-4.5m, r.AdvanceRiskBufferPct);   // 4% medium + (30% - 25%) × 10%
        Assert.Equal(77.5m, r.AdvancePct);             // 85 - 3 - 4.5
        Assert.Equal(0.25m, r.DiscountTenorAdjustmentPct); // (120 - 90) / 30 × 0.25%
        Assert.Equal(0.5m, r.DiscountConcentrationPct);    // largest debtor over 25%
        Assert.Equal(20m, r.DiscountRatePct);          // 15 + 2.25 + 2 + 0.25 + 0.5
        Assert.Equal(5m, r.SpreadOverLcrPct);
        Assert.Equal(11_000m, r.SetupFeeTotal);         // 1,000 + 1% of 1,000,000
        Assert.Equal(60_000m, r.CommissionAnnual);
        Assert.Equal(10_000m, r.HandlingAnnual);
        Assert.Equal(5_000m, r.DebtorFeesTotal);        // the minimum is more than 20 × 100
        Assert.Equal(611_506.85m, Math.Round(r.InterestIncome, 2, MidpointRounding.AwayFromZero));
        Assert.Equal(86_000m, r.TotalAnnualIncome);     // fees only, as confirmed in FINOVA
    }

    [Fact]
    public void The_advance_never_falls_below_the_minimum()
    {
        var r = FctApplicationService.Calculate(Request(band: "HIGH", baseAdvance: 50, dilution: 10), 0.50m);
        Assert.Equal(50m, r.AdvancePct);
        Assert.Equal(3.5m, r.DiscountRiskMarginPct);
    }

    [Fact]
    public void Low_risk_has_the_smallest_buffer_and_margin()
    {
        var r = FctApplicationService.Calculate(Request(band: "low", concentration: 10), 0.50m);
        Assert.Equal(-2m, r.AdvanceRiskBufferPct);
        Assert.Equal(1.25m, r.DiscountRiskMarginPct);
        Assert.Equal(0m, r.DiscountConcentrationPct);
    }

    [Fact]
    public void Financed_amount_prefers_the_advance_then_the_funded_amount_then_the_rate()
    {
        Assert.Equal(700m, FctDisbursementSummaryService.Financed(1000, 700, 600, 80));
        Assert.Equal(600m, FctDisbursementSummaryService.Financed(1000, null, 600, 80));
        Assert.Equal(800m, FctDisbursementSummaryService.Financed(1000, null, null, 80));
        Assert.Equal(1000m, FctDisbursementSummaryService.Financed(1000, null, null, null));
    }
}
