using Capitra.Application.Common.Exceptions;
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
}
