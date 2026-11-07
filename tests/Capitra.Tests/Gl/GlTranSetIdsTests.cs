using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Gl.Journals;

namespace Capitra.Tests.Gl;

/// <summary>GL transaction set IDs read CAP-{module}-{year}-{number of at least 8 digits}.</summary>
public sealed class GlTranSetIdsTests
{
    [Fact]
    public void Format_pads_the_number_to_eight_digits() =>
        Assert.Equal("CAP-CHW-2026-00000001", GlTranSetIds.Format("CHW", 2026, 1, 8));

    [Fact]
    public void Format_keeps_more_digits_when_set_or_needed()
    {
        Assert.Equal("CAP-MPA-2027-0000000042", GlTranSetIds.Format("MPA", 2027, 42, 10));
        Assert.Equal("CAP-FCT-2026-123456789", GlTranSetIds.Format("FCT", 2026, 123456789, 8));
        Assert.Equal("CAP-GLM-2026-00000007", GlTranSetIds.Format("GLM", 2026, 7, 5));
    }

    [Fact]
    public void Cheque_batch_submission_rule_posts_one_debit_and_one_credit_of_the_cheque_total()
    {
        var plan = new ProductEventPlan(2, "CHW_CHEQUES_RECEIVED", "FACTORING", "CHW_RECEIVED", [],
        [
            new(1, 10, null, null, "D", "CHEQUE_AMOUNT", false, "910101", "Cheques received in custody"),
            new(1, 20, null, null, "C", "CHEQUE_AMOUNT", false, "910102", "Clients' cheques in custody"),
        ]);
        var lines = ProductEventPosting.Build(plan, l => l.AmountCode == "CHEQUE_AMOUNT" ? 80000m : 0, null);
        Assert.Collection(lines,
            l => Assert.Equal(("910101", 80000m, 0m), (l.Account, l.Dr, l.Cr)),
            l => Assert.Equal(("910102", 0m, 80000m), (l.Account, l.Dr, l.Cr)));
        Assert.Contains(ProductActions.All, a => a.ActionCode == ProductActions.ChequeBatchSubmit && a.ModuleCode == "CHW" && a.ProductCode == "CHEQUE_WALLET");
    }
}
