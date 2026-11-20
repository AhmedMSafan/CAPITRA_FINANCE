using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Trs;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>
/// Step time limits (SLA): a task is due after its step's time limit, the assignee is reminded before and told when it is
/// overdue, and after the grace hours it is escalated (moved to another role or user, or they are told). Each happens once.
/// </summary>
public sealed class WorkflowSlaTests
{
    private static string Xml(string sla) =>
        $"""<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P" finova:workflowType="SYSTEM"><bpmn:startEvent id="S"/><bpmn:userTask id="T" name="Credit review" finova:stepCode="REVIEW" finova:assignmentType="User" finova:assignee="ahmed" finova:actions="APPROVE,REJECT" {sla}/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="T"/><bpmn:sequenceFlow id="F2" sourceRef="T" targetRef="E"/></bpmn:process></bpmn:definitions>""";

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

    private static (CapitraDbContext Db, Clock Clock, WfeRuntime Runtime, WfeSla Sla) Setup(string sla)
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);
        db.Add(new WfeWorkflowDefinition { WorkflowCode = "CREDIT", WorkflowName = "Credit", DefinitionStatus = "released", BpmnXml = Xml(sla), CreatedBy = "test" });
        db.SaveChanges();
        var clock = new Clock();
        return (db, clock, new WfeRuntime(db, clock, [], []), new WfeSla(db, clock, new Directory()));
    }

    private static Task<List<WfeAlert>> AlertsAsync(CapitraDbContext db) => db.Set<WfeAlert>().AsNoTracking().OrderBy(x => x.AlertId).ToListAsync();

    [Fact]
    public async Task A_task_is_reminded_then_overdue_then_escalated_to_a_role_once_each()
    {
        var (db, clock, runtime, sla) = Setup("""finova:slaDuration="4" finova:slaUnit="HOURS" finova:slaRemindHours="1" finova:slaEscalation="REASSIGN" finova:slaEscalateHours="2" finova:slaEscalateType="Role" finova:slaEscalateTo="CREDIT_MANAGER" """);
        await using var _db = db;
        var start = clock.UtcNow;
        var instance = await runtime.StartAsync("CREDIT", "APP-77", null, "ahmed", default);
        var inbox = await db.Set<WfeInboxItem>().SingleAsync();
        Assert.Equal(start.AddHours(4), inbox.DueAt);
        Assert.Equal(start.AddHours(3), inbox.RemindAt);
        Assert.Equal(start.AddHours(6), inbox.EscalateAt);

        Assert.Equal(new WfeSlaRunResult(0, 0, 0), await sla.RunAsync(default));

        clock.UtcNow = start.AddHours(3);
        Assert.Equal(new WfeSlaRunResult(1, 0, 0), await sla.RunAsync(default));
        var reminder = Assert.Single(await AlertsAsync(db));
        Assert.Equal(("ahmed", WfeSla.Reminder), (reminder.UserName, reminder.AlertType));
        Assert.Contains("Credit review", reminder.Message);

        // the assignee claimed it, but did not finish in time
        await runtime.ClaimAsync(inbox.TaskId, "ahmed", default);
        clock.UtcNow = start.AddHours(4).AddMinutes(1);
        Assert.Equal(new WfeSlaRunResult(0, 1, 0), await sla.RunAsync(default));
        Assert.Equal(WfeSla.Overdue, (await AlertsAsync(db)).Last().AlertType);

        clock.UtcNow = start.AddHours(6);
        Assert.Equal(new WfeSlaRunResult(0, 0, 1), await sla.RunAsync(default));
        Assert.Equal(new WfeSlaRunResult(0, 0, 0), await sla.RunAsync(default));

        inbox = await db.Set<WfeInboxItem>().AsNoTracking().Include(x => x.Task).SingleAsync();
        Assert.Equal(("OPEN", "Role", "CREDIT_MANAGER", "ahmed"), (inbox.InboxStatus, inbox.AssignmentType, inbox.AssignedRole, inbox.EscalatedFrom));
        Assert.Null(inbox.ClaimedBy);
        Assert.Equal(("OPEN", "CREDIT_MANAGER"), (inbox.Task!.TaskStatus, inbox.Task.Assignee));
        var escalated = (await AlertsAsync(db)).Where(x => x.AlertType == WfeSla.Escalated).Select(x => x.UserName).Order().ToList();
        Assert.Equal(["ahmed", "mona", "omar"], escalated);
        Assert.Contains(await db.Set<WfeInstanceHist>().Select(x => x.EventType).ToListAsync(), e => e == "TASK_ESCALATED");

        // the role can now work it; finishing late is recorded
        await runtime.ClaimAsync(inbox.TaskId, "mona", default);
        await runtime.CompleteAsync(inbox.TaskId, "APPROVE", "mona", null, default);
        var done = await db.Set<WfeInstanceHist>().SingleAsync(x => x.WorkflowInstanceId == instance.WorkflowInstanceId && x.EventType == "TASK_COMPLETED");
        Assert.Contains("after its due time", done.Details);

        // mona's alert (the task was escalated to her role), read on its own
        var alerts = new WfeAlerts(db, new User("mona"), clock);
        var mine = await alerts.MineAsync(default);
        Assert.Single(mine);
        await alerts.ReadAsync(mine[0].AlertId, default);
        Assert.True((await alerts.MineAsync(default))[0].IsRead);
    }

    [Fact]
    public async Task Notify_only_tells_the_user_and_leaves_the_task_where_it_is()
    {
        var (db, clock, runtime, sla) = Setup("""finova:slaDuration="1" finova:slaUnit="DAYS" finova:slaEscalation="NOTIFY" finova:slaEscalateType="User" finova:slaEscalateTo="omar" """);
        await using var _db = db;
        await runtime.StartAsync("CREDIT", "APP-78", null, "ahmed", default);
        clock.UtcNow = clock.UtcNow.AddDays(1);
        Assert.Equal(new WfeSlaRunResult(0, 1, 1), await sla.RunAsync(default));

        var inbox = await db.Set<WfeInboxItem>().AsNoTracking().SingleAsync();
        Assert.Equal(("User", "ahmed"), (inbox.AssignmentType, inbox.AssignedTo));
        Assert.Null(inbox.EscalatedFrom);
        var alerts = await AlertsAsync(db);
        Assert.Equal([("ahmed", WfeSla.Overdue), ("omar", WfeSla.Escalated)], alerts.Select(x => (x.UserName, x.AlertType)));
    }

    [Fact]
    public async Task Working_days_skip_the_weekend_and_the_calendar_holidays()
    {
        var (db, _, _, _) = Setup("");
        await using var _db = db;
        var two = new BpmnSla(2, BpmnSla.WorkingDays, null, null, BpmnSla.None, null, null);
        // Thursday 12 Nov 2026, 10:00 Cairo (08:00 UTC): Friday and Saturday are skipped, so Sunday and Monday
        var thursday = new DateTime(2026, 11, 12, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc), await WfeSla.DueAsync(db, two, thursday, default));

        db.Add(new TrsCalendar { CompanyId = 1, CalendarDate = new DateOnly(2026, 11, 15), IsBusinessDay = false, HolidayNameEn = "Bank holiday" });
        await db.SaveChangesAsync();
        Assert.Equal(new DateTime(2026, 11, 17, 8, 0, 0, DateTimeKind.Utc), await WfeSla.DueAsync(db, two, thursday, default));
    }

    [Fact]
    public async Task A_workflow_with_an_unfinished_deadline_is_not_released()
    {
        var (db, clock, _, _) = Setup("");
        await using var _db = db;
        var definitions = new WfeDefinitions(db, new User("ahmed"), clock);
        SaveWfeDefinitionRequest Req(string sla) => new("SLA_WF", "Deadlines", null, "released", Xml(sla), null, true, 0);

        var e = await Assert.ThrowsAsync<BusinessValidationException>(() => definitions.SaveAsync(null,
            Req("""finova:slaDuration="1.5" finova:slaUnit="WORKING_DAYS" finova:slaEscalation="REASSIGN" """), default));
        var problems = string.Join(" | ", e.Errors["sla"]);
        Assert.Contains("whole number", problems);
        Assert.Contains("user or role", problems);

        await definitions.SaveAsync(null, Req("""finova:slaDuration="8" finova:slaUnit="HOURS" finova:slaRemindHours="2" """), default);
    }
}
