using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

public sealed record WfeOutOfOfficeDto(
    long OutOfOfficeId, string UserName, string? UserFullName, string DelegateTo, string? DelegateFullName, string? WorkflowCode,
    string? WorkflowName, DateOnly FromDate, DateOnly ToDate, bool IsActive, string? Note, string State, int RowVersion);

public sealed record SaveWfeOutOfOfficeRequest(
    string? UserName, string DelegateTo, string? WorkflowCode, DateOnly FromDate, DateOnly ToDate, bool IsActive, string? Note, int RowVersion);

public sealed record WfeUserOptionDto(string UserName, string? FullName);

public sealed record WfeWorkflowOptionDto(string WorkflowCode, string WorkflowName);

/// <summary>Someone the signed-in user works for today, and in which workflows (null: all).</summary>
public sealed record WfeCoverDto(string UserName, string? FullName, string? WorkflowCode, DateOnly ToDate);

/// <summary>
/// Out of office (K2-style): a user sets the dates they are away and who works their tasks meanwhile, for all workflows or one.
/// While the dates run, new tasks for that user go to the delegate, and the delegate also sees and can claim the tasks already
/// waiting for the user (theirs and their roles').
/// </summary>
public sealed class WfeOutOfOfficeService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, IUserDirectory directory)
{
    private const int MaxChain = 5;

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    /// <summary>My entries and the ones naming me as delegate (admin: everyone's), newest first.</summary>
    public async Task<IReadOnlyList<WfeOutOfOfficeDto>> ListAsync(bool admin, CancellationToken ct)
    {
        var q = db.Set<WfeOutOfOffice>().AsNoTracking();
        if (!admin)
        {
            var me = Me;
            q = q.Where(x => x.UserName == me || x.DelegateTo == me);
        }

        var rows = await q.OrderByDescending(x => x.FromDate).ThenByDescending(x => x.OutOfOfficeId).ToListAsync(ct);
        return await MapAsync(rows, ct);
    }

    public async Task<IReadOnlyList<WfeUserOptionDto>> UsersAsync(CancellationToken ct) =>
        (await directory.ActiveUsersAsync(ct)).Select(x => new WfeUserOptionDto(x.UserName, x.FullName)).ToList();

