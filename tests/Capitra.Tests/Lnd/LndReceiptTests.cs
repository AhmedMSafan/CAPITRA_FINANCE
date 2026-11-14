using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Customer payments of a retail loan: the allocation engine and the GL lines of the rule LND_RECEIPT.</summary>
public sealed class LndReceiptTests
{
    private static readonly string[] Order = ["LATE_FEE", "CHARGES", "INSURANCE", "INTEREST", "PRINCIPAL"];

    private static LndInstallmentOwed I(int no, DateOnly due, decimal late, decimal charges, decimal insurance, decimal interest, decimal principal) =>
        new(no * 10, no, due, new Dictionary<string, decimal>
        {
            ["LATE_FEE"] = late, ["CHARGES"] = charges, ["INSURANCE"] = insurance, ["INTEREST"] = interest, ["PRINCIPAL"] = principal,
        });

    [Fact]
    public void A_partial_payment_follows_the_agreed_order()
    {
        // the deck example: 3,000 against 42 late fee, 58 charges, 200 insurance, 2,000 interest, 4,800 principal
        var (lines, left) = LndAllocator.Allocate([I(1, new(2026, 11, 1), 42, 58, 200, 2_000, 4_800)], Order, 3_000, new(2026, 11, 12));
        Assert.Equal([("LATE_FEE", 42m), ("CHARGES", 58m), ("INSURANCE", 200m), ("INTEREST", 2_000m), ("PRINCIPAL", 700m)], lines.Select(x => (x.PartCode, x.Amount)));
        Assert.Equal(0m, left);
    }

    [Fact]
    public void The_oldest_instalment_is_cleared_first()
    {
        var (lines, _) = LndAllocator.Allocate(
            [I(2, new(2026, 11, 1), 0, 0, 0, 900, 4_000), I(1, new(2026, 10, 1), 10, 0, 0, 1_000, 3_900)], Order, 5_000, new(2026, 11, 12));
        Assert.Equal(4_910m, lines.Where(x => x.InstallmentNo == 1).Sum(x => x.Amount));
        Assert.Equal([("INTEREST", 90m)], lines.Where(x => x.InstallmentNo == 2).Select(x => (x.PartCode, x.Amount)));
    }

    [Fact]
    public void Money_beyond_what_is_due_on_the_value_date_stays_as_credit()
    {
        var (lines, left) = LndAllocator.Allocate(
            [I(1, new(2026, 11, 1), 0, 0, 0, 100, 900), I(2, new(2026, 12, 1), 0, 0, 0, 90, 910)], Order, 1_500, new(2026, 11, 12));
        Assert.Equal(1_000m, lines.Sum(x => x.Amount));
        Assert.DoesNotContain(lines, x => x.InstallmentNo == 2);
        Assert.Equal(500m, left);
    }

    [Fact]
    public void A_different_order_on_the_loan_is_respected()
    {
        var (lines, _) = LndAllocator.Allocate([I(1, new(2026, 11, 1), 50, 0, 0, 100, 900)], ["PRINCIPAL", "INTEREST", "LATE_FEE", "CHARGES", "INSURANCE"], 950, new(2026, 11, 1));
        Assert.Equal([("PRINCIPAL", 900m), ("INTEREST", 50m)], lines.Select(x => (x.PartCode, x.Amount)));
    }

    private static ProductEventPlan Plan(IReadOnlyList<ProductEventGlLine> lines) => new(9, "LND_RECEIPT", "LENDING", "LND_RECEIPT", [], lines);

    private static readonly ProductEventPlan Standard = Plan(
    [
        new(1, 1, null, null, "D", "BANK_RECEIPT_AMOUNT", true, null, "Received"),
        new(1, 2, null, null, "D", "CREDIT_USED_AMOUNT", false, "220501", "Credit used"),
        new(1, 3, null, null, "C", "LATE_FEE_PAID", false, "120502", "Late fee"),
        new(1, 4, null, null, "C", "INTEREST_PAID", false, "120402", "Interest receivable"),
        new(1, 5, null, null, "C", "PRINCIPAL_PAID", false, "120301", "Loans receivable"),
        new(1, 6, null, null, "C", "CREDIT_KEPT_AMOUNT", false, "220501", "Credit kept"),
    ]);

    [Fact]
    public void A_receipt_posts_each_bank_account_and_each_part()
    {
        var paid = new Dictionary<string, decimal> { ["INTEREST"] = 2_000, ["PRINCIPAL"] = 1_000 };
        var lines = LndReceiptService.Lines(Standard, paid, [("110102", 2_500m), ("110103", 1_000m)], 0, 500);
        Assert.Equal(2_500m, lines.Single(x => x.Account == "110102").Dr);
        Assert.Equal(1_000m, lines.Single(x => x.Account == "110103").Dr);
        Assert.Equal(2_000m, lines.Single(x => x.Account == "120402").Cr);
        Assert.Equal(1_000m, lines.Single(x => x.Account == "120301").Cr);
        Assert.Equal(500m, lines.Single(x => x.Account == "220501").Cr);
        Assert.Equal(lines.Sum(x => x.Dr), lines.Sum(x => x.Cr));
    }

    [Fact]
    public void Credit_used_is_debited_from_the_customer_credit_account()
    {
        var lines = LndReceiptService.Lines(Standard, new Dictionary<string, decimal> { ["PRINCIPAL"] = 300 }, [], 300, 0);
        Assert.Equal(300m, lines.Single(x => x.Account == "220501").Dr);
        Assert.Equal(300m, lines.Single(x => x.Account == "120301").Cr);
    }

    [Fact]
    public void A_rule_that_misses_a_part_paid_does_not_balance_and_is_refused()
    {
        var noInterest = Plan([new(1, 1, null, null, "D", "BANK_RECEIPT_AMOUNT", true, null, null), new(1, 2, null, null, "C", "PRINCIPAL_PAID", false, "120301", null)]);
        Assert.Throws<BusinessRuleException>(() =>
            LndReceiptService.Lines(noInterest, new Dictionary<string, decimal> { ["INTEREST"] = 100, ["PRINCIPAL"] = 400 }, [("110102", 500m)], 0, 0));
        var wrong = Plan([new(1, 1, null, null, "D", "FACE_VALUE", false, "1", null)]);
        Assert.Throws<BusinessRuleException>(() => LndReceiptService.Lines(wrong, new Dictionary<string, decimal>(), [], 0, 0));
    }
}
