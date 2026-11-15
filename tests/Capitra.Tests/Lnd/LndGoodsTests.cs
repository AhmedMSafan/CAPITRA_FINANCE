using Capitra.Application.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>The financed item of a consumer finance loan.</summary>
public sealed class LndGoodsTests
{
    private static readonly string[] Active = ["ELECTRONICS", "MOBILE"];

    private static LndLoanRequestDto Request(string? category = "ELECTRONICS", string? description = "Samsung 55 inch TV", decimal? price = 30000,
        decimal? down = 5000, decimal amount = 25000) =>
        new(1, 1, amount, 12, null, null, null, category, description, "UA55", "B.TECH", price, down);

    [Fact]
    public void A_complete_item_passes_and_the_amount_is_the_price_less_the_down_payment()
    {
        Assert.Empty(LndLoanService.CheckGoods(Request(), 25000, Active));
        Assert.Empty(LndLoanService.CheckGoods(Request(down: null, amount: 30000), 30000, Active));
    }

    [Fact]
    public void Category_description_and_price_are_needed()
    {
        var e = LndLoanService.CheckGoods(Request(category: null, description: " ", price: null), 25000, Active);
        Assert.Contains("goodsCategoryCode", e.Keys);
        Assert.Contains("goodsDescription", e.Keys);
        Assert.Contains("goodsPriceAmount", e.Keys);
        Assert.Contains("goodsCategoryCode", LndLoanService.CheckGoods(Request(category: "TRAVEL"), 25000, Active).Keys);
    }

    [Fact]
    public void The_down_payment_stays_below_the_price_and_the_amount_must_match()
    {
        Assert.Contains("downPaymentAmount", LndLoanService.CheckGoods(Request(down: 30000, amount: 0), 0, Active).Keys);
        Assert.Contains("principalAmount", LndLoanService.CheckGoods(Request(), 26000, Active).Keys);
    }
}
