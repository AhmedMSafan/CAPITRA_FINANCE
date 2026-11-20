using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>The process monitor: where each request is and for how long, one request's steps and history, and time per step and person.</summary>
public sealed class WorkflowMonitorTests
{
    private const string Xml = """<bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:finova="http://finova/workflow"><bpmn:process id="P"><bpmn:startEvent id="S"/><bpmn:userTask id="R" name="Review" finova:stepCode="REVIEW" finova:assignmentType="User" finova:assignee="mona" finova:actions="APPROVE,REJECT" finova:slaDuration="4" finova:slaUnit="HOURS"/><bpmn:userTask id="A" name="Approve" finova:stepCode="APPROVE" finova:assignmentType="User" finova:assignee="omar" finova:actions="APPROVE"/><bpmn:endEvent id="E"/><bpmn:sequenceFlow id="F1" sourceRef="S" targetRef="R"/><bpmn:sequenceFlow id="F2" sourceRef="R" targetRef="A"/><bpmn:sequenceFlow id="F3" sourceRef="A" targetRef="E"/></bpmn:process></bpmn:definitions>""";

    private sealed class Clock : IClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 11, 10, 8, 0, 0, DateTimeKind.Utc);
    }

    private static async Task DoAsync(CapitraDbContext db, WfeRuntime runtime, string reference, string user)
    {
        var item = await db.Set<WfeInboxItem>().AsNoTracking().Include(x => x.Instance)
            .SingleAsync(x => x.AssignedTo == user && x.InboxStatus == "OPEN" && x.Instance!.BusinessReference == reference);
        await runtime.ClaimAsync(item.TaskId, user, default);
        await runtime.CompleteAsync(item.TaskId, "APPROVE", user, null, default);
    }

    [Fact]
    public async Task The_monitor_shows_where_requests_wait_and_how_long_each_step_takes()
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);
        await using var _db = db;
        db.Add(new WfeWorkflowDefinition { WorkflowCode = "LOAN", WorkflowName = "Loan", DefinitionStatus = "released", BpmnXml = Xml, CreatedBy = "test" });
        await db.SaveChangesAsync();
        var clock = new Clock();
        var runtime = new WfeRuntime(db, clock, [], []);
        var monitor = new WfeMonitor(db, clock);
        var start = clock.UtcNow;

        // L-1: reviewed in 2 hours, approved 3 hours later; L-2: reviewed late (6 hours), now waiting at Approve
        await runtime.StartAsync("LOAN", "L-1", null, "ahmed", default);
        await runtime.StartAsync("LOAN", "L-2", null, "ahmed", default);
        clock.UtcNow = start.AddHours(2);
        await DoAsync(db, runtime, "L-1", "mona");
        clock.UtcNow = start.AddHours(5);
        await DoAsync(db, runtime, "L-1", "omar");
        clock.UtcNow = start.AddHours(6);
        await DoAsync(db, runtime, "L-2", "mona");
        clock.UtcNow = start.AddHours(9);

        var running = await monitor.ListAsync(null, null, null, default);
        var l2 = Assert.Single(running);
        Assert.Equal(("L-2", "Approve", "omar", 3.0, 9.0), (l2.BusinessReference, l2.CurrentStep, l2.WaitingFor, l2.WaitingHours, l2.TotalHours));
        Assert.Equal("L-1", Assert.Single(await monitor.ListAsync("finished", null, null, default)).BusinessReference);
        Assert.Equal(2, (await monitor.ListAsync("all", null, "l-", default)).Count);

        var view = await monitor.InstanceAsync(l2.InstanceId, default);
        Assert.Equal(Xml, view.BpmnXml);
        var review = view.Steps.Single(s => s.ElementId == "R");
        Assert.Equal((6.0, "mona"), (review.Hours, review.CompletedBy));
        Assert.Equal("omar", Assert.Single(view.Steps.Single(s => s.ElementId == "A").Tasks).AssignedTo);
        Assert.Contains(view.History, h => h.EventType == "TASK_COMPLETED" && h.By == "mona");

        var report = await monitor.TimesAsync(null, null, null, default);
        var r = report.Steps.Single(s => s.ElementId == "R");
        Assert.Equal((2, 4.0, 6.0, 0, 1), (r.Completed, r.AverageHours, r.LongestHours, r.Waiting, r.Late));
        var a = report.Steps.Single(s => s.ElementId == "A");
        Assert.Equal((1, 3.0, 1), (a.Completed, a.AverageHours, a.Waiting));
        var mona = report.People.Single(p => p.UserName == "mona");
        Assert.Equal((2, 4.0, 1, 0), (mona.Completed, mona.AverageHours, mona.Late, mona.Open));
        Assert.Equal(1, report.People.Single(p => p.UserName == "omar").Open);
    }
}
