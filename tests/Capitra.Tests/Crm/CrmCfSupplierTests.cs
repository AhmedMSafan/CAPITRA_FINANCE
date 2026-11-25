using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm.CfSuppliers;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Infrastructure.Persistence.Migrations;

namespace Capitra.Tests.Crm;

/// <summary>Consumer Finance supplier onboarding: the application's rules and the CF_SUPPLIER_APPROVAL routes (Ahmed, 25 Nov 2026).</summary>
public sealed class CrmCfSupplierTests
{
    private const string Xml = CfSupplierOnboarding.SupplierApprovalBpmn;
    private static readonly DateOnly Today = new(2026, 11, 25);
    private static readonly string[] Known = ["MOBILE", "ELECTRONICS"];

    private static CrmCfSupplierApplication App() => new()
    {
        GoodsCategoryCodes = "MOBILE,ELECTRONICS", MinDownPaymentPct = 10, DownPaymentCollectorCode = "SUPPLIER", CommissionPct = 3, PaymentDays = 7,
        QuotationValidityDays = 7, DeliveryDays = 5, ValidFromDate = Today, EnteredByPerson = "EMP:100",
    };

    private static CrmClientParty Party(string type, decimal? share = null, string? screening = null) =>
        new() { PartyTypeCode = type, FullName = "X", OwnershipPct = share, ScreeningResultCode = screening };

    private static List<string> Path(string risk)
    {
        var values = new Dictionary<string, string?> { ["risk_rating"] = risk };
        var visited = new List<string>();
        var element = BpmnModel.Next(Xml, BpmnModel.StartEvent(Xml).Id, null, values).Element;
        while (element is { IsHumanTask: true } && visited.Count < 20)
        {
            visited.Add(element.StepCode!);
            element = BpmnModel.Next(Xml, element.Id, "ACCEPT", values).Element;
        }
        Assert.Equal("endEvent", element?.Type);
        return visited;
    }

    [Fact]
    public void Low_and_medium_risk_go_from_the_manager_to_the_contract()
    {
        string[] expected = [CrmCfSupplierRules.DocumentCheck, CrmCfSupplierRules.Compliance, CrmCfSupplierRules.SiteVisit, CrmCfSupplierRules.RiskReview,
            CrmCfSupplierRules.ManagerApproval, CrmCfSupplierRules.Contract];
        Assert.Equal(expected, Path("LOW"));
        Assert.Equal(expected, Path("MEDIUM"));
    }

    [Fact]
    public void High_risk_adds_the_committee_where_every_member_approves()
    {
        Assert.Equal(new[] { CrmCfSupplierRules.DocumentCheck, CrmCfSupplierRules.Compliance, CrmCfSupplierRules.SiteVisit, CrmCfSupplierRules.RiskReview,
            CrmCfSupplierRules.ManagerApproval, CrmCfSupplierRules.CommitteeApproval, CrmCfSupplierRules.Contract }, Path("HIGH"));
        Assert.Equal("ALL", BpmnModel.Element(Xml, "Task_Committee").Parallel?.Mode);
        Assert.Empty(WfeParallel.Check(Xml));
    }

    [Theory]
    [InlineData("Task_Docs")]
    [InlineData("Task_Compliance")]
    [InlineData("Task_SiteVisit")]
    [InlineData("Task_Risk")]
    [InlineData("Task_Manager")]
    [InlineData("Task_Committee")]
    [InlineData("Task_Contract")]
    public void Return_ends_the_run(string elementId) => Assert.Equal("endEvent", BpmnModel.Next(Xml, elementId, "REJECT")?.Type);

    [Theory]
    [InlineData("CF_SUPPLIER_APPROVAL", "CFS_CONTRACT", true)]
    [InlineData("cf_supplier_approval", "cfs_compliance", true)]
    [InlineData("CF_SUPPLIER_APPROVAL", "SCR_REVIEW", false)]
    [InlineData("SCR_DECISION_APPROVAL", "CFS_CONTRACT", false)]
    public void The_handler_takes_only_its_own_steps(string workflow, string step, bool expected)
    {
        var handler = new CrmCfSupplierWorkflowHandler(null!, null!, null!, null!);
        Assert.Equal(expected, handler.CanHandle(new WfeTaskContext(1, 1, workflow, step, "ACCEPT", "tester")));
    }

    [Fact]
    public void Terms_are_checked()
    {
        Assert.Empty(CrmCfSupplierRules.CheckTerms(App(), Known));
        var bad = App();
        bad.GoodsCategoryCodes = "CARS";
        bad.MinDownPaymentPct = 100;
        bad.CommissionPct = -1;
        bad.QuotationValidityDays = 0;
        bad.ValidToDate = Today;
        Assert.Equal(new[] { "goodsCategoryCodes", "minDownPaymentPct", "commissionPct", "quotationValidityDays", "validToDate" },
            CrmCfSupplierRules.CheckTerms(bad, Known).Keys);
    }

