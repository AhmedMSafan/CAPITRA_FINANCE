using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

/// <summary>One request in the process monitor: where it is, who has it and for how long.</summary>
/// <param name="WaitingHours">How long it has been at its current step (running requests only).</param>
public sealed record WfeMonitorRowDto(
    long InstanceId, long WorkflowDefinitionId, string WorkflowName, string? BusinessReference, string Status, string? CurrentStep,
    string? WaitingFor, DateTime StartedAt, string StartedBy, DateTime? CompletedAt, double? WaitingHours, bool Overdue, double TotalHours);

/// <summary>One visit of a step on the process view: when it started and ended, who finished it, how long it took.</summary>
public sealed record WfeMonitorStepDto(
    string ElementId, string? StepName, string ElementType, string Status, string? Outcome, DateTime EnteredAt, DateTime? CompletedAt,
    string? CompletedBy, double Hours, IReadOnlyList<WfeMonitorTaskDto> Tasks);

/// <summary>A task of a step: who it is with, its state, and its due time.</summary>
public sealed record WfeMonitorTaskDto(string? AssignedTo, string Status, string? ClaimedBy, string? Outcome, DateTime? DueAt, DateTime? CompletedAt);

public sealed record WfeMonitorEventDto(DateTime At, string EventType, string? ElementId, string? Action, string? By, string? Details);

/// <summary>The process view of one request: its diagram (the version it runs on), each step's visits and times, and its history.</summary>
public sealed record WfeMonitorInstanceDto(
    WfeMonitorRowDto Summary, string BpmnXml, IReadOnlyList<WfeMonitorStepDto> Steps, IReadOnlyList<WfeMonitorEventDto> History,
    IReadOnlyList<WfeDataValueDto> Data);

/// <summary>Time spent at one step of one workflow, over the finished visits in the period.</summary>
public sealed record WfeStepTimeDto(
    long WorkflowDefinitionId, string WorkflowName, string ElementId, string StepName, int Completed, double AverageHours, double MedianHours,
    double LongestHours, int Waiting, int Overdue, int Late);

/// <summary>Time one person took on the tasks they finished in the period.</summary>
public sealed record WfeUserTimeDto(string UserName, int Completed, double AverageHours, double LongestHours, int Late, int Open);

public sealed record WfeTimeReportDto(DateOnly From, DateOnly To, IReadOnlyList<WfeStepTimeDto> Steps, IReadOnlyList<WfeUserTimeDto> People);

/// <summary>
/// The process monitor (K2's View Flow and process reports): every request with its current step, the diagram of one request with
/// its progress, and the time spent at each step and by each person.
/// </summary>
public sealed class WfeMonitor(ICapitraDbContext db, IClock clock)
{
    private static readonly string[] Closed = ["COMPLETED", "REJECTED", "CANCELLED"];
    private static readonly string[] OpenTask = [WfeRuntime.Open, WfeRuntime.Claimed, WfeRuntime.InProgress];

    /// <param name="status">running (default), finished or all.</param>
    public async Task<IReadOnlyList<WfeMonitorRowDto>> ListAsync(string? status, long? workflowDefinitionId, string? search, CancellationToken ct)
    {
        var q = db.Set<WfeWorkflowInstance>().AsNoTracking().Include(x => x.Definition).AsQueryable();
        var s = (status ?? "running").Trim().ToLowerInvariant();
        if (s == "running")
            q = q.Where(x => !Closed.Contains(x.InstanceStatus));
        else if (s == "finished")
            q = q.Where(x => Closed.Contains(x.InstanceStatus));
        if (workflowDefinitionId is { } wid)
            q = q.Where(x => x.WorkflowDefinitionId == wid);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim().ToLowerInvariant();
#pragma warning disable CA1862 // EF Core translates ToLower to SQL lower()
            q = q.Where(x => (x.BusinessReference != null && x.BusinessReference.ToLower().Contains(t)) || x.StartedBy.ToLower().Contains(t));
#pragma warning restore CA1862
        }

