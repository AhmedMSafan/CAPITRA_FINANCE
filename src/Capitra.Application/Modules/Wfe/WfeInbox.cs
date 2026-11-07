using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Mtg;
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
    string? AssignedGroup,
    string? ClientNumber = null);

/// <summary>RoutingMode USER sends the next task to AssignedUserId instead of the workflow default (FINOVA's routing override).</summary>
public sealed record CompleteInboxItemRequest(string? Action, string? Comment, string? RoutingMode = null, string? AssignedUserId = null);

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
        return await WithEntityAsync(rows.Where(CanAccess).Select(Map).ToList(), cancellationToken);
    }

    /// <summary>Every open, claimed or in-progress task of every user, for the live operations of the Workflow Engine home.</summary>
    public async Task<IReadOnlyList<WfeInboxItemDto>> AllOpenAsync(CancellationToken cancellationToken)
    {
        var rows = await Query()
            .Where(x => x.InboxStatus == WfeRuntime.Open || x.InboxStatus == WfeRuntime.Claimed || x.InboxStatus == WfeRuntime.InProgress)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return await WithEntityAsync(rows.Select(Map).ToList(), cancellationToken);
    }

    public async Task<WfeInboxItemDto> GetAsync(long inboxId, CancellationToken cancellationToken)
    {
        var row = await Query().FirstOrDefaultAsync(x => x.InboxItemId == inboxId, cancellationToken)
            ?? throw new NotFoundException("Workflow inbox item not found.");
        if (!CanAccess(row))
            throw new BusinessRuleException("This task is not assigned to you.");
        return (await WithEntityAsync([Map(row)], cancellationToken))[0];
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
        var toUser = string.Equals(request.RoutingMode, "USER", StringComparison.OrdinalIgnoreCase);
        if (toUser && string.IsNullOrWhiteSpace(request.AssignedUserId))
            throw new BusinessRuleException("Select a user or turn off Assign to a specific user.");
        await runtime.CompleteAsync(row.TaskId, action, Me, variables, toUser ? request.AssignedUserId : null, cancellationToken);
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

    /// <summary>
    /// A purchased-portfolio task shows its portfolio's current figures: the developer as the customer
    /// (name and code), the face value as the amount and the portfolio's currency, where the workflow variables have none.
    /// </summary>
    private async Task<List<WfeInboxItemDto>> WithEntityAsync(List<WfeInboxItemDto> items, CancellationToken cancellationToken)
    {
        var ids = items.Where(i => i.EntityType == "PURCHASED_PORTFOLIO" && i.EntityId != null).Select(i => i.EntityId!.Value).Distinct().ToList();
        if (ids.Count == 0)
            return items;
        var portfolios = await db.Set<MtgPurchasedPortfolio>()
            .Where(p => ids.Contains(p.PortfolioId))
            .Join(db.Set<MtgDeveloper>(), p => p.DeveloperId, d => d.DeveloperId,
                (p, d) => new { p.PortfolioId, p.FaceValue, p.CurrencyCode, d.DeveloperId, d.DeveloperCode, d.LegalNameEn, d.LegalNameAr })
            .ToDictionaryAsync(x => x.PortfolioId, cancellationToken);
        return items.Select(i => i.EntityType == "PURCHASED_PORTFOLIO" && i.EntityId is { } id && portfolios.TryGetValue(id, out var p)
            ? i with
            {
                ClientId = i.ClientId ?? p.DeveloperId,
                ClientName = i.ClientName ?? (string.IsNullOrWhiteSpace(p.LegalNameEn) ? p.LegalNameAr : p.LegalNameEn),
                ClientNumber = i.ClientNumber ?? p.DeveloperCode,
                Amount = i.Amount ?? (p.FaceValue > 0 ? p.FaceValue : null),
                CurrencyCode = i.CurrencyCode ?? p.CurrencyCode,
            }
            : i).ToList();
    }

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
