using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>
/// Rules on data fields: a path can carry conditions (AMOUNT &gt; 1000000) and is taken only when they hold; the path without
/// conditions takes every other case. Diagrams without conditions route as before.
/// </summary>
public sealed class WorkflowRulesTests
{
    private const string Cfo = """[{"field":"AMOUNT","op":">","value":"1000000"}]""";

    private static string Xml(string cfoConditions, string otherConditions = "") =>
        $$"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P" finova:variables='[{"name":"AMOUNT","type":"Decimal"},{"name":"CURRENCY","type":"String"}]'><bpmn:startEvent id="S"/><bpmn:userTask id="T" name="Credit review" finova:stepCode="REVIEW" finova:assignmentType="User" finova:assignee="ahmed" finova:actions="APPROVE,REJECT"/><bpmn:exclusiveGateway id="G" name="Amount check"/><bpmn:userTask id="C" name="CFO approval" finova:stepCode="CFO" finova:assignmentType="User" finova:assignee="cfo" finova:actions="APPROVE"/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="T"/><bpmn:sequenceFlow id="F2" name="APPROVE" sourceRef="T" targetRef="G"/><bpmn:sequenceFlow id="F3" name="REJECT" sourceRef="T" targetRef="E"/><bpmn:sequenceFlow id="F4" sourceRef="G" targetRef="C" finova:conditions='{{cfoConditions}}'/><bpmn:sequenceFlow id="F5" sourceRef="G" targetRef="E" {{otherConditions}}/><bpmn:sequenceFlow id="F6" sourceRef="C" targetRef="E"/></bpmn:process></bpmn:definitions>""";

    private sealed class Clock : IClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 11, 10, 8, 0, 0, DateTimeKind.Utc);
    }

    private sealed class User(string name) : ICurrentUser
    {
        public string? UserId => name;
        public string? UserName => name;
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles => [];
    }

    private static (CapitraDbContext Db, WfeRuntime Runtime) Setup(string xml)
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);
        db.Add(new WfeWorkflowDefinition { WorkflowCode = "CREDIT", WorkflowName = "Credit", DefinitionStatus = "released", BpmnXml = xml, CreatedBy = "test" });
        db.SaveChanges();
        return (db, new WfeRuntime(db, new Clock(), [], []));
    }

    private static async Task<WfeWorkflowInstance> ApproveAsync(CapitraDbContext db, WfeRuntime runtime, string amount)
    {
        var instance = await runtime.StartAsync("CREDIT", "APP-" + amount, new Dictionary<string, string?> { ["AMOUNT"] = amount }, "ahmed", default);
        var task = await db.Set<WfeTask>().Include(x => x.StepInstance).SingleAsync(x => x.StepInstance!.WorkflowInstanceId == instance.WorkflowInstanceId);
        await runtime.ClaimAsync(task.TaskId, "ahmed", default);
        await runtime.CompleteAsync(task.TaskId, "APPROVE", "ahmed", null, default);
        return await db.Set<WfeWorkflowInstance>().AsNoTracking().SingleAsync(x => x.WorkflowInstanceId == instance.WorkflowInstanceId);
    }

    [Fact]
    public async Task A_big_amount_goes_to_the_CFO_and_a_small_one_ends()
    {
        var (db, runtime) = Setup(Xml(Cfo));
        await using var _db = db;

        var big = await ApproveAsync(db, runtime, "2500000");
        Assert.Equal(("WAITING", "C"), (big.InstanceStatus, big.CurrentElementId));
        var why = await db.Set<WfeInstanceHist>().SingleAsync(x => x.WorkflowInstanceId == big.WorkflowInstanceId && x.EventType == "PATH_CHOSEN");
        Assert.Equal("To CFO approval: AMOUNT > 1000000 (2500000)", why.Details);

        var small = await ApproveAsync(db, runtime, "1000000");
        Assert.Equal("COMPLETED", small.InstanceStatus);
        Assert.False(await db.Set<WfeInstanceHist>().AnyAsync(x => x.WorkflowInstanceId == small.WorkflowInstanceId && x.EventType == "PATH_CHOSEN"));
    }

    [Fact]
    public void Conditions_compare_numbers_dates_yes_no_and_text()
    {
        Assert.True(WfeRules.Holds(new BpmnRule("A", ">", "1000000"), "1,500,000"));
        Assert.False(WfeRules.Holds(new BpmnRule("A", ">", "900"), ""));
        Assert.True(WfeRules.Holds(new BpmnRule("D", "<", "2026-12-01"), "2026-11-10"));
        Assert.True(WfeRules.Holds(new BpmnRule("B", "=", "yes"), "true"));
        Assert.True(WfeRules.Holds(new BpmnRule("C", "=", "egp"), "EGP"));
        Assert.True(WfeRules.Holds(new BpmnRule("C", "!=", "USD"), "EGP"));
        Assert.True(WfeRules.Holds(new BpmnRule("N", "contains", "bank"), "National Bank"));
        Assert.True(WfeRules.Holds(new BpmnRule("N", "empty", null), " "));
        Assert.True(WfeRules.Holds(new BpmnRule("N", "notEmpty", null), "x"));
        Assert.False(WfeRules.Holds(new BpmnRule("A", ">", "5"), null));
    }

    [Fact]
    public void A_diagram_without_conditions_routes_as_before()
    {
        var xml = Xml("[]");
        Assert.Equal("G", BpmnModel.Next(xml, "T", "APPROVE")?.Id);
        Assert.Equal("E", BpmnModel.Next(xml, "T", "REJECT")?.Id);
        Assert.Equal("C", BpmnModel.Next(xml, "G", "APPROVE")?.Id);
    }

    [Fact]
    public async Task Release_checks_the_fields_and_that_one_path_takes_every_other_case()
    {
        var (db, _) = Setup(Xml(Cfo));
        await using var _db = db;
        var definitions = new WfeDefinitions(db, new User("ahmed"), new Clock());
        SaveWfeDefinitionRequest Req(string xml) => new("RULES_WF", "Rules", null, "released", xml, null, true, 0);

        var e = await Assert.ThrowsAsync<BusinessValidationException>(() => definitions.SaveAsync(null,
            Req(Xml("""[{"field":"LIMIT","op":">","value":"5"},{"field":"CURRENCY","op":">","value":"EGP"}]""",
                """finova:conditions='[{"field":"AMOUNT","op":"&lt;=","value":"a lot"}]'""")), default));
        var problems = string.Join(" | ", e.Errors["rules"]);
        Assert.Contains("no data field 'LIMIT'", problems);
        Assert.Contains("CURRENCY is not a number or date", problems);
        Assert.Contains("'a lot' is not a number", problems);
        Assert.Contains("Amount check: every path out of it has conditions", problems);

        await definitions.SaveAsync(null, Req(Xml(Cfo)), default);
    }

    [Fact]
    public void With_no_path_for_other_cases_the_request_stops_with_a_clear_message()
    {
        var xml = Xml(Cfo, """finova:conditions='[{"field":"AMOUNT","op":"&lt;","value":"10"}]'""");
        var e = Assert.Throws<BusinessRuleException>(() => BpmnModel.Next(xml, "G", null, new Dictionary<string, string?> { ["AMOUNT"] = "500" }));
        Assert.Contains("No path out of 'Amount check'", e.Message);
    }
}
