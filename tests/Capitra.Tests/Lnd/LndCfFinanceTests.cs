using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>CF_FINANCE: a consumer finance from a supplier (Ahmed, 26 Nov 2026).</summary>
public sealed class LndCfFinanceTests
{
    [Fact]
    public void Credit_approval_needs_clear_compliance_and_an_active_supplier()
    {
        Assert.Empty(LndCfFinanceRules.CheckCredit("CLEAR", "Clear", supplierActive: true));
        Assert.Empty(LndCfFinanceRules.CheckCredit("DUE", "Review due", supplierActive: true)); // due for review soon still counts as clear
        Assert.Equal(2, LndCfFinanceRules.CheckCredit("PENDING", "Pending", supplierActive: false).Count);
        Assert.Single(LndCfFinanceRules.CheckCredit("EDD", "Enhanced due diligence", supplierActive: true));
    }

    [Fact]
    public void The_person_who_entered_the_finance_does_none_of_its_steps()
    {
        Assert.Throws<BusinessRuleException>(() => LndCfFinanceRules.CheckPerson("E123", "E123"));
        LndCfFinanceRules.CheckPerson("E123", "E456");
        LndCfFinanceRules.CheckPerson(null, "E456"); // a finance entered before the workflow has no person
    }

    [Fact]
    public void Steps_have_names_and_only_credit_approval_is_ready_now()
    {
        Assert.Equal("Credit approval", LndCfFinanceRules.StepName("cf_credit"));
        Assert.Equal("Supplier payment", LndCfFinanceRules.StepName(LndCfFinanceRules.SupplierPayment));
        Assert.Equal([LndCfFinanceRules.Credit], LndCfFinanceRules.Ready);
    }

    // an administration fee of 1% of the financed amount (upfront by default) and a fixed 50 stamp fee with 10% tax
    private static readonly ProductEventCharge AdminFee = new(1, 10, "CF_ADMIN_FEE", "Administration fee", null, "PERCENTAGE", null, 1m, null,
        "FINANCED_AMOUNT", "PRORATE", null, null, 2, "PAID_UPFRONT", 0m);
    private static readonly ProductEventCharge Stamp = new(2, 20, "STAMP", "Stamp fee", null, "FIXED", 50m, null, null, null, "IMMEDIATE", null, null, 2,
        "PAID_UPFRONT", 10m);

    [Fact]
    public void Charges_are_paid_upfront_by_default_and_the_loan_is_the_price_less_the_down_payment()
    {
        // price 20,000, down 4,000: financed 16,000; fee 1% = 160; stamp 50 + 5 tax = 55; all upfront = 215
        var r = LndCfCharges.Work([AdminFee, Stamp], 20000m, 4000m, null);
        Assert.Null(r.Problem);
        Assert.Equal(16000m, r.LoanAmount);
        Assert.Equal(215m, r.PaidUpfront);
        Assert.Equal(0m, r.AddedToFinance);
    }

    [Fact]
    public void A_charge_added_to_the_finance_raises_the_loan_amount()
    {
        // the fee 160 added to the finance: loan 16,160; the stamp 55 is still upfront
        var r = LndCfCharges.Work([AdminFee, Stamp], 20000m, 4000m, new Dictionary<string, string> { ["CF_ADMIN_FEE"] = LndCfCharges.Financed });
        Assert.Equal(16160m, r.LoanAmount);
        Assert.Equal(160m, r.AddedToFinance);
        Assert.Equal(55m, r.PaidUpfront);
    }

    [Fact]
    public void A_charge_on_the_goods_price_uses_the_price()
    {
        var onPrice = AdminFee with { AmountBaseCode = "GOODS_PRICE" };
        Assert.Equal(200m, LndCfCharges.Work([onPrice], 20000m, 4000m, null).Charges[0].Amount); // 1% of 20,000
    }
}
