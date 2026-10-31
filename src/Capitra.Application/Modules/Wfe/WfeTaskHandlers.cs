namespace Capitra.Application.Modules.Wfe;

/// <summary>What a task handler is told about the task being claimed or completed.</summary>
public sealed record WfeTaskContext(long TaskId, long WorkflowInstanceId, string? WorkflowCode, string? StepCode, string Action, string User);

/// <summary>
/// A module's business effect of completing a workflow task (e.g. a portfolio moving to its next status).
/// It runs inside the completion's transaction, before the task closes, so a failure leaves the task open.
/// </summary>
public interface IWfeTaskCompletionHandler
{
    bool CanHandle(WfeTaskContext context);

    Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken);
}

/// <summary>A module's business effect of claiming a workflow task.</summary>
public interface IWfeTaskClaimHandler
{
    bool CanHandle(WfeTaskContext context);

    Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken);
}
