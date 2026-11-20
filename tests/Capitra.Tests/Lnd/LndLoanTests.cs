using Capitra.Application.Modules.Lnd;
using Xunit;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Tests.Lnd;

/// <summary>Retail Lending loan core: the schedule engine, the loan's checks against its product version and the status transitions.</summary>
public sealed class LndLoanTests
{
    private static readonly DateOnly First = new(2026, 12, 15);

    [Fact]
    public void A_declining_loan_has_equal_instalments_and_repays_the_amount()
    {
        // EGP 100,000 at 24% a year, 12 monthly instalments
        var s = LndSchedule.Build(100_000, 12, "MONTHLY", "DECLINING", 24, First);
        Assert.Equal(12, s.Count);
        Assert.Equal(9_455.96m, s[0].Total);
        Assert.Equal(2_000.00m, s[0].Interest);
        Assert.Equal(185.41m, s[^1].Interest);
        Assert.Equal(9_455.97m, s[^1].Total);
        Assert.Equal(100_000m, s.Sum(x => x.Principal));
        Assert.Equal(13_471.53m, s.Sum(x => x.Interest));
        Assert.Equal(0m, s[^1].ClosingPrincipal);
        Assert.Equal(new DateOnly(2027, 11, 15), s[^1].DueDate);
    }

    [Fact]
    public void A_flat_loan_spreads_principal_and_interest_equally()
    {
        // EGP 100,000 at 20% flat a year for 2 years: interest 40,000
        var s = LndSchedule.Build(100_000, 24, "MONTHLY", "FLAT", 20, First);
        Assert.Equal(24, s.Count);
        Assert.Equal(4_166.67m, s[0].Principal);
        Assert.Equal(1_666.67m, s[0].Interest);
        Assert.Equal(4_166.59m, s[^1].Principal);
        Assert.Equal(1_666.59m, s[^1].Interest);
        Assert.Equal(100_000m, s.Sum(x => x.Principal));
        Assert.Equal(40_000m, s.Sum(x => x.Interest));
    }

    [Theory]
    [InlineData(12, "WEEKLY", 52)]
    [InlineData(6, "FORTNIGHTLY", 13)]
    [InlineData(18, "MONTHLY", 18)]
    public void The_term_gives_the_number_of_instalments(int months, string frequency, int count) =>
        Assert.Equal(count, LndSchedule.InstallmentCount(months, frequency));

    [Fact]
    public void Weekly_instalments_fall_every_seven_days_and_a_zero_rate_has_no_interest()
    {
        var s = LndSchedule.Build(5_200, 12, "WEEKLY", "DECLINING", 0, First);
        Assert.Equal(52, s.Count);
        Assert.Equal(First.AddDays(7), s[1].DueDate);
        Assert.All(s, x => Assert.Equal(100m, x.Total));
        Assert.Equal(0m, s.Sum(x => x.Interest));
    }

    [Fact]
    public void A_loan_must_stay_within_its_product_version()
    {
        var v = LndProductTerms.Defaults(1); // 1,000 to 50,000; 3 to 36 months
        var d = new DateOnly(2026, 11, 15);
        Assert.Empty(LndLoanService.CheckTerms(v, 20_000, 12, d, d.AddMonths(1)));
        Assert.Equal(["firstDueDate", "principalAmount", "termMonths"],
            LndLoanService.CheckTerms(v, 60_000, 48, d, d).Keys.Order(StringComparer.Ordinal));
        Assert.Contains("firstDueDate", LndLoanService.CheckTerms(v, 20_000, 12, d, d.AddMonths(4)).Keys);
    }

    [Fact]
    public void Status_changes_follow_the_transition_table()
    {
        Assert.True(LndLoanService.CanMove(St.Approved, St.Cancelled));
        Assert.True(LndLoanService.CanMove(St.Approved, St.Active));
        Assert.False(LndLoanService.CanMove(St.Active, St.Cancelled));
        Assert.False(LndLoanService.CanMove(St.Cancelled, St.Approved));
        // a closed loan reopens only when one of its receipts is reversed
        Assert.True(LndLoanService.CanMove(St.Closed, St.Active));
        Assert.False(LndLoanService.CanMove(St.Closed, St.Cancelled));
        Assert.False(LndLoanService.CanMove(St.WrittenOff, St.Active));
    }
}
