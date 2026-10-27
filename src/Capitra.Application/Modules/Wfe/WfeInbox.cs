using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

public sealed record WfeInboxItemDto(
    long InboxId,
    long TaskId,
    long InstanceId,
    long StepInstanceId,
    string Status,
    string? AssignmentType,
    string? AssignedTo,
    string? AssignedRole,
    string? ClaimedBy,
    string? ClaimedByName,
    bool IsClaimedByMe,
    string? Subject,
    string? Route,
    string? RouteParameters,
    string? AllowedActions,
    string? Outcome,
    DateTime CreatedOn,
    DateTime? ClaimedOn,
    DateTime? CompletedOn,
    string? WorkflowCode,
    string? WorkflowName,
    string? StepCode,
    string? StepName,
    string? ModuleCode,
    string? FunctionCode,
    string? EntityType,
    long? EntityId,
    string? EntityRef,
    long? ClientId,
    string? ClientName,
    decimal? Amount,
    string? CurrencyCode,
    string Priority,
    string? Instructions,
    DateTime? DueDate,
    string? AssignedUserId,
    string? AssignedUserName,
    string? AssignedGroup);

public sealed record CompleteInboxItemRequest(string? Action, string? Comment);

/// <summary>
/// My Inbox: the open tasks the signed-in user may work on (assigned to them, or to one of their roles),
/// and claiming, starting and completing them. The user always comes from the login token.
/// </summary>
public sealed class WfeInbox(ICapitraDbContext db, ICurrentUser currentUser, WfeRuntime runtime)
{
    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    public async Task<IReadOnlyList<WfeInboxItemDto>> MyInboxAsync(CancellationToken cancellationToken)
    {
        var rows = await Query()
            .Where(x => x.InboxStatus == WfeRuntime.Open || x.InboxStatus == WfeRuntime.Claimed || x.InboxStatus == WfeRuntime.InProgress)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.Where(CanAccess).Select(Map).ToList();
    }

