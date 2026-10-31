using Capitra.Application.Modules.Wfe;

namespace Capitra.Tests.Workflow;

/// <summary>The App 105 workflow routes exactly as the APEX WFE transitions of PURCHASED_PORTFOLIO_WF.</summary>
public sealed class PurchasedPortfolioBpmnTests
{
    private static readonly string Xml = File.ReadAllText(Path.Combine(RepoRoot(), "tools", "oracle-port", "workflows", "PURCHASED_PORTFOLIO_WF.bpmn"));

    [Fact]
    public void Happy_path_visits_every_step_in_apex_order()
    {
        var actions = new Dictionary<string, string>
        {
            ["LOAD_DATA"] = "COMPLETE", ["AML_REVIEW"] = "APPROVE", ["PORTFOLIO_INQUIRY"] = "APPROVE", ["LEGAL_REVIEW"] = "APPROVE",
            ["VALUATION_REVIEW"] = "COMPLETE", ["COVERAGE_REVIEW"] = "COMPLETE", ["CHEQUE_COVERAGE_REVIEW"] = "APPROVE",
            ["FINANCIAL_REVIEW"] = "COMPLETE", ["BOARD_REVIEW"] = "APPROVE", ["DISBURSEMENT"] = "COMPLETE",
        };
        var visited = new List<string>();
        var element = BpmnModel.Next(Xml, BpmnModel.StartEvent(Xml).Id);
        while (element is { Type: "userTask" })
        {
            visited.Add(element.StepCode!);
            element = BpmnModel.Next(Xml, element.Id, actions[element.StepCode!]);
        }

        Assert.Equal("endEvent", element?.Type);
        Assert.Equal(actions.Keys, visited);
    }

    [Theory]
    [InlineData("AML_REVIEW")]
    [InlineData("PORTFOLIO_INQUIRY")]
    [InlineData("LEGAL_REVIEW")]
    [InlineData("CHEQUE_COVERAGE_REVIEW")]
    [InlineData("BOARD_REVIEW")]
    public void Reject_ends_the_workflow(string stepCode) =>
        Assert.Equal("endEvent", BpmnModel.Next(Xml, "Task_" + stepCode, "REJECT")?.Type);

    [Fact]
    public void Steps_carry_the_apex_role_screen_and_actions()
    {
        var aml = BpmnModel.Element(Xml, "Task_AML_REVIEW");
        Assert.Equal("CREDIT_OFFICER", aml.Assignee);
        Assert.Equal("mtg-209", aml.Route);
        Assert.Equal("CLAIM,START,APPROVE,REJECT", aml.Actions);
        Assert.Equal("CLAIM,START,COMPLETE", BpmnModel.Element(Xml, "Task_COVERAGE_REVIEW").Actions);
    }

    [Fact]
    public void A_step_carries_its_product_event_rule_code()
    {
        var xml = Xml.Replace("finova:stepCode=\"DISBURSEMENT\"", "finova:stepCode=\"DISBURSEMENT\" finova:ruleCode=\"PP_DISBURSE\"");
        Assert.Equal("PP_DISBURSE", BpmnModel.Element(xml, "Task_DISBURSEMENT").RuleCode);
        Assert.Null(BpmnModel.Element(xml, "Task_AML_REVIEW").RuleCode);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Capitra.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
