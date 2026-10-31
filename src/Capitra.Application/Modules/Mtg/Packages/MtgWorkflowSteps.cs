using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Mtg;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

#pragma warning disable CA1862 // upper(si.STEP_CODE) = :code runs in the database

/// <summary>
/// The workflow task of a step page of App 105 (pages 208 to 217): what the pages read from WFE_WORKFLOW_TASK_QUEUE
/// and WFE_WF_INBOX_V, and the WFE_OPERATIONS_PKG calls they make, on the Capitra workflow engine.
/// </summary>
public sealed class MtgWorkflowSteps(ICapitraDbContext db, WfeRuntime runtime, WfeInbox inbox, ICurrentUser user)
{
    /// <summary>An open task of the portfolio: its id and status.</summary>
    public sealed record StepTask(long TaskId, string Status);

    private string Me => user.UserName ?? string.Empty;

    /// <summary>
    /// The open task of the portfolio at a step: the one the page was opened with when it is that step's, otherwise
    /// the newest (max(TASK_ID) keep (dense_rank first order by case when TASK_ID = :P_TASK_ID then 0 else 1 end, TASK_ID desc))).
    /// </summary>
    /// <remarks>With like the step code contains stepCode and not exclude; with page a task targeting that page matches too.</remarks>
    public async Task<StepTask?> OpenTaskAsync(long portfolioId, string stepCode, long? preferred, CancellationToken ct, bool like = false, string? exclude = null, int? page = null)
    {
        var instanceId = await InstanceAsync(portfolioId, ct);
        if (instanceId is null)
            return null;
        var route = page is null ? null : "mtg-" + page.Value.ToString(CultureInfo.InvariantCulture);
        var tasks = await OpenTasks(instanceId.Value)
            .Where(t => (like
                    ? t.StepInstance!.StepCode!.ToUpper().Contains(stepCode) && (exclude == null || !t.StepInstance!.StepCode!.ToUpper().Contains(exclude))
                    : t.StepInstance!.StepCode!.ToUpper() == stepCode)
                || (route != null && t.Route == route))
            .Select(t => new StepTask(t.TaskId, t.TaskStatus))
            .ToListAsync(ct);
        return tasks.OrderBy(t => t.TaskId == preferred ? 0 : 1).ThenByDescending(t => t.TaskId).FirstOrDefault();
    }

    /// <summary>The newest open task of the portfolio whose target is the page (max(TASK_ID) from WFE_WF_INBOX_V where TARGET_PAGE_ID = :page).</summary>
    public async Task<StepTask?> PageTaskAsync(long portfolioId, int page, CancellationToken ct)
    {
        var instanceId = await InstanceAsync(portfolioId, ct);
        if (instanceId is null)
            return null;
        var route = "mtg-" + page.ToString(CultureInfo.InvariantCulture);
        return await OpenTasks(instanceId.Value).Where(t => t.Route == route).OrderByDescending(t => t.TaskId)
            .Select(t => new StepTask(t.TaskId, t.TaskStatus)).FirstOrDefaultAsync(ct);
    }

    /// <summary>The open tasks of the portfolio ("12 LOAD_DATA OPEN; ..."), for the message when the step's task is missing.</summary>
    public async Task<string?> OpenTasksTextAsync(long portfolioId, CancellationToken ct)
    {
        var instanceId = await InstanceAsync(portfolioId, ct);
        if (instanceId is null)
            return null;
        var rows = await OpenTasks(instanceId.Value).OrderBy(t => t.TaskId)
            .Select(t => new { t.TaskId, t.StepInstance!.StepCode, t.TaskStatus }).ToListAsync(ct);
        return rows.Count == 0 ? null : string.Join("; ", rows.Select(r => r.TaskId.ToString(CultureInfo.InvariantCulture) + " " + r.StepCode + " " + r.TaskStatus));
    }

    /// <summary>WFE_OPERATIONS_PKG.START_TASK: the task is claimed by the user and started, unless it is in progress already.</summary>
    public async Task StartAsync(StepTask task, CancellationToken ct)
    {
        if (task.Status == WfeRuntime.InProgress)
            return;
        if (task.Status == WfeRuntime.Open)
            await runtime.ClaimAsync(task.TaskId, Me, ct);
        await runtime.StartTaskAsync(task.TaskId, Me, ct);
    }

