using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Receipt reversal with replay: the later receipts applied again, and the GL of the change.</summary>
public sealed class LndReceiptReversalTests
{
    private static readonly IReadOnlyList<string> Order = ["LATE_FEE", "CHARGES", "INSURANCE", "INTEREST", "PRINCIPAL"];

    private static List<LndInstallment> Schedule() =>
    [
        new() { InstallmentId = 1, InstallmentNo = 1, DueDate = new(2026, 2, 1), PrincipalDueAmount = 900, InterestDueAmount = 100, StatusCode = "DUE" },
        new() { InstallmentId = 2, InstallmentNo = 2, DueDate = new(2026, 3, 1), PrincipalDueAmount = 950, InterestDueAmount = 50, StatusCode = "DUE" },
    ];

    private static Dictionary<string, decimal> Paid(decimal interest = 0, decimal principal = 0) => new()
    {
        ["LATE_FEE"] = 0, ["CHARGES"] = 0, ["INSURANCE"] = 0, ["INTEREST"] = interest, ["PRINCIPAL"] = principal,
    };

    [Fact]
    public void Without_the_reversed_receipt_a_later_one_pays_the_older_instalment_first()
    {
        // first receipt (1,000 on 1 Feb) is reversed; the later one (1,000 on 1 Mar) had paid instalment 2 and is applied again
        var s = Schedule();
        var r = LndReplay.Run(s, Order, [new LndReplay.Receipt(20, new(2026, 3, 1), 1000, Paid(50, 950))]).Single();
        Assert.Equal(1000m, r.Allocated);
        Assert.Equal(0m, r.Left);
        Assert.Equal("PAID", s[0].StatusCode);
        Assert.Equal(new DateOnly(2026, 3, 1), s[0].PaidDate);
        Assert.Equal("DUE", s[1].StatusCode);
        Assert.Equal(50m, r.Delta["INTEREST"]);
        Assert.Equal(-50m, r.Delta["PRINCIPAL"]);
        Assert.True(r.Changed);
    }

    [Fact]
    public void A_later_receipt_that_kept_credit_now_applies_more()
    {
        var s = Schedule();
        // the later receipt of 1,500 had applied 1,000 (instalment 2) and kept 500
        var r = LndReplay.Run(s, Order, [new LndReplay.Receipt(20, new(2026, 3, 1), 1500, Paid(50, 950))]).Single();
        Assert.Equal(1500m, r.Allocated);
        Assert.Equal(0m, r.Left);
        Assert.Equal(500m, r.Delta.Values.Sum());
        Assert.Equal("PART_PAID", s[1].StatusCode);
    }

    [Fact]
    public void Undoing_a_payment_puts_the_instalment_back_to_due()
    {
        var i = Schedule()[0];
        LndReplay.Pay(i, "INTEREST", 100);
        LndReplay.Pay(i, "PRINCIPAL", 900);
        LndReplay.SetStatus(i, new(2026, 2, 1));
        Assert.Equal("PAID", i.StatusCode);
        LndReplay.Pay(i, "PRINCIPAL", -900);
        LndReplay.Pay(i, "INTEREST", -100);
        LndReplay.SetStatus(i, null);
        Assert.Equal("DUE", i.StatusCode);
        Assert.Null(i.PaidDate);
    }

    [Fact]
    public void The_replay_gl_moves_customer_credit_to_the_parts_paid_and_nets_shifts()
    {
        var plan = new ProductEventPlan(5, "LND_RECEIPT", "LENDING", "LND_RECEIPT", [],
        [
            new(1, 1, null, null, "D", "BANK_RECEIPT_AMOUNT", true, null, "Bank"),
            new(1, 2, null, null, "D", "CREDIT_USED_AMOUNT", false, "230101", "Customer credit"),
            new(1, 3, null, null, "C", "INTEREST_PAID", false, "120501", "Interest receivable"),
            new(1, 4, null, null, "C", "PRINCIPAL_PAID", false, "120101", "Loans receivable"),
            new(1, 5, null, null, "C", "CREDIT_KEPT_AMOUNT", false, "230101", "Customer credit"),
        ]);
        var lines = LndReceiptReversalService.AdjustmentLines(plan, Paid(50, 450));
        Assert.Equal(500m, lines.Single(x => x.Account == "230101").Dr);
        Assert.Equal(50m, lines.Single(x => x.Account == "120501").Cr);
        Assert.Equal(450m, lines.Single(x => x.Account == "120101").Cr);

        // interest +50, principal −50: no credit moves
        var shift = LndReceiptReversalService.AdjustmentLines(plan, Paid(50, -50));
        Assert.DoesNotContain(shift, x => x.Account == "230101");
        Assert.Equal(50m, shift.Single(x => x.Account == "120501").Cr);
        Assert.Equal(50m, shift.Single(x => x.Account == "120101").Dr);
    }

    [Fact]
    public void Suspense_follows_what_is_owed_again_after_a_reversal()
    {
        var loan = new LndLoan { StatusCode = "ACTIVE", DisbursementDate = new(2026, 1, 1), LateFeeTypeCode = "NONE", IsNonAccrual = true, InterestSuspenseAmount = 50 };
        var s = Schedule();
        foreach (var i in s) i.InterestAccruedAmount = i.InterestDueAmount;
        s[0].InterestPaidAmount = 100; // paid then: suspense kept only instalment 2's 50
        s[0].InterestPaidAmount = 0;   // the receipt is reversed: 150 unpaid again
        var r = LndEod.Process(loan, s, new(2026, 3, 10), 30, 180);
        Assert.Equal(100m, r.InterestToSuspense);
        Assert.Equal(150m, loan.InterestSuspenseAmount);
    }
}
