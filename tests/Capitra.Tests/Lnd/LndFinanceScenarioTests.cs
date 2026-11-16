using Capitra.Application.Modules.Lnd;
using Capitra.Application.Modules.Tls;
using Capitra.Domain.Modules.Tls;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>New Finance from a saved scenario of the Finance Calculator: one invoice per date (principal with interest) and one rate.</summary>
public sealed class LndFinanceScenarioTests
{
    private static readonly TlsLoanScenario Scenario = new() { ScenarioId = 1, ScenarioName = "S", PrincipalAmount = 2000 };

    private static TlsLoanScenarioLine Line(int no, decimal principal, decimal interest) =>
        new() { InstallmentNo = no, PaymentDate = new DateOnly(2026, 12, 1).AddMonths(no - 1), PrincipalAmount = principal, InterestAmount = interest };

    private static List<TlsLoanScenarioRate> Rate(params decimal[] pcts) =>
        [.. pcts.Select((p, i) => new TlsLoanScenarioRate { EffectiveDate = new DateOnly(2026, 11, 15).AddMonths(i), InterestRatePct = p })];

    [Fact]
    public void A_scenario_with_principal_and_interest_on_every_date_and_one_rate_is_accepted()
    {
        Assert.Empty(LndLoanService.CheckScenario(Scenario, Rate(22), [Line(1, 1000, 30), Line(2, 1000, 15)]));
    }

    [Fact]
    public void Separate_dates_a_rate_change_or_no_schedule_are_refused()
    {
        Assert.Contains("scenarioId", LndLoanService.CheckScenario(Scenario, Rate(22), [Line(1, 0, 30), Line(2, 2000, 15)]).Keys);
        Assert.Contains("scenarioId", LndLoanService.CheckScenario(Scenario, Rate(22), [Line(1, 1000, 0), Line(2, 1000, 15)]).Keys);
        Assert.Contains("scenarioId", LndLoanService.CheckScenario(Scenario, Rate(22, 25), [Line(1, 1000, 30), Line(2, 1000, 15)]).Keys);
        Assert.Contains("scenarioId", LndLoanService.CheckScenario(Scenario, Rate(22), []).Keys);
        Assert.Empty(LndLoanService.CheckScenario(Scenario, Rate(0), [Line(1, 1000, 0), Line(2, 1000, 0)]));
    }

    [Fact]
    public void Term_and_day_count()
    {
        Assert.Equal(12, LndLoanService.TermMonths(new(2026, 11, 15), new(2027, 11, 15)));
        Assert.Equal(13, LndLoanService.TermMonths(new(2026, 11, 15), new(2027, 11, 30)));
        Assert.Equal("THIRTY_360", LndLoanService.DayCount("30_360"));
        Assert.Equal("ACT_360", LndLoanService.DayCount("ACT_360"));
        Assert.Equal("ACT_365", LndLoanService.DayCount("ACT_ACT"));
    
    [Fact]
    public void A_new_rate_works_the_schedule_out_again_on_the_same_dates()
    {
        var s = new TlsLoanScenario
        {
            ScenarioName = "Level", PrincipalAmount = 12000, ContractDate = new(2026, 11, 16), TenorValue = 12, TenorUnit = "MONTH", ScheduleMode = "COMBINED",
            PrincipalMethod = "EQUAL_INSTALLMENT", DayCountMethod = "ACT_365", PrincipalPaymentStartDate = new(2026, 12, 31), InterestPaymentStartDate = new(2026, 12, 31),
            CombinedFrequencyValue = 1, CombinedFrequencyUnit = "EOM",
        };
        var at22 = TlsLoanCalculator.Generate(TlsLoanCalculator.Terms(LndLoanService.Request(s, 22)));
        var at18 = TlsLoanCalculator.Generate(TlsLoanCalculator.Terms(LndLoanService.Request(s, 18)));
        Assert.Equal(at22.Select(x => x.PaymentDate), at18.Select(x => x.PaymentDate));
        Assert.True(at18.Sum(x => x.InterestAmount) < at22.Sum(x => x.InterestAmount));
        Assert.Equal(12000m, at18.Sum(x => x.PrincipalAmount));
    }
}