    public async Task<IReadOnlyList<WfeWorkflowOptionDto>> WorkflowsAsync(CancellationToken ct) =>
        await db.Set<WfeWorkflowDefinition>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.WorkflowName)
            .Select(x => new WfeWorkflowOptionDto(x.WorkflowCode, x.WorkflowName)).ToListAsync(ct);

    /// <summary>The users the signed-in user covers today.</summary>
    public async Task<IReadOnlyList<WfeCoverDto>> CoveringAsync(CancellationToken ct)
    {
        var today = GlTime.Today(clock);
        var me = Me;
        var rows = await db.Set<WfeOutOfOffice>().AsNoTracking()
            .Where(x => x.IsActive && x.DelegateTo == me && x.FromDate <= today && x.ToDate >= today).ToListAsync(ct);
        var names = await NamesAsync(ct);
        return rows.Select(x => new WfeCoverDto(x.UserName, names.GetValueOrDefault(x.UserName), x.WorkflowCode, x.ToDate)).ToList();
    }

    /// <param name="admin">The caller may set out of office for other users (workflow administrators).</param>
    public async Task<WfeOutOfOfficeDto> SaveAsync(long? id, SaveWfeOutOfOfficeRequest r, bool admin, CancellationToken ct)
    {
        var user = string.IsNullOrWhiteSpace(r.UserName) ? Me : r.UserName.Trim();
        if (!string.Equals(user, Me, StringComparison.OrdinalIgnoreCase) && !admin)
            throw new BusinessRuleException("You can set out of office only for yourself.");
        var to = (r.DelegateTo ?? string.Empty).Trim();
        var workflow = string.IsNullOrWhiteSpace(r.WorkflowCode) ? null : r.WorkflowCode.Trim().ToUpperInvariant();

        var errors = new Dictionary<string, string[]>();
        var users = await directory.ActiveUsersAsync(ct);
        var who = users.FirstOrDefault(x => string.Equals(x.UserName, user, StringComparison.OrdinalIgnoreCase));
        var del = users.FirstOrDefault(x => string.Equals(x.UserName, to, StringComparison.OrdinalIgnoreCase));
        if (who is null)
            errors["userName"] = [$"User \"{user}\" is not an active user."];
        if (to.Length == 0)
            errors["delegateTo"] = ["Choose who works the tasks while you are away."];
        else if (del is null)
            errors["delegateTo"] = [$"User \"{to}\" is not an active user."];
        else if (string.Equals(del.UserName, user, StringComparison.OrdinalIgnoreCase))
            errors["delegateTo"] = ["Choose someone else, not the user who is away."];
        if (r.ToDate < r.FromDate)
            errors["toDate"] = ["The last day is before the first day."];
        if (workflow is not null && !await db.Set<WfeWorkflowDefinition>().AnyAsync(x => x.WorkflowCode == workflow, ct))
            errors["workflowCode"] = [$"Workflow {workflow} does not exist."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
        user = who!.UserName;
        to = del!.UserName;

        // one rule per user, workflow and overlapping dates, so it is always clear who gets a task
        var clash = await db.Set<WfeOutOfOffice>().AsNoTracking()
            .Where(x => x.IsActive && x.UserName == user && x.WorkflowCode == workflow && x.OutOfOfficeId != (id ?? 0)
                && x.FromDate <= r.ToDate && x.ToDate >= r.FromDate)
            .FirstOrDefaultAsync(ct);
        if (r.IsActive && clash is not null)
            throw new ConflictException($"{user} is already out of office {D(clash.FromDate)} to {D(clash.ToDate)} for {(workflow ?? "all workflows")}. Change that entry instead.");

        WfeOutOfOffice row;
        if (id is null)
        {
            row = new WfeOutOfOffice();
            db.Add(row);
        }
        else
        {
            row = await db.Set<WfeOutOfOffice>().FirstOrDefaultAsync(x => x.OutOfOfficeId == id, ct)
                ?? throw new NotFoundException("Out of office entry not found.");
            if (!string.Equals(row.UserName, Me, StringComparison.OrdinalIgnoreCase) && !admin)
                throw new BusinessRuleException("You can change only your own out of office.");
            if (row.RowVersion != r.RowVersion)
                throw new ConflictException("The entry was changed by someone else. Reload it and try again.");
        }

        row.UserName = user;
        row.DelegateTo = to;
        row.WorkflowCode = workflow;
        row.FromDate = r.FromDate;
        row.ToDate = r.ToDate;
        row.IsActive = r.IsActive;
        row.Note = string.IsNullOrWhiteSpace(r.Note) ? null : r.Note.Trim();
        await db.SaveChangesAsync(ct);
        return (await MapAsync([row], ct))[0];
    }

    public async Task DeleteAsync(long id, bool admin, CancellationToken ct)
    {
        var row = await db.Set<WfeOutOfOffice>().FirstOrDefaultAsync(x => x.OutOfOfficeId == id, ct)
            ?? throw new NotFoundException("Out of office entry not found.");
        if (!string.Equals(row.UserName, Me, StringComparison.OrdinalIgnoreCase) && !admin)
            throw new BusinessRuleException("You can remove only your own out of office.");
        db.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Who gets a task meant for this user today: the delegate of their rule for this workflow, else of their rule for all
    /// workflows, followed when the delegate is away too (at most five steps, never back to someone already passed); null when
    /// the user is not away.
    /// </summary>
    public static async Task<string?> DelegateOfAsync(ICapitraDbContext db, string userName, string? workflowCode, DateOnly today, CancellationToken ct)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { userName };
        string? result = null;
        var current = userName;
        for (var i = 0; i < MaxChain; i++)
        {
            var rules = await db.Set<WfeOutOfOffice>().AsNoTracking()
                .Where(x => x.IsActive && x.UserName == current && x.FromDate <= today && x.ToDate >= today
                    && (x.WorkflowCode == null || x.WorkflowCode == workflowCode))
                .ToListAsync(ct);
            var rule = rules.FirstOrDefault(x => x.WorkflowCode != null) ?? rules.FirstOrDefault();
            if (rule is null || !seen.Add(rule.DelegateTo))
                break;
            result = current = rule.DelegateTo;
        }

        return result;
    }

    private async Task<Dictionary<string, string?>> NamesAsync(CancellationToken ct) =>
        (await directory.ActiveUsersAsync(ct)).ToDictionary(x => x.UserName, x => x.FullName, StringComparer.OrdinalIgnoreCase);

    private async Task<List<WfeOutOfOfficeDto>> MapAsync(List<WfeOutOfOffice> rows, CancellationToken ct)
    {
        var names = await NamesAsync(ct);
        var workflows = await db.Set<WfeWorkflowDefinition>().AsNoTracking().ToDictionaryAsync(x => x.WorkflowCode, x => x.WorkflowName, ct);
        var today = GlTime.Today(clock);
        return rows.Select(x => new WfeOutOfOfficeDto(
            x.OutOfOfficeId, x.UserName, names.GetValueOrDefault(x.UserName), x.DelegateTo, names.GetValueOrDefault(x.DelegateTo),
            x.WorkflowCode, x.WorkflowCode is null ? null : workflows.GetValueOrDefault(x.WorkflowCode),
            x.FromDate, x.ToDate, x.IsActive, x.Note,
            !x.IsActive ? "OFF" : x.ToDate < today ? "ENDED" : x.FromDate > today ? "PLANNED" : "AWAY",
            x.RowVersion)).ToList();
    }

    private static string D(DateOnly d) => d.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
}
