using Capitra.Application.Modules.Wfe;
using Capitra.Infrastructure.Persistence.Migrations;

namespace Capitra.Tests.Workflow;

/// <summary>FACTORING_LIMIT_APPROVAL routes as in FINOVA_FINANCEING, with Committee Approval REJECT ending the workflow.</summary>
public sealed class FactoringBpmnTests
{
    private const string Xml = SeedFactoringWorkflow.FactoringBpmn;

    private static readonly string[] Steps =
    [
        "OPERATION_RECEIVED", "SALES_REVIEW", "INVESTIGATION", "CREDIT_ANALYSIS", "RISK_REVIEW", "COMMITTEE_APPROVAL",
        "CLIENT_ACCEPTANCE", "GENERATE_CONTRACT", "LEGAL_REVIEW",
    ];

    [Fact]
    public void Accept_path_visits_every_step_in_order()
    {
        var visited = new List<string>();
        var element = BpmnModel.Next(Xml, BpmnModel.StartEvent(Xml).Id);
        while (element is { IsHumanTask: true } && visited.Count < 20)
        {
            visited.Add(element.StepCode!);
            element = BpmnModel.Next(Xml, element.Id, "ACCEPT");
        }

        Assert.Equal("endEvent", element?.Type);
        Assert.Equal(Steps.Length, visited.Count);
    }

    [Theory]
    [InlineData("Activity_0btcy1n")] // Risk Review
    [InlineData("Activity_0pogexl")] // Committee Approval
    public void Reject_ends_the_workflow(string elementId) =>
        Assert.Equal("endEvent", BpmnModel.Next(Xml, elementId, "REJECT")?.Type);

    [Fact]
    public void Committee_accept_goes_to_client_acceptance_and_client_reject_back_to_committee()
    {
        Assert.Equal("Activity_10lu0rl", BpmnModel.Next(Xml, "Activity_0pogexl", "ACCEPT")?.Id);
        Assert.Equal("Activity_0pogexl", BpmnModel.Next(Xml, "Activity_10lu0rl", "REJECT")?.Id);
        Assert.Equal("CLAIM,ACCEPT,REJECT,COMPLETE", BpmnModel.Element(Xml, "Activity_0pogexl").Actions);
    }
}