    [Fact]
    public void Submitting_needs_an_owner_a_signatory_and_shares_up_to_100_but_no_documents()
    {
        Assert.Empty(CrmCfSupplierRules.CheckSubmit([Party("OWNER", 60), Party("OWNER", 40), Party("SIGNATORY")]));
        Assert.Equal(2, CrmCfSupplierRules.CheckSubmit([Party("OWNER", 70), Party("OWNER", 40)]).Count); // no signatory, > 100%
        Assert.Single(CrmCfSupplierRules.CheckSubmit([Party("SIGNATORY")])); // no owner
    }

    [Fact]
    public void Compliance_checks_the_customer_compliance_its_risk_grade_and_every_person_screened_clear()
    {
        // the step enters nothing: it reads Customer 360 > Compliance (Ahmed, 26 Nov 2026)
        var a = App();
        Assert.Equal(3, CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER")], None, null, true, Today, "PENDING", null).Count);
        Assert.Empty(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER", screening: "CLEAR")], None, null, true, Today, "CLEAR", "HIGH"));
        Assert.Empty(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER", screening: "CLEAR")], None, null, true, Today, "DUE", "LOW"));
        Assert.Single(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER", screening: "MATCH")], None, null, true, Today, "CLEAR", "LOW"));
        // a PEP passes the screening; the customer's compliance (EDD until HIGH and verified) decides
        Assert.Empty(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER", screening: "PEP")], None, null, true, Today, "CLEAR", "HIGH"));
        Assert.Single(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Compliance, a, [Party("OWNER", screening: "PEP")], None, null, true, Today, "EDD", "LOW"));
    }

    private static readonly string[] None = [];
    private static readonly string[] Report = ["TAX_CARD", CrmCfSupplierRules.SiteVisitReport];
    private static readonly string[] Signed = [CrmCfSupplierRules.SignedAgreement];

    [Fact]
    public void Site_visit_needs_a_past_date_and_its_report()
    {
        var a = App();
        Assert.Equal(2, CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.SiteVisit, a, [], None, null, true, Today).Count);
        a.SiteVisitDate = Today.AddDays(1);
        Assert.Single(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.SiteVisit, a, [], Report, null, true, Today));
        a.SiteVisitDate = Today;
        Assert.Empty(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.SiteVisit, a, [], Report, null, true, Today));
    }

    [Fact]
    public void Contract_needs_the_signed_agreement_and_a_verified_bank_account()
    {
        var a = App();
        Assert.Equal(2, CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Contract, a, [], None, null, true, Today).Count);
        Assert.Single(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Contract, a, [], Signed, CrmClientBankAccount.Pending, true, Today));
        Assert.Empty(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.Contract, a, [], Signed, CrmClientBankAccount.Verified, true, Today));
        Assert.Single(CrmCfSupplierRules.CheckAccept(CrmCfSupplierRules.DocumentCheck, a, [], None, null, false, Today));
    }

    [Fact]
    public void The_person_who_entered_it_does_no_step()
    {
        Assert.Throws<BusinessRuleException>(() => CrmCfSupplierRules.CheckPerson("EMP:100", "EMP:100"));
        CrmCfSupplierRules.CheckPerson("EMP:100", "EMP:200");
    }

    [Theory]
    [InlineData("CF_SUPPLIER", null, "SUPPLIER", "CONSUMER_FINANCE")]
    [InlineData("factoring_supplier", null, "SUPPLIER", "FACTORING")]
    [InlineData("SUPPLIER", null, "SUPPLIER", "FACTORING")]
    [InlineData("SUPPLIER", "consumer_finance", "SUPPLIER", "CONSUMER_FINANCE")]
    [InlineData("DEBTOR", null, "DEBTOR", null)]
    public void The_two_supplier_types_are_the_supplier_role_of_their_business_line(string chosen, string? line, string role, string? expectedLine)
    {
        var (r, l) = Capitra.Application.Modules.Crm.CrmCustomer360Service.Choice(chosen, line);
        Assert.Equal(role, r);
        Assert.Equal(expectedLine, l);
    }

    [Fact]
    public void Supplier_roles_are_named_by_their_type()
    {
        Assert.Equal("Consumer Finance Supplier", Capitra.Application.Modules.Crm.CrmCustomer360Service.RoleLabel(
            new CrmClientRole { RoleCode = "SUPPLIER", BusinessLineCode = "CONSUMER_FINANCE" }));
        Assert.Equal("Factoring Supplier", Capitra.Application.Modules.Crm.CrmCustomer360Service.RoleLabel(
            new CrmClientRole { RoleCode = "SUPPLIER", BusinessLineCode = "FACTORING" }));
    }
}
