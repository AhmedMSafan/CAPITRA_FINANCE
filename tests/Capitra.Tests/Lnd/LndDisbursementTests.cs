using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Paying a retail loan out: the GL lines of the rule LND_DISBURSE and its checks.</summary>
public sealed class LndDisbursementTests
{
    private static ProductEventCharge Fee(string timing = "DEDUCT_FROM_DISBURSEMENT", string income = "IMMEDIATE") =>
        new(1, 1, "ADMIN_FEE", "Admin fee", null, "PERCENTAGE", null, 2, null, "LOAN_AMOUNT", income, null, null, 2, timing, 0);

    private static ProductEventPlan Plan(params ProductEventCharge[] charges) => new(9, "LND_DISBURSE", "LENDING", "LND_DISBURSEMENT", charges,
    [
        new(1, 1, null, null, "D", "PRINCIPAL_AMOUNT", false, "120301", "Loans receivable"),
        new(1, 2, null, null, "C", "NET_PAYOUT_AMOUNT", true, null, "Paid out"),
        new(1, 1, 1, "ADMIN_FEE", "C", "CHARGE_IMMEDIATE", false, "410301", "Fee income"),
        new(1, 2, 1, "ADMIN_FEE", "C", "CHARGE_DEFERRED", false, "230301", "Unearned fee income"),
    ]);

    private static List<(ProductEventCharge, ProductEventChargeAmount)> Amounts(ProductEventPlan p, decimal principal) =>
        [.. p.Charges.Select(c => (c, ProductEventPosting.Calculate(c, code => code == "LOAN_AMOUNT" ? principal : null, 0)))];

    [Fact]
    public void A_fee_earned_at_once_is_taken_from_the_payout()
    {
        var p = Plan(Fee());
        var a = Amounts(p, 50_000);
        Assert.Null(LndDisbursementService.Check(p, 50_000, a));
        var lines = LndDisbursementService.Lines(p, 50_000, 49_000, a, "110102");
        Assert.Equal(3, lines.Count);
        Assert.Equal(50_000m, lines.Single(x => x.Account == "120301").Dr);
        Assert.Equal(49_000m, lines.Single(x => x.Account == "110102").Cr);
        Assert.Equal(1_000m, lines.Single(x => x.Account == "410301").Cr);
        Assert.Equal(lines.Sum(x => x.Dr), lines.Sum(x => x.Cr));
    }

    [Fact]
    public void A_pro_rated_fee_goes_to_unearned_income()
    {
        var p = Plan(Fee(income: "PRORATE"));
        var lines = LndDisbursementService.Lines(p, 50_000, 49_000, Amounts(p, 50_000), "110102");
        Assert.Equal(1_000m, lines.Single(x => x.Account == "230301").Cr);
        Assert.DoesNotContain(lines, x => x.Account == "410301");
    }

    [Fact]
    public void Only_charges_deducted_from_the_payout_are_accepted()
    {
        var p = Plan(Fee(timing: "DUE_SEPARATELY"));
        Assert.Contains("not deducted", LndDisbursementService.Check(p, 50_000, Amounts(p, 50_000)), StringComparison.Ordinal);
    }

    [Fact]
    public void A_rule_without_the_bank_line_or_with_other_figures_is_refused()
    {
        var noBank = new ProductEventPlan(9, "LND_DISBURSE", "LENDING", "LND_DISBURSEMENT", [], [new(1, 1, null, null, "D", "PRINCIPAL_AMOUNT", false, "120301", null)]);
        Assert.Contains("NET_PAYOUT_AMOUNT", LndDisbursementService.Check(noBank, 50_000, []), StringComparison.Ordinal);
        var other = new ProductEventPlan(9, "LND_DISBURSE", "LENDING", "LND_DISBURSEMENT", [], [new(1, 1, null, null, "D", "FACE_VALUE", false, "120301", null)]);
        Assert.Contains("FACE_VALUE", LndDisbursementService.Check(other, 50_000, []), StringComparison.Ordinal);
    }

    [Fact]
    public void Charges_cannot_take_the_whole_loan()
    {
        var p = Plan(Fee() with { ChargeRate = 100 });
        Assert.Contains("nothing would be paid", LndDisbursementService.Check(p, 50_000, Amounts(p, 50_000)), StringComparison.Ordinal);
    }
}
