using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Trs;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

/// <summary>An alert on the bell in the page header.</summary>
public sealed record WfeAlertDto(long AlertId, string AlertType, string Message, long? InboxId, long? InstanceId, DateTime AlertAt, bool IsRead);

/// <summary>What one run of the time-limit check did.</summary>
public sealed record WfeSlaRunResult(int Reminded, int Overdue, int Escalated);

/// <summary>
/// Step time limits (SLA), as in K2: a human step can have a time limit; its task is then due at a set time, the assignee is
/// reminded before it, told when it is overdue, and after a grace period the task is escalated: moved to another user or role
/// (REASSIGN) or that user or role is told (NOTIFY). Working days skip the Treasury calendar's holidays and Friday and Saturday.
/// </summary>
public sealed class WfeSla(ICapitraDbContext db, IClock clock, IUserDirectory directory)
{
    public const string Reminder = "REMINDER";
    public const string Overdue = "OVERDUE";
    public const string Escalated = "ESCALATED";

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>Sets a new task's due, reminder and escalation times from its step's time limit.</summary>
    public static async Task ApplyAsync(ICapitraDbContext db, WfeInboxItem inbox, BpmnSla? sla, DateTime nowUtc, CancellationToken ct)
    {
        if (sla is null)
            return;
        var due = await DueAsync(db, sla, nowUtc, ct);
        inbox.DueAt = due;
        if (sla.RemindHours is > 0 && due.AddHours((double)-sla.RemindHours.Value) > nowUtc)
            inbox.RemindAt = due.AddHours((double)-sla.RemindHours.Value);
        if (sla.Escalates)
            inbox.EscalateAt = due.AddHours((double)(sla.EscalateHours ?? 0));
    }