    /// <summary>Every open, claimed or in-progress task of every user, for the live operations of the Workflow Engine home.</summary>
    public async Task<IReadOnlyList<WfeInboxItemDto>> AllOpenAsync(CancellationToken cancellationToken)
    {
        var rows = await Query()
            .Where(x => x.InboxStatus == WfeRuntime.Open || x.InboxStatus == WfeRuntime.Claimed || x.InboxStatus == WfeRuntime.InProgress)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<WfeInboxItemDto> GetAsync(long inboxId, CancellationToken cancellationToken)
    {
        var row = await Query().FirstOrDefaultAsync(x => x.InboxItemId == inboxId, cancellationToken)
            ?? throw new NotFoundException("Workflow inbox item not found.");
        if (!CanAccess(row))
            throw new BusinessRuleException("This task is not assigned to you.");
        return Map(row);
    }

    public async Task<WfeInboxItemDto> ClaimAsync(long inboxId, CancellationToken cancellationToken)
    {
        var row = await AccessibleAsync(inboxId, cancellationToken);
        await runtime.ClaimAsync(row.TaskId, Me, cancellationToken);
        return await GetAsync(inboxId, cancellationToken);
    }

    public async Task<WfeInboxItemDto> StartAsync(long inboxId, CancellationToken cancellationToken)
    {
        var row = await AccessibleAsync(inboxId, cancellationToken);
        await runtime.StartTaskAsync(row.TaskId, Me, cancellationToken);
        return await GetAsync(inboxId, cancellationToken);
    }

    public async Task<WfeInboxItemDto> CompleteAsync(long inboxId, CompleteInboxItemRequest request, CancellationToken cancellationToken)
    {
        var row = await AccessibleAsync(inboxId, cancellationToken);
        var action = string.IsNullOrWhiteSpace(request.Action) ? "COMPLETE" : request.Action;
        var variables = string.IsNullOrWhiteSpace(request.Comment) ? null : new Dictionary<string, string?> { ["TASK_COMMENT"] = request.Comment };
        await runtime.CompleteAsync(row.TaskId, action, Me, variables, cancellationToken);
        db.DiscardChanges();
        var done = await Query().FirstAsync(x => x.InboxItemId == inboxId, cancellationToken);
        return Map(done);
    }

    private async Task<WfeInboxItem> AccessibleAsync(long inboxId, CancellationToken cancellationToken)
    {
        var row = await db.Set<WfeInboxItem>().AsNoTracking().FirstOrDefaultAsync(x => x.InboxItemId == inboxId, cancellationToken)
            ?? throw new NotFoundException("Workflow inbox item not found.");
        if (!CanAccess(row))
            throw new BusinessRuleException("This task is not assigned to you.");
        return row;
    }

    /// <summary>A claimed task belongs to whoever claimed it; an open one to its user, or to everyone in its role.</summary>
    private bool CanAccess(WfeInboxItem x)
    {
        if (x.InboxStatus is WfeRuntime.Claimed or WfeRuntime.InProgress)
            return Same(x.ClaimedBy);
        if (string.Equals(x.AssignmentType, "Role", StringComparison.OrdinalIgnoreCase))
        {
            var role = x.AssignedRole ?? x.AssignedTo;
            return !string.IsNullOrWhiteSpace(role) && currentUser.Roles.Contains(role.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        return string.Equals(x.AssignmentType, "User", StringComparison.OrdinalIgnoreCase) && Same(x.AssignedTo);
    }

    private bool Same(string? user) =>
        !string.IsNullOrWhiteSpace(user)
        && (string.Equals(user, currentUser.UserName, StringComparison.OrdinalIgnoreCase) || string.Equals(user, currentUser.UserId, StringComparison.OrdinalIgnoreCase));

    private IQueryable<WfeInboxItem> Query() =>
        db.Set<WfeInboxItem>().AsNoTracking()
            .Include(x => x.StepInstance)
            .Include(x => x.Instance!).ThenInclude(x => x.Definition)
            .Include(x => x.Instance!).ThenInclude(x => x.Variables);

    private WfeInboxItemDto Map(WfeInboxItem x)
    {
        var vars = x.Instance?.Variables.ToDictionary(v => v.VariableName, v => v.VariableValue, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        string? V(params string[] names) =>
            names.Select(n => vars.TryGetValue(n, out var v) ? v : null).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        long? L(params string[] names) => long.TryParse(V(names), out var n) ? n : null;
        decimal? D(params string[] names) => decimal.TryParse(V(names), System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : null;
        DateTime? T(params string[] names) => DateTime.TryParse(V(names), System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;

        return new WfeInboxItemDto(
            x.InboxItemId, x.TaskId, x.WorkflowInstanceId, x.StepInstanceId, x.InboxStatus, x.AssignmentType, x.AssignedTo, x.AssignedRole,
            x.ClaimedBy, x.ClaimedBy, Same(x.ClaimedBy), x.Subject, x.Route, x.RouteParameters, x.AllowedActions, x.Outcome,
            x.CreatedAt, x.ClaimedAt, x.CompletedAt,
            x.Instance?.Definition?.WorkflowCode, x.Instance?.Definition?.WorkflowName,
            x.StepInstance?.StepCode, x.StepInstance?.StepName ?? x.Subject,
            V("MODULE_CODE"), V("FUNCTION_CODE"), V("ENTITY_TYPE"), L("ENTITY_ID"),
            V("ENTITY_REF", "BUSINESS_REFERENCE") ?? x.Instance?.BusinessReference,
            L("CLIENT_ID"), V("CLIENT_NAME"), D("AMOUNT"), V("CURRENCY_CODE"), V("PRIORITY") ?? "NORMAL", V("INSTRUCTIONS"), T("DUE_DATE"),
            x.AssignedTo, x.AssignedTo, x.AssignedRole);
    }
}
