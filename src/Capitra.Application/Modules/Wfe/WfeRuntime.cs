using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

/// <summary>
/// Runs BPMN workflows: starts an instance, walks automatic elements, stops at user tasks (task and inbox item),
/// and moves on when a task is completed with one of its actions. Ported from the FINOVA_FINANCEING engine.
/// </summary>
public sealed class WfeRuntime(
    ICapitraDbContext db,
    IClock clock,
    IEnumerable<IWfeTaskCompletionHandler> completionHandlers,
    IEnumerable<IWfeTaskClaimHandler> claimHandlers,
    IUserDirectory? directory = null)
{
    public const string Cancelled = "CANCELLED";
    public const string Open = "OPEN";
    public const string Claimed = "CLAIMED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";

    /// <summary>Starts the released, active definition with this code for one business record.</summary>
    public async Task<WfeWorkflowInstance> StartAsync(
        string workflowCode, string? businessReference, IDictionary<string, string?>? variables, string startedBy, CancellationToken cancellationToken)
    {
        var definition = await db.Set<WfeWorkflowDefinition>().FirstOrDefaultAsync(x => x.WorkflowCode == workflowCode, cancellationToken)
            ?? throw new NotFoundException($"Workflow '{workflowCode}' is not defined.");
        return await StartAsync(definition, businessReference, variables, startedBy, cancellationToken);
    }

    public async Task<WfeWorkflowInstance> StartAsync(
        long workflowDefinitionId, string? businessReference, IDictionary<string, string?>? variables, string startedBy, CancellationToken cancellationToken)
    {
        var definition = await db.Set<WfeWorkflowDefinition>().FirstOrDefaultAsync(x => x.WorkflowDefinitionId == workflowDefinitionId, cancellationToken)
            ?? throw new NotFoundException("Workflow definition not found.");
        return await StartAsync(definition, businessReference, variables, startedBy, cancellationToken);
    }

    private async Task<WfeWorkflowInstance> StartAsync(
        WfeWorkflowDefinition definition, string? businessReference, IDictionary<string, string?>? variables, string startedBy, CancellationToken cancellationToken)
    {
        if (!definition.IsActive)
            throw new BusinessRuleException("Workflow definition is inactive.");
        if (!string.Equals(definition.DefinitionStatus, "released", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Only a released workflow definition can be started.");

        // the instance runs on the latest released version, and stays on it when the workflow changes later
        var version = await WfeVersions.CurrentAsync(db, definition, startedBy, clock.UtcNow, cancellationToken);
        // the workflow's data fields: defaults filled in, types and required fields checked before anything is saved
        var fields = WfeDataFields.Of(version.BpmnXml);
        var values = WfeDataFields.ForStart(fields, variables);
        var instance = new WfeWorkflowInstance
        {
            WorkflowDefinitionId = definition.WorkflowDefinitionId,
            Version = version,
            InstanceStatus = "RUNNING",
            BusinessReference = businessReference,
            StartedAt = clock.UtcNow,
            StartedBy = startedBy,
        };
        db.Add(instance);
        await db.SaveChangesAsync(cancellationToken);

        if (values.Count > 0)
            await SetVariablesAsync(instance.WorkflowInstanceId, values, await WfeForms.AllFieldsAsync(db, version.BpmnXml, cancellationToken), cancellationToken);
        AddHistory(instance.WorkflowInstanceId, "INSTANCE_STARTED", null, null, startedBy, businessReference);
        await db.SaveChangesAsync(cancellationToken);

        await EnterAndAdvanceAsync(instance, definition.WorkflowCode, version.BpmnXml, BpmnModel.StartEvent(version.BpmnXml), startedBy, null, cancellationToken);
        return instance;
    }

    public async Task ClaimAsync(long taskId, string user, CancellationToken cancellationToken)
    {
        var task = await LoadTaskAsync(taskId, cancellationToken);
        if (task.TaskStatus != Open)
            throw new BusinessRuleException("Only an open task can be claimed.");

        task.TaskStatus = Claimed;
        task.ClaimedBy = user;
        task.ClaimedAt = clock.UtcNow;
        if (task.InboxItem is { } inbox)
        {
            inbox.InboxStatus = Claimed;
            inbox.ClaimedBy = user;
            inbox.ClaimedAt = task.ClaimedAt;
        }

        var step = task.StepInstance!;
        AddHistory(step.WorkflowInstanceId, "TASK_CLAIMED", step.ElementId, "CLAIM", user, null);

        var context = new WfeTaskContext(task.TaskId, step.WorkflowInstanceId, step.Instance!.Definition!.WorkflowCode, step.StepCode, "CLAIM", user);
        foreach (var handler in claimHandlers.Where(h => h.CanHandle(context)))
            await handler.HandleAsync(context, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>A delegate takes over a task the away user had claimed: it becomes claimed by the delegate.</summary>
    public async Task TakeOverAsync(long taskId, string user, CancellationToken cancellationToken)
    {
        var task = await LoadTaskAsync(taskId, cancellationToken);
        if (task.TaskStatus is not (Claimed or InProgress))
            throw new BusinessRuleException("Only a claimed task can be taken over.");
        var from = task.ClaimedBy;
        task.TaskStatus = Claimed;
        task.ClaimedBy = user;
        task.ClaimedAt = clock.UtcNow;
        if (task.InboxItem is { } inbox)
        {
            inbox.InboxStatus = Claimed;
            inbox.ClaimedBy = user;
            inbox.ClaimedAt = task.ClaimedAt;
        }

        AddHistory(task.StepInstance!.WorkflowInstanceId, "TASK_TAKEN_OVER", task.StepInstance.ElementId, "CLAIM", user, $"Taken over from {from} (out of office).");
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task StartTaskAsync(long taskId, string user, CancellationToken cancellationToken)
    {
        var task = await LoadTaskAsync(taskId, cancellationToken);
        if (task.TaskStatus != Claimed)
            throw new BusinessRuleException("Only a claimed task can be started.");
        if (!string.Equals(task.ClaimedBy, user, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Task is claimed by another user.");

        task.TaskStatus = InProgress;
        if (task.InboxItem is { } inbox)
            inbox.InboxStatus = InProgress;

        AddHistory(task.StepInstance!.WorkflowInstanceId, "TASK_STARTED", task.StepInstance.ElementId, "START", user, null);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Completes a task with one of its allowed actions. Module handlers run first, in the same transaction,
    /// so if one fails nothing changes and the task stays open.
    /// </summary>
    public Task CompleteAsync(long taskId, string action, string user, IDictionary<string, string?>? variables, CancellationToken cancellationToken) =>
        CompleteAsync(taskId, action, user, variables, null, cancellationToken);

    /// <param name="assignNextTo">
    /// A user id or name: the next human task of this instance goes to that user instead of the assignee in the definition
    /// (FINOVA's "assign the next task to a specific user for this workflow instance only").
    /// </param>
    public async Task CompleteAsync(
        long taskId, string action, string user, IDictionary<string, string?>? variables, string? assignNextTo, CancellationToken cancellationToken)
    {
        var own = db.CurrentTransaction is null;
        var transaction = own ? await db.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            var task = await LoadTaskAsync(taskId, cancellationToken);
            if (task.TaskStatus is not (Open or Claimed or InProgress))
                throw new BusinessRuleException("Task is already completed.");

            action = action.Trim().ToUpperInvariant();
            // claiming and starting a task are not outcomes: they never complete it
            if (action is "CLAIM" or "START")
                throw new BusinessRuleException($"Action '{action}' does not complete a task.");
            var allowed = (task.AllowedActions ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (allowed.Length > 0 && !allowed.Contains(action, StringComparer.OrdinalIgnoreCase))
                throw new BusinessRuleException($"Action '{action}' is not allowed for this task.");

            var step = task.StepInstance!;
            var instance = step.Instance!;
            var definition = instance.Definition!;
            var xml = WfeVersions.XmlOf(instance);

            if (variables is not null)
            {
                var fields = WfeDataFields.Of(xml);
                await SetVariablesAsync(
                    instance.WorkflowInstanceId, WfeDataFields.ForUpdate(fields, variables), await WfeForms.AllFieldsAsync(db, xml, cancellationToken), cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // module handlers (postings, status changes) run once per step: on a parallel approval, when it is decided
            var parallel = BpmnModel.Element(xml, step.ElementId).Parallel is { } p
                && await db.Set<WfeTask>().CountAsync(x => x.StepInstanceId == step.StepInstanceId, cancellationToken) > 1 ? p : null;
            if (parallel is null)
                await RunCompletionHandlersAsync(task.TaskId, instance.WorkflowInstanceId, definition.WorkflowCode, step.StepCode, action, user, cancellationToken);

            var now = clock.UtcNow;
            task.TaskStatus = Completed;
            task.CompletedAt = now;
            if (task.InboxItem is { } inbox)
            {
                inbox.InboxStatus = Completed;
                inbox.CompletedAt = now;
                inbox.Outcome = action;
            }

            var late = task.InboxItem?.DueAt is { } due && now > due
                ? $"Completed after its due time ({Gl.GlTime.Stamp(due, "dd-MMM-yyyy HH:mm")})."
                : null;
            AddHistory(instance.WorkflowInstanceId, "TASK_COMPLETED", step.ElementId, action, user, late);
            await db.SaveChangesAsync(cancellationToken);

            // a parallel approval moves on only when its rule is met
            var outcome = action;
            if (parallel is not null)
            {
                var decided = await DecideParallelAsync(instance.WorkflowInstanceId, step, parallel, action, user, now, cancellationToken);
                if (decided is null)
                {
                    await db.SaveChangesAsync(cancellationToken);
                    if (transaction is not null)
                        await transaction.CommitAsync(cancellationToken);
                    return;
                }

                outcome = decided;
                await RunCompletionHandlersAsync(task.TaskId, instance.WorkflowInstanceId, definition.WorkflowCode, step.StepCode, outcome, user, cancellationToken);
            }

            step.StepStatus = Completed;
            step.CompletedAt = now;
            step.CompletedBy = user;
            step.Outcome = outcome;
            await db.SaveChangesAsync(cancellationToken);

            var next = await NextAsync(instance.WorkflowInstanceId, xml, step.ElementId, outcome, user, cancellationToken);
            if (next is null)
                await CompleteInstanceAsync(instance, user, cancellationToken);
            else
                await EnterAndAdvanceAsync(instance, definition.WorkflowCode, xml, next, user, outcome, cancellationToken, assignNextTo);

            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    /// <summary>The open task of an instance at a step, if the instance is waiting there.</summary>
    public Task<WfeTask?> OpenTaskAsync(long workflowInstanceId, string stepCode, CancellationToken cancellationToken) =>
        db.Set<WfeTask>()
            .Include(x => x.StepInstance)
            .Where(x => x.StepInstance!.WorkflowInstanceId == workflowInstanceId && x.StepInstance.StepCode == stepCode
                && (x.TaskStatus == Open || x.TaskStatus == Claimed || x.TaskStatus == InProgress))
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<WfeTask> LoadTaskAsync(long taskId, CancellationToken cancellationToken) =>
        await db.Set<WfeTask>()
            .Include(x => x.InboxItem)
            .Include(x => x.StepInstance!).ThenInclude(x => x.Instance!).ThenInclude(x => x.Definition)
            .Include(x => x.StepInstance!).ThenInclude(x => x.Instance!).ThenInclude(x => x.Version)
            .FirstOrDefaultAsync(x => x.TaskId == taskId, cancellationToken)
        ?? throw new NotFoundException("Workflow task not found.");

    private async Task EnterAndAdvanceAsync(
        WfeWorkflowInstance instance, string workflowCode, string xml, BpmnElement element, string user, string? outcome, CancellationToken cancellationToken,
        string? assignNextTo = null)
    {
        // Guard against a malformed diagram looping forever through automatic elements.
        for (var hops = 0; hops < 100; hops++)
        {
            instance.CurrentElementId = element.Id;
            var step = new WfeStepInstance
            {
                WorkflowInstanceId = instance.WorkflowInstanceId,
                ElementId = element.Id,
                StepCode = element.StepCode,
                StepName = element.Name,
                ElementType = element.Type,
                StepStatus = "ENTERED",
                EnteredAt = clock.UtcNow,
            };
            db.Add(step);
            await db.SaveChangesAsync(cancellationToken);
            AddHistory(instance.WorkflowInstanceId, "STEP_ENTERED", element.Id, null, user, element.Name);

            if (element.Type == "endEvent")
            {
                step.StepStatus = Completed;
                step.CompletedAt = clock.UtcNow;
                step.CompletedBy = user;
                await CompleteInstanceAsync(instance, user, cancellationToken);
                return;
            }

            if (element.IsHumanTask)
            {
                step.StepStatus = "WAITING";
                var parameters = await ResolveRouteParametersAsync(instance.WorkflowInstanceId, element.RouteParameters, element.StepCode ?? element.Id, cancellationToken);
                instance.InstanceStatus = "WAITING";

                var toUser = !string.IsNullOrWhiteSpace(assignNextTo);
                if (!toUser && element.Parallel is { } parallel)
                {
                    // parallel approval: every approver gets their own task now
                    var approvers = await ApproversAsync(element, cancellationToken);
                    if (approvers.Count == 0)
                        throw new BusinessRuleException($"Step '{element.Name ?? element.Id}' has no approvers: add users, or give the role members.");
                    foreach (var person in approvers)
                        await CreateTaskAsync(instance, workflowCode, step, element, parameters, "User", person, user, cancellationToken);
                    AddHistory(instance.WorkflowInstanceId, "PARALLEL_STARTED", element.Id, null, user,
                        $"Sent to {string.Join(", ", approvers)}; needs {parallel.Text(approvers.Count)} to approve.");
                }
                else
                {
                    var byRole = !toUser && string.Equals(element.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase);
                    await CreateTaskAsync(instance, workflowCode, step, element, parameters,
                        toUser ? "User" : element.AssignmentType, toUser ? assignNextTo!.Trim() : element.Assignee, user, cancellationToken, byRole);
                }

                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            // Start events, gateways and automatic tasks move on at once.
            step.StepStatus = Completed;
            step.CompletedAt = clock.UtcNow;
            step.CompletedBy = user;
            await db.SaveChangesAsync(cancellationToken);

            var next = await NextAsync(instance.WorkflowInstanceId, xml, element.Id, outcome, user, cancellationToken);
            if (next is null)
            {
                await CompleteInstanceAsync(instance, user, cancellationToken);
                return;
            }

            element = next;
        }

        instance.InstanceStatus = "FAULTED";
        instance.ErrorMessage = "Automatic BPMN traversal exceeded 100 steps.";
        await db.SaveChangesAsync(cancellationToken);
        throw new BusinessRuleException(instance.ErrorMessage);
    }

    private async Task RunCompletionHandlersAsync(
        long taskId, long workflowInstanceId, string workflowCode, string? stepCode, string action, string user, CancellationToken cancellationToken)
    {
        var context = new WfeTaskContext(taskId, workflowInstanceId, workflowCode, stepCode, action, user);
        foreach (var handler in completionHandlers.Where(h => h.CanHandle(context)))
            await handler.HandleAsync(context, cancellationToken);
    }

    /// <summary>Approving actions count towards a parallel approval; REJECT counts against it; any other action (RETURN) decides at once.</summary>
    private static readonly string[] Approving = ["APPROVE", "ACCEPT", "COMPLETE"];

    /// <summary>
    /// The outcome of a parallel approval after one more decision, or null while it waits for others. When it is decided, the
    /// approvers who have not answered yet have their tasks cancelled.
    /// </summary>
    private async Task<string?> DecideParallelAsync(
        long workflowInstanceId, WfeStepInstance step, BpmnParallel parallel, string action, string user, DateTime now, CancellationToken cancellationToken)
    {
        var tasks = await db.Set<WfeTask>().Include(x => x.InboxItem).Where(x => x.StepInstanceId == step.StepInstanceId).ToListAsync(cancellationToken);
        var answers = tasks.Where(x => x.TaskStatus == Completed).Select(x => x.InboxItem?.Outcome ?? string.Empty).ToList();
        var total = tasks.Count;
        var needed = parallel.Needed(total);
        var approved = answers.Count(a => Approving.Contains(a, StringComparer.OrdinalIgnoreCase));
        var rejected = answers.Count(a => string.Equals(a, "REJECT", StringComparison.OrdinalIgnoreCase));

        string? outcome = null;
        if (!Approving.Contains(action, StringComparer.OrdinalIgnoreCase) && action != "REJECT")
            outcome = action;
        else if (approved >= needed)
            outcome = answers.First(a => Approving.Contains(a, StringComparer.OrdinalIgnoreCase)).ToUpperInvariant();
        else if (total - rejected < needed || answers.Count == total)
            outcome = "REJECT";

        var tally = $"{approved} of {total} approved, {rejected} rejected; needs {parallel.Text(total)}.";
        if (outcome is null)
        {
            AddHistory(workflowInstanceId, "PARALLEL_WAITING", step.ElementId, action, user, $"{tally} Waiting for {needed - approved} more.");
            return null;
        }

        foreach (var open in tasks.Where(x => x.TaskStatus is Open or Claimed or InProgress))
        {
            open.TaskStatus = Cancelled;
            open.CompletedAt = now;
            if (open.InboxItem is { } item)
            {
                item.InboxStatus = Cancelled;
                item.CompletedAt = now;
            }
        }

        AddHistory(workflowInstanceId, "PARALLEL_DECIDED", step.ElementId, outcome, user, $"{outcome}: {tally}");
        return outcome;
    }

    /// <summary>
    /// One task and inbox item for one assignee (a user, or a role when <paramref name="byRole"/>). A user who is out of office
    /// today is replaced by their delegate (K2's out of office); the step's time limit (SLA) gives the due times.
    /// </summary>
    private async Task CreateTaskAsync(
        WfeWorkflowInstance instance, string workflowCode, WfeStepInstance step, BpmnElement element, string? parameters,
        string? assignmentType, string? assignee, string user, CancellationToken cancellationToken, bool byRole = false)
    {
        var task = new WfeTask
        {
            StepInstanceId = step.StepInstanceId,
            TaskStatus = Open,
            AssignmentType = byRole ? element.AssignmentType : assignmentType,
            Assignee = assignee,
            Route = element.Route,
            RouteParameters = parameters,
            AllowedActions = element.Actions,
        };
        db.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        var person = byRole ? null : assignee?.Trim();
        string? cover = null;
        if (!string.IsNullOrWhiteSpace(person))
            cover = await WfeOutOfOfficeService.DelegateOfAsync(db, person, workflowCode, Gl.GlTime.Today(clock), cancellationToken);
        var inbox = new WfeInboxItem
        {
            TaskId = task.TaskId,
            WorkflowInstanceId = instance.WorkflowInstanceId,
            StepInstanceId = step.StepInstanceId,
            InboxStatus = Open,
            AssignmentType = cover is not null ? "User" : assignmentType,
            AssignedTo = cover ?? (byRole ? null : assignee),
            AssignedRole = byRole ? assignee : null,
            Subject = element.Name,
            Route = element.Route,
            RouteParameters = parameters,
            AllowedActions = element.Actions,
        };
        await WfeSla.ApplyAsync(db, inbox, element.Sla, clock.UtcNow, cancellationToken);
        db.Add(inbox);

        AddHistory(instance.WorkflowInstanceId, "TASK_CREATED", element.Id, null, user, assignee);
        if (cover is not null)
            AddHistory(instance.WorkflowInstanceId, "TASK_DELEGATED", element.Id, null, "SYSTEM", $"{person} is out of office; the task went to {cover}.");
    }

    /// <summary>The approvers of a parallel step: the users listed in its assignee, or every active member of its role.</summary>
    private async Task<List<string>> ApproversAsync(BpmnElement element, CancellationToken cancellationToken)
    {
        var names = (element.Assignee ?? string.Empty).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!string.Equals(element.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase))
            return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (directory is null)
            return [];
        var users = await directory.ActiveUsersAsync(cancellationToken);
        return users.Where(u => u.Roles.Any(r => names.Contains(r, StringComparer.OrdinalIgnoreCase)))
            .Select(u => u.UserName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// The next element, chosen by the action and by the rules on the instance's data fields; a path chosen by its conditions
    /// is written to the history, so it is clear why the request went that way.
    /// </summary>
    private async Task<BpmnElement?> NextAsync(long workflowInstanceId, string xml, string elementId, string? outcome, string user, CancellationToken cancellationToken)
    {
        var values = await db.Set<WfeInstanceVariable>().AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowInstanceId)
            .Select(x => new { x.VariableName, x.VariableValue })
            .ToListAsync(cancellationToken);
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in values)
            data[v.VariableName] = v.VariableValue;
        var (next, flow) = BpmnModel.Next(xml, elementId, outcome, data);
        if (flow is { Rules.Count: > 0 })
            AddHistory(workflowInstanceId, "PATH_CHOSEN", flow.Id, outcome, user,
                $"To {flow.TargetName}: " + string.Join(" and ", flow.Rules.Select(r => $"{r.Text} ({(data.TryGetValue(r.Field, out var a) && !string.IsNullOrEmpty(a) ? a : "empty")})")));
        return next;
    }

    private async Task CompleteInstanceAsync(WfeWorkflowInstance instance, string user, CancellationToken cancellationToken)
    {
        instance.InstanceStatus = Completed;
        instance.CompletedAt = clock.UtcNow;
        instance.CurrentElementId = null;
        AddHistory(instance.WorkflowInstanceId, "INSTANCE_COMPLETED", null, null, user, null);
        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed class RouteParameter
    {
        public string? Name { get; set; }

        public string? Value { get; set; }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Route parameters name instance variables; the task stores their values.</summary>
    private async Task<string?> ResolveRouteParametersAsync(long workflowInstanceId, string? json, string stepCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        List<RouteParameter>? parameters;
        try
        {
            parameters = JsonSerializer.Deserialize<List<RouteParameter>>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw new BusinessRuleException($"Invalid route-parameter configuration for step '{stepCode}'.");
        }

        if (parameters is null || parameters.Count == 0)
            return json;

        var values = await db.Set<WfeInstanceVariable>().AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowInstanceId)
            .ToDictionaryAsync(x => x.VariableName, x => x.VariableValue, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var parameter in parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Name))
                throw new BusinessRuleException($"A route parameter on step '{stepCode}' has no parameter name.");
            var variable = parameter.Value?.Trim();
            if (string.IsNullOrWhiteSpace(variable))
                throw new BusinessRuleException($"Route parameter '{parameter.Name}' on step '{stepCode}' has no workflow variable mapping.");
            if (!values.TryGetValue(variable, out var value))
                throw new BusinessRuleException($"Workflow variable '{variable}' required by route parameter '{parameter.Name}' on step '{stepCode}' was not found.");
            parameter.Value = value;
        }

        return JsonSerializer.Serialize(parameters);
    }

    private async Task SetVariablesAsync(
        long workflowInstanceId, IDictionary<string, string?> values, IReadOnlyList<WfeDataField> fields, CancellationToken cancellationToken)
    {
        foreach (var (name, value) in values)
        {
            var type = WfeDataFields.TypeOf(fields, name);
            var row = await db.Set<WfeInstanceVariable>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstanceId && x.VariableName == name, cancellationToken);
            if (row is null)
                db.Add(new WfeInstanceVariable { WorkflowInstanceId = workflowInstanceId, VariableName = name, VariableValue = value, VariableType = type });
            else
            {
                row.VariableValue = value;
                row.VariableType = type ?? row.VariableType;
            }
        }
    }

    private void AddHistory(long workflowInstanceId, string eventType, string? elementId, string? action, string? user, string? details) =>
        db.Add(new WfeInstanceHist
        {
            WorkflowInstanceId = workflowInstanceId,
            EventType = eventType,
            ElementId = elementId,
            ActionCode = action,
            PerformedBy = user,
            Details = details,
            EventAt = clock.UtcNow,
        });
}