    /// <summary>
    /// APPROVE_TASK / REJECT_TASK / COMPLETE_TASK with the comment; APPROVE falls back to COMPLETE (and COMPLETE to
    /// APPROVE) when the step does not have that action (sqlcode -20012). Returns the next open task (p_new_task_id).
    /// </summary>
    public async Task<long?> DecideAsync(long taskId, string action, string? comment, CancellationToken ct)
    {
        var task = await db.Set<WfeTask>().Include(t => t.StepInstance).FirstAsync(t => t.TaskId == taskId, ct);
        var allowed = (task.AllowedActions ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (allowed.Length > 0 && !allowed.Contains(action, StringComparer.OrdinalIgnoreCase))
            action = action == "APPROVE" ? "COMPLETE" : action == "COMPLETE" ? "APPROVE" : action;
        var variables = string.IsNullOrWhiteSpace(comment) ? null : new Dictionary<string, string?> { ["TASK_COMMENT"] = comment };
        var instanceId = task.StepInstance!.WorkflowInstanceId;
        await runtime.CompleteAsync(taskId, action, Me, variables, ct);
        return await OpenTasks(instanceId).OrderBy(t => t.TaskId).Select(t => (long?)t.TaskId).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// The action codes of the step page's task in the user's inbox (WFE_WF_INBOX_V.available_actions), the task being
    /// P_TASK_ID or the newest open task of the portfolio whose target is the page; null in the read-only view.
    /// </summary>
    public async Task<IReadOnlyList<string>?> ActionsAsync(int page, ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") == "VIEW")
            return null;
        var taskId = Long(request.Item("P" + page + "_TASK_ID"));
        if (taskId is null && Long(request.Item("P" + page + "_PORTFOLIO_ID")) is { } portfolioId && await InstanceAsync(portfolioId, ct) is { } instanceId)
        {
            var route = "mtg-" + page.ToString(CultureInfo.InvariantCulture);
            taskId = await OpenTasks(instanceId).Where(t => t.Route == route).MaxAsync(t => (long?)t.TaskId, ct);
        }

        return (await inbox.MyInboxAsync(ct)).Where(x => x.TaskId == taskId)
            .SelectMany(x => (x.AllowedActions ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(c => c.ToUpperInvariant()).Distinct().ToList();
    }

    /// <summary>
    /// The PL/SQL region "Step Actions" of the review pages (209, 211 to 215): Accept and Reject of the decision bar
    /// follow the Allowed Actions; Accept becomes "Complete &amp; continue" on a step that completes.
    /// </summary>
    public async Task<string> StepActionsAsync(
        int page, ApexRequest request, CancellationToken ct,
        string accept = "wbAccept", string reject = "wbReject", string completeEn = "Complete & continue", string completeAr = "إكمال ومتابعة")
    {
        if (await ActionsAsync(page, request, ct) is not { } codes)
            return string.Empty;
        string pos = "NONE", rej = "N";
        foreach (var code in codes)
        {
            if (code == "APPROVE")
                pos = "APPROVE";
            else if (code == "COMPLETE" && pos != "APPROVE")
                pos = "COMPLETE";
            else if (code == "REJECT")
                rej = "Y";
        }

        if (codes.Count == 0)
            return "<style>#" + accept + ",#" + reject + "{display:none!important}</style><script>(function(){var b=document.querySelector('.wb-dbar-in');if(!b)return;var s=document.createElement('span');s.className='wb-hint';s.style.color='var(--bad)';s.textContent='No workflow actions are available: this task is closed or not in your inbox.';b.appendChild(s);})();</script>\n";
        var h = rej == "N" ? "<style>#" + reject + "{display:none!important}</style>\n" : string.Empty;
        if (pos == "NONE")
            h += "<style>#" + accept + "{display:none!important}</style><script>(function(){var b=document.querySelector('.wb-dbar-in');if(!b)return;var s=document.createElement('span');s.className='wb-hint';s.style.color='var(--bad)';s.textContent='This step has no Approve or Complete action in the workflow setup.';b.appendChild(s);})();</script>\n";
        else if (pos == "COMPLETE")
            h += "<script>(function(){var b=document.getElementById('" + accept + "');if(!b)return;var s=b.querySelector('[data-finova-en]');if(!s)return;"
                + "s.setAttribute('data-finova-en','" + completeEn + "');s.setAttribute('data-finova-ar','" + completeAr + "');"
                + "s.textContent=document.documentElement.dir==='rtl'?'" + completeAr + "':'" + completeEn + "';})();</script>\n";
        return h;
    }

    private Task<long?> InstanceAsync(long portfolioId, CancellationToken ct) =>
        db.Set<MtgPurchasedPortfolio>().Where(p => p.PortfolioId == portfolioId).Select(p => p.WorkflowInstanceId).FirstOrDefaultAsync(ct);

    private IQueryable<WfeTask> OpenTasks(long instanceId) =>
        db.Set<WfeTask>().Where(t => t.StepInstance!.WorkflowInstanceId == instanceId
            && (t.TaskStatus == WfeRuntime.Open || t.TaskStatus == WfeRuntime.Claimed || t.TaskStatus == WfeRuntime.InProgress));

    private static long? Long(string? v) => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
}
