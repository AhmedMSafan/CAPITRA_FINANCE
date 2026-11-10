using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>
/// Parallel approvals: one step sends a task to every approver at once, and moves on when all of them, any one, or a number
/// of them approve. A rejection that makes the rule impossible decides at once; the unanswered tasks are cancelled.
/// </summary>
public sealed class WorkflowParallelTests
{
    private static string Xml(string assignment, string parallel) =>
        $"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P"><bpmn:startEvent id="S"/><bpmn:userTask id="T" name="Committee approval" finova:stepCode="COMMITTEE" {assignment} finova:actions="APPROVE,REJECT" {parallel}/><bpmn:userTask id="D" name="Disburse" finova:stepCode="DISBURSE" finova:assignmentType="User" finova:assignee="ops" finova:actions="COMPLETE"/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="T"/><bpmn:sequenceFlow id="F2" name="APPROVE" sourceRef="T" targetRef="D"/><bpmn:sequenceFlow id="F3" name="REJECT" sourceRef="T" targetRef="E"/><bpmn:sequenceFlow id="F4" sourceRef="D" targetRef="E"/></bpmn:process></bpmn:definitions>""";

    private const string Users = """finova:assignmentType="User" finova:assignee="mona, omar, ahmed" """;

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

    private sealed class Directory : IUserDirectory
    {
        public Task<IReadOnlyList<DirectoryUser>> ActiveUsersAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DirectoryUser>>(
            [
                new("1", "ahmed", "Ahmed Safan", ["OFFICER"]),
                new("2", "mona", "Mona Hassan", ["CREDIT_MANAGER"]),
                new("3", "omar", "Omar Adel", ["CREDIT_MANAGER"]),
            ]);
    }

    private sealed class Counter : IWfeTaskCompletionHandler
    {
        public List<string> Calls { get; } = [];
        public bool CanHandle(WfeTaskContext context) => context.StepCode == "COMMITTEE";
        public Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
        {
            Calls.Add(context.Action);
            return Task.CompletedTask;
        }
    }

    private static (CapitraDbContext Db, WfeRuntime Runtime, Counter Counter) Setup(string xml)
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);
        db.Add(new WfeWorkflowDefinition { WorkflowCode = "LOAN", WorkflowName = "Loan", DefinitionStatus = "released", BpmnXml = xml, CreatedBy = "test" });
        db.SaveChanges();
        var counter = new Counter();
        return (db, new WfeRuntime(db, new Clock(), [counter], [], new Directory()), counter);
    }

    private static async Task DecideAsync(CapitraDbContext db, WfeRuntime runtime, string user, string action)
    {
        var item = await db.Set<WfeInboxItem>().AsNoTracking().SingleAsync(x => x.AssignedTo == user && x.InboxStatus == "OPEN");
        await runtime.ClaimAsync(item.TaskId, user, default);
        await runtime.CompleteAsync(item.TaskId, action, user, null, default);
    }

    private static Task<WfeWorkflowInstance> InstanceAsync(CapitraDbContext db) => db.Set<WfeWorkflowInstance>().AsNoTracking().SingleAsync();

    private static async Task<List<(string?, string)>> CommitteeAsync(CapitraDbContext db) =>
        (await db.Set<WfeInboxItem>().AsNoTracking().Where(x => x.Subject == "Committee approval").OrderBy(x => x.AssignedTo).ToListAsync())
        .Select(x => (x.AssignedTo, x.InboxStatus)).ToList();

    [Fact]
    public async Task All_must_approve_before_the_request_moves_on()
    {
        var (db, runtime, counter) = Setup(Xml(Users, """finova:parallelMode="ALL" """));
        await using var _db = db;
        await runtime.StartAsync("LOAN", "L-1", null, "ahmed", default);
        Assert.Equal([("ahmed", "OPEN"), ("mona", "OPEN"), ("omar", "OPEN")], await CommitteeAsync(db));

        await DecideAsync(db, runtime, "mona", "APPROVE");
        await DecideAsync(db, runtime, "omar", "APPROVE");
        Assert.Equal("T", (await InstanceAsync(db)).CurrentElementId);
        Assert.Empty(counter.Calls);

        await DecideAsync(db, runtime, "ahmed", "APPROVE");
        Assert.Equal("D", (await InstanceAsync(db)).CurrentElementId);
        Assert.Equal(["APPROVE"], counter.Calls);
        var decided = await db.Set<WfeInstanceHist>().SingleAsync(x => x.EventType == "PARALLEL_DECIDED");
        Assert.Equal("APPROVE: 3 of 3 approved, 0 rejected; needs all 3.", decided.Details);
        Assert.Equal(2, await db.Set<WfeInstanceHist>().CountAsync(x => x.EventType == "PARALLEL_WAITING"));
    }

    [Fact]
    public async Task With_all_needed_one_rejection_decides_and_cancels_the_rest()
    {
        var (db, runtime, counter) = Setup(Xml(Users, """finova:parallelMode="ALL" """));
        await using var _db = db;
        await runtime.StartAsync("LOAN", "L-2", null, "ahmed", default);
        await DecideAsync(db, runtime, "mona", "APPROVE");
        await DecideAsync(db, runtime, "omar", "REJECT");

        var instance = await InstanceAsync(db);
        Assert.Equal("COMPLETED", instance.InstanceStatus);
        Assert.Equal([("ahmed", "CANCELLED"), ("mona", "COMPLETED"), ("omar", "COMPLETED")], await CommitteeAsync(db));
        Assert.Equal(["REJECT"], counter.Calls);
        Assert.Equal("REJECT", (await db.Set<WfeStepInstance>().SingleAsync(x => x.ElementId == "T")).Outcome);
    }

    [Fact]
    public async Task Any_member_of_the_role_can_approve_for_all()
    {
        var (db, runtime, _) = Setup(Xml("""finova:assignmentType="Role" finova:assignee="CREDIT_MANAGER" """, """finova:parallelMode="ANY" """));
        await using var _db = db;
        await runtime.StartAsync("LOAN", "L-3", null, "ahmed", default);
        Assert.Equal([("mona", "OPEN"), ("omar", "OPEN")], await CommitteeAsync(db));

        await DecideAsync(db, runtime, "omar", "APPROVE");
        Assert.Equal("D", (await InstanceAsync(db)).CurrentElementId);
        Assert.Equal([("mona", "CANCELLED"), ("omar", "COMPLETED")], await CommitteeAsync(db));
    }

    [Fact]
    public async Task Two_of_three_ignores_one_rejection()
    {
        var (db, runtime, _) = Setup(Xml(Users, """finova:parallelMode="COUNT" finova:parallelCount="2" """));
        await using var _db = db;
        await runtime.StartAsync("LOAN", "L-4", null, "ahmed", default);
        await DecideAsync(db, runtime, "mona", "REJECT");
        await DecideAsync(db, runtime, "omar", "APPROVE");
        Assert.Equal("T", (await InstanceAsync(db)).CurrentElementId);
        await DecideAsync(db, runtime, "ahmed", "APPROVE");
        Assert.Equal("D", (await InstanceAsync(db)).CurrentElementId);
    }

    [Fact]
    public async Task A_step_without_parallel_settings_works_as_before()
    {
        var (db, runtime, counter) = Setup(Xml("""finova:assignmentType="Role" finova:assignee="CREDIT_MANAGER" """, ""));
        await using var _db = db;
        await runtime.StartAsync("LOAN", "L-5", null, "ahmed", default);
        var item = await db.Set<WfeInboxItem>().AsNoTracking().SingleAsync();
        Assert.Equal(("Role", null, "CREDIT_MANAGER"), (item.AssignmentType, item.AssignedTo, item.AssignedRole));
        await runtime.ClaimAsync(item.TaskId, "mona", default);
        await runtime.CompleteAsync(item.TaskId, "APPROVE", "mona", null, default);
        Assert.Equal("D", (await InstanceAsync(db)).CurrentElementId);
        Assert.Equal(["APPROVE"], counter.Calls);
    }

    [Fact]
    public async Task Release_checks_the_approvers_and_the_number_needed()
    {
        var (db, _, _) = Setup(Xml(Users, ""));
        await using var _db = db;
        var definitions = new WfeDefinitions(db, new User("ahmed"), new Clock());
        SaveWfeDefinitionRequest Req(string xml) => new("PAR_WF", "Parallel", null, "released", xml, null, true, 0);

        var one = await Assert.ThrowsAsync<BusinessValidationException>(() => definitions.SaveAsync(null,
            Req(Xml("""finova:assignmentType="User" finova:assignee="mona" """, """finova:parallelMode="ALL" """)), default));
        Assert.Contains("at least two users", string.Join(" | ", one.Errors["parallel"]));
        var many = await Assert.ThrowsAsync<BusinessValidationException>(() => definitions.SaveAsync(null,
            Req(Xml(Users, """finova:parallelMode="COUNT" finova:parallelCount="4" """)), default));
        Assert.Contains("needs 4 approvals but has only 3", string.Join(" | ", many.Errors["parallel"]));

        await definitions.SaveAsync(null, Req(Xml(Users, """finova:parallelMode="COUNT" finova:parallelCount="2" """)), default);
    }
}
