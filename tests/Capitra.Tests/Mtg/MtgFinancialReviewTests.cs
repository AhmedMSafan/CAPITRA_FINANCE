using Capitra.Application.Modules.Mtg.Pages.Custom;
using Xunit;

namespace Capitra.Tests.Mtg;

/// <summary>Purchased portfolio financial review: Oracle months_between and the present value of each cheque.</summary>
public sealed class MtgFinancialReviewTests
{
    [Fact]
    public void Months_between_as_Oracle()
    {
        Assert.Equal(2m, MtgAjax.MonthsBetween(new(2026, 3, 15), new(2026, 1, 15)));
        Assert.Equal(1m, MtgAjax.MonthsBetween(new(2026, 3, 31), new(2026, 2, 28))); // both month ends
        Assert.Equal(15 / 31m, MtgAjax.MonthsBetween(new(2026, 3, 20), new(2026, 3, 5)));
        Assert.Equal(-2m, MtgAjax.MonthsBetween(new(2026, 1, 15), new(2026, 3, 15)));
    }

    [Fact]
    public void Present_value_by_method()
    {
        Assert.Equal(8928.57m, FinancialReviewMath.PresentValue(10000, 0.12m, "SIMPLE", 365, 12));
        Assert.Equal(8928.57m, FinancialReviewMath.PresentValue(10000, 0.12m, "ANNUAL", 365, 12));
        Assert.Equal(8874.49m, FinancialReviewMath.PresentValue(10000, 0.12m, "MONTHLY", 365, 12));
        Assert.Equal(10000.13m, FinancialReviewMath.PresentValue(10000.125m, 0, "SIMPLE", 365, 12)); // no rate: the amount
    }

    [Fact]
    public void A_cheque_past_due_or_without_a_date_has_no_period()
    {
        var today = new DateOnly(2026, 6, 1);
        Assert.Equal((0m, 0m), FinancialReviewMath.Period(null, today));
        Assert.Equal((0m, 0m), FinancialReviewMath.Period(new(2026, 5, 1), today));
        Assert.Equal((30m, 1m), FinancialReviewMath.Period(new(2026, 7, 1), today));
    }
}
