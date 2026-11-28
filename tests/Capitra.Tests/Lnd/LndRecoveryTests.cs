using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Money received on a written-off retail loan: recovery income up to what was written off, the rest kept as credit.</summary>
public sealed class LndRecoveryTests
{
    [Fact]
    public void A_recovery_is_income_up_to_what_is_left_and_the_rest_is_credit()
    {
        Assert.Equal((500m, 0m), LndReceiptService.SplitRecovery(500, 2560, 0));
        Assert.Equal((60m, 440m), LndReceiptService.SplitRecovery(500, 2560, 2500));
        Assert.Equal((0m, 500m), LndReceiptService.SplitRecovery(500, 2560, 2560));
    }

    private static ProductEventPlan Plan(IReadOnlyList<ProductEventGlLine> lines) => new(7, "LND_RECOVERY", "LENDING", "LND_RECOVERY", [], lines);

    [Fact]
    public void Gl_lines_take_the_bank_the_recovery_and_the_credit_kept()
    {
        var plan = Plan(
        [
            new(1, 1, null, null, "D", "BANK_RECEIPT_AMOUNT", true, null, "Bank"),
            new(1, 2, null, null, "C", "RECOVERY_AMOUNT", false, "490101", "Recovery income"),
            new(1, 3, null, null, "C", "CREDIT_KEPT_AMOUNT", false, "230101", "Customer credit"),
        ]);
        var lines = LndReceiptService.Lines(plan, new Dictionary<string, decimal>(), [("110201", 500m)], 0, 440, 60);
        Assert.Equal(500m, lines.Single(x => x.Account == "110201").Dr);
        Assert.Equal(60m, lines.Single(x => x.Account == "490101").Cr);
        Assert.Equal(440m, lines.Single(x => x.Account == "230101").Cr);
    }

    [Fact]
    public void A_recovery_rule_cannot_post_instalment_parts()
    {
        var plan = Plan(
        [
            new(1, 1, null, null, "D", "BANK_RECEIPT_AMOUNT", true, null, "Bank"),
            new(1, 2, null, null, "C", "PRINCIPAL_PAID", false, "120101", "Loans receivable"),
        ]);
        Assert.Throws<BusinessRuleException>(() => LndReceiptService.Lines(plan, new Dictionary<string, decimal>(), [("110201", 500m)], 0, 0, 500));
    }
}