    /// <summary>When a task started at <paramref name="startUtc"/> is due.</summary>
    public static async Task<DateTime> DueAsync(ICapitraDbContext db, BpmnSla sla, DateTime startUtc, CancellationToken ct)
    {
        startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        switch (sla.Unit)
        {
            case BpmnSla.Days:
                return startUtc.AddDays((double)sla.Duration);
            case BpmnSla.WorkingDays:
                // the same time of day, that many working days later (Cairo time)
                var local = TimeZoneInfo.ConvertTimeFromUtc(startUtc, Cairo);
                var day = DateOnly.FromDateTime(local);
                var days = (int)Math.Ceiling(sla.Duration);
                var holidays = await HolidaysAsync(db, day, day.AddDays(days * 3 + 14), ct);
                while (days > 0)
                {
                    day = day.AddDays(1);
                    if (IsWorkingDay(day, holidays))
                        days--;
                }

                var dueLocal = DateTime.SpecifyKind(day.ToDateTime(TimeOnly.FromDateTime(local)), DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(dueLocal, Cairo);
            default:
                return startUtc.AddHours((double)sla.Duration);
        }
    }

    /// <summary>Days the Treasury calendar marks as working (true) or not (false); days it does not list follow the Friday-Saturday weekend.</summary>
    private static async Task<Dictionary<DateOnly, bool>> HolidaysAsync(ICapitraDbContext db, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var rows = await db.Set<TrsCalendar>().AsNoTracking()
            .Where(x => x.IsActive && x.CalendarDate > from && x.CalendarDate <= to)
            .Select(x => new { x.CalendarDate, x.IsBusinessDay })
            .ToListAsync(ct);
        return rows.GroupBy(x => x.CalendarDate).ToDictionary(g => g.Key, g => g.All(x => x.IsBusinessDay));
    }

    private static bool IsWorkingDay(DateOnly day, Dictionary<DateOnly, bool> calendar) =>
        calendar.TryGetValue(day, out var working) ? working : day.DayOfWeek is not (DayOfWeek.Friday or DayOfWeek.Saturday);

    /// <summary>The mistakes in the steps' time limits, for the designer to fix before release.</summary>
    public static List<string> Check(string xml)
    {
        var errors = new List<string>();
        foreach (var step in BpmnModel.SlaAttributes(xml).GroupBy(x => x.Step))
        {
            var a = step.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
            decimal? Num(string n) =>
                a.TryGetValue(n, out var v) ? decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : -1 : null;
            var duration = Num("slaDuration");
            if (duration is null)
            {
                if (a.Keys.Any(k => k is not ("slaUnit" or "slaEscalation" or "slaEscalateType") || (k == "slaEscalation" && a[k] != BpmnSla.None)))
                    errors.Add($"{step.Key}: give the time limit, or clear the deadline settings.");
                continue;
            }

            if (duration <= 0)
                errors.Add($"{step.Key}: the time limit must be a number above 0.");
            var unit = a.GetValueOrDefault("slaUnit", BpmnSla.Hours).ToUpperInvariant();
            if (unit is not (BpmnSla.Hours or BpmnSla.Days or BpmnSla.WorkingDays))
                errors.Add($"{step.Key}: the time limit unit must be hours, days or working days.");
            else if (unit == BpmnSla.WorkingDays && duration > 0 && duration != Math.Floor(duration.Value))
                errors.Add($"{step.Key}: working days must be a whole number.");
            if (Num("slaRemindHours") is < 0)
                errors.Add($"{step.Key}: the reminder must be a number of hours, 0 or more.");
            if (Num("slaEscalateHours") is < 0)
                errors.Add($"{step.Key}: the escalation delay must be a number of hours, 0 or more.");
            var escalation = a.GetValueOrDefault("slaEscalation", BpmnSla.None).ToUpperInvariant();
            if (escalation is not (BpmnSla.None or BpmnSla.Reassign or BpmnSla.Notify))
                errors.Add($"{step.Key}: escalation must be none, reassign or notify.");
            else if (escalation != BpmnSla.None && !a.ContainsKey("slaEscalateTo"))
                errors.Add($"{step.Key}: choose the user or role the overdue task is escalated to.");
        }

        return errors;
    }

    /// <summary>
    /// Sends the reminders and overdue alerts that fell due and escalates the overdue tasks. Run every minute by the
    /// background checker; safe to run again (each event happens once per task).
    /// </summary>
    public async Task<WfeSlaRunResult> RunAsync(CancellationToken ct)
    {
        var now = clock.UtcNow;
        string[] open = [WfeRuntime.Open, WfeRuntime.Claimed, WfeRuntime.InProgress];
        var items = await db.Set<WfeInboxItem>()
            .Include(x => x.Task)
            .Include(x => x.StepInstance)
            .Include(x => x.Instance!).ThenInclude(x => x.Definition)
            .Include(x => x.Instance!).ThenInclude(x => x.Version)
            .Where(x => open.Contains(x.InboxStatus) && x.DueAt != null
                && ((x.RemindAt != null && x.RemindedAt == null && x.RemindAt <= now)
                    || (x.OverdueNotifiedAt == null && x.DueAt <= now)
                    || (x.EscalateAt != null && x.EscalatedAt == null && x.EscalateAt <= now)))
            .OrderBy(x => x.DueAt)
            .ToListAsync(ct);
        if (items.Count == 0)
            return new WfeSlaRunResult(0, 0, 0);

        var users = await directory.ActiveUsersAsync(ct);
        int reminded = 0, overdue = 0, escalated = 0;
        foreach (var x in items)
        {
            var name = Title(x);
            if (x.RemindAt <= now && x.RemindedAt is null)
            {
                x.RemindedAt = now;
                if (x.DueAt > now)
                {
                    Alert(Holders(x, users), Reminder, x, $"{name} is due {Stamp(x.DueAt)}.", now);
                    History(x, "TASK_REMINDER", $"Reminder sent: due {Stamp(x.DueAt)}.", now);
                    reminded++;
                }
            }

            if (x.DueAt <= now && x.OverdueNotifiedAt is null)
            {
                x.OverdueNotifiedAt = now;
                Alert(Holders(x, users), Overdue, x, $"{name} is overdue. It was due {Stamp(x.DueAt)}.", now);
                History(x, "TASK_OVERDUE", $"Overdue: it was due {Stamp(x.DueAt)}.", now);
                overdue++;
            }

            if (x.EscalateAt <= now && x.EscalatedAt is null)
            {
                x.EscalatedAt = now;
                if (await EscalateAsync(x, name, users, now, ct))
                    escalated++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new WfeSlaRunResult(reminded, overdue, escalated);
    }

    private async Task<bool> EscalateAsync(WfeInboxItem x, string name, IReadOnlyList<DirectoryUser> users, DateTime now, CancellationToken ct)
    {
        var sla = x.StepInstance is { } step ? Try<BpmnSla>(() => BpmnModel.Element(WfeVersions.XmlOf(x.Instance!), step.ElementId).Sla) : null;
        if (sla is null || !sla.Escalates)
            return false;
        var byRole = !string.Equals(sla.EscalateType, "User", StringComparison.OrdinalIgnoreCase);
        var target = sla.EscalateTo!;
        if (!byRole)
            target = await WfeOutOfOfficeService.DelegateOfAsync(db, target, x.Instance?.Definition?.WorkflowCode ?? string.Empty, Gl.GlTime.Today(clock), ct) ?? target;
        var targetUsers = byRole ? users.Where(u => u.Roles.Contains(target, StringComparer.OrdinalIgnoreCase)).Select(u => u.UserName).ToList() : [target];
        var who = byRole ? $"role {target}" : target;

        if (sla.Escalation == BpmnSla.Notify)
        {
            Alert(targetUsers, Escalated, x, $"{name} is overdue (due {Stamp(x.DueAt)}). It is with {HolderText(x)}.", now);
            History(x, "TASK_ESCALATED", $"Overdue: {who} was told.", now);
            return true;
        }

        var before = Holders(x, users);
        var from = HolderText(x);
        x.EscalatedFrom = from;
        x.InboxStatus = WfeRuntime.Open;
        x.ClaimedBy = null;
        x.ClaimedAt = null;
        x.AssignmentType = byRole ? "Role" : "User";
        x.AssignedTo = byRole ? null : target;
        x.AssignedRole = byRole ? target : null;
        if (x.Task is { } task)
        {
            task.TaskStatus = WfeRuntime.Open;
            task.ClaimedBy = null;
            task.ClaimedAt = null;
            task.AssignmentType = x.AssignmentType;
            task.Assignee = target;
        }

        Alert(targetUsers, Escalated, x, $"{name} is overdue and was moved to you from {from}.", now);
        Alert(before.Except(targetUsers, StringComparer.OrdinalIgnoreCase), Escalated, x, $"{name} was overdue and was moved to {who}.", now);
        History(x, "TASK_ESCALATED", $"Overdue: moved from {from} to {who}.", now);
        return true;
    }

    private static T? Try<T>(Func<T?> f) where T : class
    {
        try
        {
            return f();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>Who works the task now: whoever claimed it, its user, or everyone in its role.</summary>
    private static List<string> Holders(WfeInboxItem x, IReadOnlyList<DirectoryUser> users)
    {
        if (x.InboxStatus is WfeRuntime.Claimed or WfeRuntime.InProgress && !string.IsNullOrWhiteSpace(x.ClaimedBy))
            return [x.ClaimedBy!];
        if (string.Equals(x.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase))
        {
            var role = (x.AssignedRole ?? x.AssignedTo)?.Trim();
            return string.IsNullOrEmpty(role) ? [] : users.Where(u => u.Roles.Contains(role, StringComparer.OrdinalIgnoreCase)).Select(u => u.UserName).ToList();
        }

        return string.IsNullOrWhiteSpace(x.AssignedTo) ? [] : [x.AssignedTo!.Trim()];
    }

    private static string HolderText(WfeInboxItem x) =>
        x.InboxStatus is WfeRuntime.Claimed or WfeRuntime.InProgress && !string.IsNullOrWhiteSpace(x.ClaimedBy) ? x.ClaimedBy!
        : string.Equals(x.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase) ? $"role {x.AssignedRole ?? x.AssignedTo}"
        : x.AssignedTo ?? "nobody";

    private static string Title(WfeInboxItem x)
    {
        var step = x.StepInstance?.StepName ?? x.Subject ?? "A task";
        var workflow = x.Instance?.Definition?.WorkflowName;
        var reference = x.Instance?.BusinessReference;
        return $"Task #{x.TaskId} \"{step}\"" + (string.IsNullOrWhiteSpace(workflow) ? string.Empty : $" of {workflow}")
            + (string.IsNullOrWhiteSpace(reference) ? string.Empty : $" ({reference})");
    }

    private static string Stamp(DateTime? utc) => Gl.GlTime.Stamp(utc, "dd-MMM-yyyy HH:mm") ?? "-";

    private void Alert(IEnumerable<string> userNames, string type, WfeInboxItem x, string message, DateTime now)
    {
        foreach (var user in userNames.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct(StringComparer.OrdinalIgnoreCase))
            db.Add(new WfeAlert
            {
                UserName = user,
                AlertType = type,
                InboxItemId = x.InboxItemId,
                WorkflowInstanceId = x.WorkflowInstanceId,
                Message = message.Length > 1000 ? message[..1000] : message,
                AlertAt = now,
            });
    }

    private void History(WfeInboxItem x, string eventType, string details, DateTime now) =>
        db.Add(new WfeInstanceHist
        {
            WorkflowInstanceId = x.WorkflowInstanceId,
            EventType = eventType,
            ElementId = x.StepInstance?.ElementId,
            PerformedBy = "SYSTEM",
            Details = details,
            EventAt = now,
        });
}

/// <summary>The signed-in user's alerts (the bell in the page header).</summary>
public sealed class WfeAlerts(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    private string? Me => currentUser.UserName;

    /// <summary>Unread alerts, then the last week's read ones, newest first (at most 30).</summary>
    public async Task<IReadOnlyList<WfeAlertDto>> MineAsync(CancellationToken ct)
    {
        var me = Me;
        if (string.IsNullOrWhiteSpace(me))
            return [];
        var since = clock.UtcNow.AddDays(-7);
        return await db.Set<WfeAlert>().AsNoTracking()
            .Where(x => x.UserName == me && (x.ReadAt == null || x.AlertAt >= since))
            .OrderBy(x => x.ReadAt != null).ThenByDescending(x => x.AlertAt).ThenByDescending(x => x.AlertId)
            .Take(30)
            .Select(x => new WfeAlertDto(x.AlertId, x.AlertType, x.Message, x.InboxItemId, x.WorkflowInstanceId, x.AlertAt, x.ReadAt != null))
            .ToListAsync(ct);
    }

    /// <summary>Marks one alert (or, with null, all of the user's alerts) as read.</summary>
    public async Task ReadAsync(long? alertId, CancellationToken ct)
    {
        var me = Me;
        if (string.IsNullOrWhiteSpace(me))
            return;
        var rows = await db.Set<WfeAlert>()
            .Where(x => x.UserName == me && x.ReadAt == null && (alertId == null || x.AlertId == alertId))
            .ToListAsync(ct);
        foreach (var r in rows)
            r.ReadAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
