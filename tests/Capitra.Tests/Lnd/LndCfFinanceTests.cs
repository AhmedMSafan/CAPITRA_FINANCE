using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
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
    public void Steps_have_names_and_the_first_three_are_ready_now()
    {
        Assert.Equal("Credit approval", LndCfFinanceRules.StepName("cf_credit"));
        Assert.Equal("Charges and claim", LndCfFinanceRules.StepName(LndCfFinanceRules.Charges));
        Assert.Equal("Schedule and contract", LndCfFinanceRules.StepName(LndCfFinanceRules.Schedule));
        Assert.Equal("Supplier payment", LndCfFinanceRules.StepName(LndCfFinanceRules.SupplierPayment));
        Assert.Equal([LndCfFinanceRules.Credit, LndCfFinanceRules.Charges, LndCfFinanceRules.Schedule], LndCfFinanceRules.Ready);
    }

    // an administration fee of 1% of the financed amount (upfront by default) and a fixed 50 stamp fee with 10% tax
    private static readonly ProductEventCharge AdminFee = new(1, 10, "CF_ADMIN_FEE", "Administration fee", null, "PERCENTAGE", null, 1m, null,
        "FINANCED_AMOUNT", "IMMEDIATE", null, null, 2, "PAID_UPFRONT", 0m);
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
        Assert.All(r.Charges, c => Assert.False(c.IsOverridden));
    }

    [Fact]
    public void A_charge_added_to_the_finance_raises_the_loan_amount()
    {
        // the fee 160 added to the finance: loan 16,160; the stamp 55 is still upfront
        var r = LndCfCharges.Work([AdminFee, Stamp], 20000m, 4000m, new Dictionary<string, string> { ["CF_ADMIN_FEE"] = LndCfCharges.Financed });
        Assert.Equal(16160m, r.LoanAmount);
        Assert.Equal(160m, r.AddedToFinance);
        Assert.Equal(55m, r.PaidUpfront);
        Assert.True(r.Charges.Single(x => x.ChargeCode == "CF_ADMIN_FEE").IsOverridden); // Credit changed the rule's default
        Assert.False(r.Charges.Single(x => x.ChargeCode == "STAMP").IsOverridden);
    }

    [Fact]
    public void A_charge_on_the_goods_price_uses_the_price()
    {
        var onPrice = AdminFee with { AmountBaseCode = "GOODS_PRICE" };
        Assert.Equal(200m, LndCfCharges.Work([onPrice], 20000m, 4000m, null).Charges[0].Amount); // 1% of 20,000
    }

    [Fact]
    public void The_claim_is_the_upfront_charges_and_the_down_payment_we_take()
    {
        LndLoanCharge[] charges =
        [
            new() { ChargeCode = "CF_ADMIN_FEE", ChargeAmount = 160m, TaxAmount = 0m, TimingCode = LndCfCharges.Upfront },
            new() { ChargeCode = "STAMP", ChargeAmount = 50m, TaxAmount = 5m, TimingCode = LndCfCharges.Financed }, // part of the principal
        ];
        Assert.Equal((160m, 4000m), LndCfFinanceRules.ClaimAmounts(charges, "COMPANY", 4000m));
        Assert.Equal((160m, 0m), LndCfFinanceRules.ClaimAmounts(charges, "SUPPLIER", 4000m)); // the supplier takes the down payment
    }

    [Fact]
    public void Charges_and_claim_is_accepted_once_the_claim_is_paid_or_nothing_is_upfront()
    {
        Assert.Null(LndCfFinanceRules.CheckClaim(LndCfFinanceRules.Claim.Paid, 4160m));
        Assert.Null(LndCfFinanceRules.CheckClaim(LndCfFinanceRules.Claim.None, 0m));
        Assert.Null(LndCfFinanceRules.CheckClaim(null, 0m)); // nothing upfront: no claim needed
        Assert.NotNull(LndCfFinanceRules.CheckClaim(null, 160m));
        Assert.NotNull(LndCfFinanceRules.CheckClaim(LndCfFinanceRules.Claim.Open, 160m));
    }
}
