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
    IEnumerable<IWfeTaskClaimHandler> claimHandlers)
{
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

        var instance = new WfeWorkflowInstance
        {
            WorkflowDefinitionId = definition.WorkflowDefinitionId,
            InstanceStatus = "RUNNING",
            BusinessReference = businessReference,
            StartedAt = clock.UtcNow,
            StartedBy = startedBy,
        };
        db.Add(instance);
        await db.SaveChangesAsync(cancellationToken);

        if (variables is not null)
            await SetVariablesAsync(instance.WorkflowInstanceId, variables, cancellationToken);
        AddHistory(instance.WorkflowInstanceId, "INSTANCE_STARTED", null, null, startedBy, businessReference);
        await db.SaveChangesAsync(cancellationToken);

        await EnterAndAdvanceAsync(instance, definition, BpmnModel.StartEvent(definition.BpmnXml), startedBy, null, cancellationToken);
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

            if (variables is not null)
            {
                await SetVariablesAsync(instance.WorkflowInstanceId, variables, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            var context = new WfeTaskContext(task.TaskId, instance.WorkflowInstanceId, definition.WorkflowCode, step.StepCode, action, user);
            foreach (var handler in completionHandlers.Where(h => h.CanHandle(context)))
                await handler.HandleAsync(context, cancellationToken);

            var now = clock.UtcNow;
            task.TaskStatus = Completed;
            task.CompletedAt = now;
            if (task.InboxItem is { } inbox)
            {
                inbox.InboxStatus = Completed;
                inbox.CompletedAt = now;
                inbox.Outcome = action;
            }

            step.StepStatus = Completed;
            step.CompletedAt = now;
            step.CompletedBy = user;
            step.Outcome = action;
            AddHistory(instance.WorkflowInstanceId, "TASK_COMPLETED", step.ElementId, action, user, null);
            await db.SaveChangesAsync(cancellationToken);

            var next = BpmnModel.Next(definition.BpmnXml, step.ElementId, action);
            if (next is null)
                await CompleteInstanceAsync(instance, user, cancellationToken);
            else
                await EnterAndAdvanceAsync(instance, definition, next, user, action, cancellationToken, assignNextTo);

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
            .FirstOrDefaultAsync(x => x.TaskId == taskId, cancellationToken)
        ?? throw new NotFoundException("Workflow task not found.");

    private async Task EnterAndAdvanceAsync(
        WfeWorkflowInstance instance, WfeWorkflowDefinition definition, BpmnElement element, string user, string? outcome, CancellationToken cancellationToken,
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
                var task = new WfeTask
                {
                    StepInstanceId = step.StepInstanceId,
                    TaskStatus = Open,
                    AssignmentType = element.AssignmentType,
                    Assignee = element.Assignee,
                    Route = element.Route,
                    RouteParameters = parameters,
                    AllowedActions = element.Actions,
                };
                db.Add(task);
                await db.SaveChangesAsync(cancellationToken);

                var toUser = !string.IsNullOrWhiteSpace(assignNextTo);
                var byRole = !toUser && string.Equals(element.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase);
                db.Add(new WfeInboxItem
                {
                    TaskId = task.TaskId,
                    WorkflowInstanceId = instance.WorkflowInstanceId,
                    StepInstanceId = step.StepInstanceId,
                    InboxStatus = Open,
                    AssignmentType = toUser ? "User" : element.AssignmentType,
                    AssignedTo = toUser ? assignNextTo!.Trim() : byRole ? null : element.Assignee,
                    AssignedRole = byRole ? element.Assignee : null,
                    Subject = element.Name,
                    Route = element.Route,
                    RouteParameters = parameters,
                    AllowedActions = element.Actions,
                });

                instance.InstanceStatus = "WAITING";
                AddHistory(instance.WorkflowInstanceId, "TASK_CREATED", element.Id, null, user, toUser ? assignNextTo!.Trim() : element.Assignee);
                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            // Start events, gateways and automatic tasks move on at once.
            step.StepStatus = Completed;
            step.CompletedAt = clock.UtcNow;
            step.CompletedBy = user;
            await db.SaveChangesAsync(cancellationToken);

            var next = BpmnModel.Next(definition.BpmnXml, element.Id, outcome);
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

    private async Task SetVariablesAsync(long workflowInstanceId, IDictionary<string, string?> values, CancellationToken cancellationToken)
    {
        foreach (var (name, value) in values)
        {
            var row = await db.Set<WfeInstanceVariable>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstanceId && x.VariableName == name, cancellationToken);
            if (row is null)
                db.Add(new WfeInstanceVariable { WorkflowInstanceId = workflowInstanceId, VariableName = name, VariableValue = value });
            else
                row.VariableValue = value;
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