        var rows = await q.OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.WorkflowInstanceId).Take(500).ToListAsync(ct);
        var ids = rows.Select(x => x.WorkflowInstanceId).ToList();
        var steps = await db.Set<WfeStepInstance>().AsNoTracking().Where(x => ids.Contains(x.WorkflowInstanceId)).ToListAsync(ct);
        var inbox = await db.Set<WfeInboxItem>().AsNoTracking()
            .Where(x => ids.Contains(x.WorkflowInstanceId) && OpenTask.Contains(x.InboxStatus)).ToListAsync(ct);
        return rows.Select(x => Row(x, steps.Where(s => s.WorkflowInstanceId == x.WorkflowInstanceId).ToList(),
            inbox.Where(i => i.WorkflowInstanceId == x.WorkflowInstanceId).ToList())).ToList();
    }

    public async Task<WfeMonitorInstanceDto> InstanceAsync(long instanceId, CancellationToken ct)
    {
        var x = await db.Set<WfeWorkflowInstance>().AsNoTracking()
            .Include(i => i.Definition).Include(i => i.Version).Include(i => i.Variables)
            .FirstOrDefaultAsync(i => i.WorkflowInstanceId == instanceId, ct)
            ?? throw new NotFoundException("Workflow request not found.");
        var steps = await db.Set<WfeStepInstance>().AsNoTracking().Where(s => s.WorkflowInstanceId == instanceId)
            .OrderBy(s => s.EnteredAt).ThenBy(s => s.StepInstanceId).ToListAsync(ct);
        var items = await db.Set<WfeInboxItem>().AsNoTracking().Where(i => i.WorkflowInstanceId == instanceId).OrderBy(i => i.InboxItemId).ToListAsync(ct);
        var history = await db.Set<WfeInstanceHist>().AsNoTracking().Where(h => h.WorkflowInstanceId == instanceId)
            .OrderBy(h => h.EventAt).ThenBy(h => h.InstanceHistId)
            .Select(h => new WfeMonitorEventDto(h.EventAt, h.EventType, h.ElementId, h.ActionCode, h.PerformedBy, h.Details)).ToListAsync(ct);
        var now = clock.UtcNow;
        var xml = WfeVersions.XmlOf(x);
        var fields = WfeDataFields.Of(xml);
        var values = x.Variables.GroupBy(v => v.VariableName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().VariableValue, StringComparer.OrdinalIgnoreCase);
        var data = fields.Select(f => new WfeDataValueDto(f.Name, f.Caption, f.Type, f.Required, values.GetValueOrDefault(f.Name), f.Description)).ToList();
        return new WfeMonitorInstanceDto(
            Row(x, steps, items.Where(i => OpenTask.Contains(i.InboxStatus)).ToList()), xml,
            steps.Select(s => new WfeMonitorStepDto(s.ElementId, s.StepName, s.ElementType, s.StepStatus, s.Outcome, s.EnteredAt, s.CompletedAt, s.CompletedBy,
                Hours(s.EnteredAt, s.CompletedAt ?? now),
                items.Where(i => i.StepInstanceId == s.StepInstanceId)
                    .Select(i => new WfeMonitorTaskDto(i.AssignedTo ?? i.AssignedRole, i.InboxStatus, i.ClaimedBy, i.Outcome, i.DueAt, i.CompletedAt)).ToList()))
                .ToList(),
            history, data);
    }

    /// <summary>
    /// Time per step and per person over the steps finished between <paramref name="from"/> and <paramref name="to"/> (dates, inclusive),
    /// with what is waiting and overdue now. Only human steps count: automatic ones take no time.
    /// </summary>
    public async Task<WfeTimeReportDto> TimesAsync(DateOnly? from, DateOnly? to, long? workflowDefinitionId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow);
        var end = to ?? today;
        var start = from ?? end.AddDays(-90);
        var startUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var now = clock.UtcNow;

        var q = db.Set<WfeInboxItem>().AsNoTracking().Include(i => i.StepInstance!).ThenInclude(s => s.Instance!).ThenInclude(i => i.Definition).AsQueryable();
        if (workflowDefinitionId is { } wid)
            q = q.Where(i => i.Instance!.WorkflowDefinitionId == wid);
        var items = await q.Where(i => OpenTask.Contains(i.InboxStatus)
                || (i.InboxStatus == WfeRuntime.Completed && i.CompletedAt >= startUtc && i.CompletedAt < endUtc))
            .ToListAsync(ct);

        var steps = items.Where(i => i.StepInstance?.Instance?.Definition is not null)
            .GroupBy(i => (i.StepInstance!.Instance!.WorkflowDefinitionId, i.StepInstance.ElementId))
            .Select(g =>
            {
                var first = g.First().StepInstance!;
                // a step's time is from entering it until it was decided: the visit, not each approver's task
                var visits = g.Where(i => i.InboxStatus == WfeRuntime.Completed).Select(i => i.StepInstance!).DistinctBy(s => s.StepInstanceId)
                    .Where(s => s.CompletedAt is not null).Select(s => Hours(s.EnteredAt, s.CompletedAt!.Value)).OrderBy(h => h).ToList();
                var open = g.Where(i => OpenTask.Contains(i.InboxStatus)).ToList();
                return new WfeStepTimeDto(
                    first.Instance!.WorkflowDefinitionId, first.Instance.Definition!.WorkflowName, first.ElementId, first.StepName ?? first.ElementId,
                    visits.Count, Round(visits.DefaultIfEmpty(0).Average()), Round(Median(visits)), Round(visits.DefaultIfEmpty(0).Max()),
                    open.Select(i => i.StepInstanceId).Distinct().Count(), open.Count(i => i.DueAt is { } d && d < now),
                    g.Count(i => i.InboxStatus == WfeRuntime.Completed && i.DueAt is { } d && i.CompletedAt > d));
            })
            .OrderBy(x => x.WorkflowName).ThenByDescending(x => x.AverageHours).ToList();

        var people = items.Where(i => !string.IsNullOrWhiteSpace(i.ClaimedBy ?? i.AssignedTo))
            .GroupBy(i => (i.ClaimedBy ?? i.AssignedTo)!, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var done = g.Where(i => i.InboxStatus == WfeRuntime.Completed && i.CompletedAt is not null)
                    .Select(i => Hours(i.StepInstance!.EnteredAt, i.CompletedAt!.Value)).ToList();
                return new WfeUserTimeDto(g.Key, done.Count, Round(done.DefaultIfEmpty(0).Average()), Round(done.DefaultIfEmpty(0).Max()),
                    g.Count(i => i.InboxStatus == WfeRuntime.Completed && i.DueAt is { } d && i.CompletedAt > d), g.Count(i => OpenTask.Contains(i.InboxStatus)));
            })
            .OrderByDescending(x => x.AverageHours).ToList();

        return new WfeTimeReportDto(start, end, steps, people);
    }

    private WfeMonitorRowDto Row(WfeWorkflowInstance x, List<WfeStepInstance> steps, List<WfeInboxItem> open)
    {
        var now = clock.UtcNow;
        var running = !Closed.Contains(x.InstanceStatus);
        var current = running ? steps.Where(s => s.StepStatus != WfeRuntime.Completed).OrderByDescending(s => s.EnteredAt).FirstOrDefault() : null;
        var who = open.Count == 0 ? null : string.Join(", ", open.Select(i => i.ClaimedBy ?? i.AssignedTo ?? i.AssignedRole).Where(w => w is not null).Distinct());
        return new WfeMonitorRowDto(
            x.WorkflowInstanceId, x.WorkflowDefinitionId, x.Definition?.WorkflowName ?? x.WorkflowDefinitionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            x.BusinessReference, x.InstanceStatus, current?.StepName ?? current?.ElementId, who, x.StartedAt, x.StartedBy, x.CompletedAt,
            current is null ? null : Round(Hours(current.EnteredAt, now)), open.Any(i => i.DueAt is { } d && d < now),
            Round(Hours(x.StartedAt, x.CompletedAt ?? now)));
    }

    private static double Hours(DateTime from, DateTime to) => Math.Max(0, (to - from).TotalHours);

    private static double Round(double h) => Math.Round(h, 2);

    private static double Median(List<double> sorted) =>
        sorted.Count == 0 ? 0 : sorted.Count % 2 == 1 ? sorted[sorted.Count / 2] : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2;
}
