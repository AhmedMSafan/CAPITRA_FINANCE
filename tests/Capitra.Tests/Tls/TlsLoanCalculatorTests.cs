using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Tls;
using Xunit;

namespace Capitra.Tests.Tls;

/// <summary>Tools > Loan Calculator: the FINOVA engine as ported (reducing balance, day counts, month-end rules, P and I calendars).</summary>
public sealed class TlsLoanCalculatorTests
{
    private static readonly DateOnly Contract = new(2026, 1, 1);

    private static TlsScenarioRequest Combined(string method = "EQUAL_INSTALLMENT", string unit = "MONTH", int tenor = 12, DateOnly? first = null,
        string dayCount = "ACT_365", decimal rate = 12, IReadOnlyList<TlsScenarioRateRequest>? rates = null) =>
        new(null, "Test", null, 12000, Contract, tenor, "MONTH", rate, dayCount, "COMBINED", method, "REGULAR",
            first ?? new(2026, 2, 1), first ?? new(2026, 2, 1), null, null, null, null, null, null, 1, unit, null, rates);

    private static List<TlsLine> Run(TlsScenarioRequest r) => TlsLoanCalculator.Generate(TlsLoanCalculator.Terms(r));

    [Fact]
    public void Equal_instalments_pay_off_the_principal_on_the_reducing_balance()
    {
        var s = Run(Combined());
        Assert.Equal(12, s.Count);
        Assert.Equal(new DateOnly(2027, 1, 1), s[^1].PaymentDate);
        Assert.Equal(12000m, s.Sum(x => x.PrincipalAmount));
        Assert.Equal(0m, s[^1].ClosingPrincipal);
        Assert.Equal(122.30m, s[0].InterestAmount); // 12,000 × 12% × 31/365
        Assert.All(s.Take(11), x => Assert.Equal(s[0].TotalPaymentAmount, x.TotalPaymentAmount));
        Assert.All(s.Skip(1), x => Assert.True(x.InterestAmount < s[0].InterestAmount));
    }

    [Fact]
    public void Equal_principal_and_a_single_payment_at_maturity()
    {
        var s = Run(Combined("EQUAL_PRINCIPAL"));
        Assert.All(s, x => Assert.Equal(1000m, x.PrincipalAmount));
        var one = Run(Combined(unit: "MATURITY"));
        Assert.Single(one);
        Assert.Equal(12000m, one[0].PrincipalAmount);
        Assert.Equal(1440m, one[0].InterestAmount); // 365 days at 12%
    }

    [Fact]
    public void Month_end_rules()
    {
        var eom = Run(Combined(unit: "EOM", first: new(2026, 1, 15), tenor: 3));
        Assert.Equal(new DateOnly[] { new(2026, 1, 15), new(2026, 1, 31), new(2026, 2, 28), new(2026, 3, 31), new(2026, 4, 1) }, eom.Select(x => x.PaymentDate));
        var next = Run(Combined(unit: "NEXT_EOM", first: new(2026, 1, 15), tenor: 3));
        Assert.Equal(new DateOnly[] { new(2026, 1, 15), new(2026, 2, 28), new(2026, 3, 31), new(2026, 4, 1) }, next.Select(x => x.PaymentDate));
        var d30 = Run(Combined(unit: "EOM_30", first: new(2026, 1, 15), tenor: 3));
        Assert.Equal(new DateOnly[] { new(2026, 1, 15), new(2026, 1, 30), new(2026, 2, 28), new(2026, 3, 30), new(2026, 4, 1) }, d30.Select(x => x.PaymentDate));
    }

    [Fact]
    public void Day_counts()
    {
        Assert.Equal(124.00m, Run(Combined(dayCount: "ACT_360"))[0].InterestAmount); // 31/360
        Assert.Equal(120.00m, Run(Combined(dayCount: "30_360"))[0].InterestAmount);  // 30/360
        Assert.Equal(30, Run(Combined(dayCount: "30_365"))[0].AccrualDays);
    }

    [Fact]
    public void Separate_principal_and_interest_dates()
    {
        var r = new TlsScenarioRequest(null, "Sep", null, 12000, Contract, 6, "MONTH", 12, "ACT_365", "SEPARATE", "EQUAL_INSTALLMENT", "REGULAR",
            new(2026, 4, 1), new(2026, 2, 1), 3, "MONTH", null, 1, "MONTH", null, null, null, null, null);
        var t = TlsLoanCalculator.Terms(r);
        Assert.Equal("EQUAL_PRINCIPAL", t.PrincipalMethod);
        var s = TlsLoanCalculator.Generate(t);
        Assert.Equal(6, s.Count);
        Assert.Equal(6000m, s.Single(x => x.PaymentDate == new DateOnly(2026, 4, 1)).PrincipalAmount);
        Assert.Equal(0m, s.Single(x => x.PaymentDate == new DateOnly(2026, 3, 1)).PrincipalAmount);
        Assert.Equal(0m, s[^1].ClosingPrincipal);
    }

    [Fact]
    public void A_later_rate_applies_from_its_date()
    {
        var flat = Run(Combined(unit: "MATURITY"))[0].InterestAmount;
        var t = TlsLoanCalculator.Terms(Combined(unit: "MATURITY", rates: [new(new(2026, 7, 1), 24)]));
        Assert.Equal(new[] { 12m, 24m }, t.Rates.Select(x => x.InterestRatePct));
        Assert.True(TlsLoanCalculator.Generate(t)[0].InterestAmount > flat);
    }

    [Fact]
    public void Bad_terms_are_refused()
    {
        Assert.Throws<BusinessRuleException>(() => Run(Combined() with { PrincipalAmount = 0 }));
        Assert.Throws<BusinessRuleException>(() => Run(Combined(first: new(2025, 12, 1))));
        Assert.Throws<BusinessRuleException>(() => Run(Combined(rate: -1)));
    }
}
