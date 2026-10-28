using Capitra.Application.Modules.Adm.Products;

namespace Capitra.Tests.Adm;

/// <summary>The PP_DISBURSE rule of scripts/setup-pp-disburse-rule.sql posts the same entries as the fixed disbursement GL.</summary>
public sealed class ProductEventPostingTests
{
    private static ProductEventGlLine Rule(int j, int no, string dc, string amount, string? account, string description) =>
        new(j, no, null, null, dc, amount, account is null, account, description);

    private static IEnumerable<ProductEventGlLine> Charge(int seq, string code) =>
    [
        new(1, 40, seq, code, "C", "CHARGE_DEFERRED", false, "120404", "Unearned income - fees (pro-rated)"),
        new(1, 60, seq, code, "C", "CHARGE_IMMEDIATE", false, "450103", "Fee income - recognised on disbursement"),
    ];

    private static readonly ProductEventPlan Plan = new(1, "PP_DISBURSE", "PP_DISBURSE", ["ADMIN_EXPENSE", "LEGAL_FEE", "VALUATION_FEE"],
    [
        Rule(1, 10, "D", "FACE_VALUE", "120401", "Investor cheques - face value"),
        Rule(1, 20, "C", "DISCOUNT", "120402", "Unearned income - funding recovery"),
        Rule(1, 30, "C", "MARGIN_DEFERRED", "120403", "Unearned income - margin (pro-rated)"),
        Rule(1, 50, "C", "MARGIN_IMMEDIATE", "450102", "Margin income - recognised on disbursement"),
        Rule(1, 70, "C", "PURCHASE_PRICE", "230103", "Payable to developer - purchase price"),
        Rule(2, 10, "D", "PURCHASE_PRICE", "230103", "Payable to developer - settled"),
        Rule(2, 20, "C", "PURCHASE_PRICE", null, "Payment to developer"),
        .. Charge(10, "ADMIN_EXPENSE"), .. Charge(20, "LEGAL_FEE"), .. Charge(30, "VALUATION_FEE"),
    ]);

    [Fact]
    public void Disbursement_rule_posts_the_fixed_entries()
    {
        // face 1,000, discount 100, margin 50 pro-rated, admin 10 pro-rated, legal 5 and valuation 3 at once: price 832
        var charges = new Dictionary<string, (decimal Amount, bool Prorate)>
        {
            ["ADMIN_EXPENSE"] = (10, true), ["LEGAL_FEE"] = (5, false), ["VALUATION_FEE"] = (3, false),
        };
        var figures = new Dictionary<string, decimal>
        {
            ["FACE_VALUE"] = 1000, ["DISCOUNT"] = 100, ["MARGIN_DEFERRED"] = 50, ["MARGIN_IMMEDIATE"] = 0, ["PURCHASE_PRICE"] = 832,
        };
        decimal AmountOf(ProductEventGlLine l) => l.ChargeCode is null ? figures[l.AmountCode]
            : l.AmountCode == "CHARGE_DEFERRED" ? (charges[l.ChargeCode].Prorate ? charges[l.ChargeCode].Amount : 0)
            : charges[l.ChargeCode].Prorate ? 0 : charges[l.ChargeCode].Amount;

        var lines = ProductEventPosting.Build(Plan, AmountOf, "110104");

        Assert.Equal(
        [
            new ProductEventPostingLine(1, "120401", 1000, 0, "Investor cheques - face value"),
            new ProductEventPostingLine(1, "120402", 0, 100, "Unearned income - funding recovery"),
            new ProductEventPostingLine(1, "120403", 0, 50, "Unearned income - margin (pro-rated)"),
            new ProductEventPostingLine(1, "120404", 0, 10, "Unearned income - fees (pro-rated)"),
            new ProductEventPostingLine(1, "450103", 0, 8, "Fee income - recognised on disbursement"),
            new ProductEventPostingLine(1, "230103", 0, 832, "Payable to developer - purchase price"),
            new ProductEventPostingLine(2, "230103", 832, 0, "Payable to developer - settled"),
            new ProductEventPostingLine(2, "110104", 0, 832, "Payment to developer"),
        ], lines);
        Assert.Equal(lines.Where(x => x.JournalNo == 1).Sum(x => x.Dr), lines.Where(x => x.JournalNo == 1).Sum(x => x.Cr));
    }
}
