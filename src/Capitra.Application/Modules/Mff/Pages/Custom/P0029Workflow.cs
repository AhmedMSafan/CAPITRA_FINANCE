using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>
/// App 110 page 29, After Submit "Apply Workflow Transition" (APPLY_TRANSITION), "Assign Work" (ASSIGN_WORK),
/// "Complete Task" (COMPLETE_TASK) and "Load Application" (LOAD_APPLICATION).
/// </summary>
public sealed class P0029Workflow(ICapitraDbContext db, MffControlPackage control, MffWorkflowPackage workflow, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 29;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("APPLY_TRANSITION" or "ASSIGN_WORK" or "COMPLETE_TASK" or "LOAD_APPLICATION"))
            return Task.FromResult<object>(new { ok = true });
        var appId = Long(request, "P29_APPLICATION_ID");
        return MffForm.RunAsync(db, async () =>
        {
            switch (req)
            {
                case "APPLY_TRANSITION":
                {
                    var target = Text(request, "P29_TARGET_STATUS") ?? throw new ApexProcessException("ORA-20042: Select a target status.");
                    await control.ChangeStatusCheckedAsync(appId ?? 0, target, Text(request, "P29_ROLE_CODE"), user.UserName, Text(request, "P29_REMARKS"), ct);
                    var (status, owner) = await CurrentAsync(appId, ct) ?? throw new ApexProcessException("ORA-01403: no data found");
                    return ("Status changed successfully.", new Dictionary<string, string?> { ["P29_CURRENT_STATUS"] = status, ["P29_CURRENT_OWNER"] = owner });
                }

                case "ASSIGN_WORK":
                {
                    var to = Text(request, "P29_ASSIGNED_TO") ?? throw new ApexProcessException("ORA-20043: Assigned To is required.");
                    var id = await workflow.AssignWorkAsync(appId ?? 0, Text(request, "P29_ASSIGNMENT_TYPE") ?? "REVIEW", to, user.UserName,
                        Date(request, "P29_DUE_DATE"), Text(request, "P29_ASSIGN_NOTES"), ct);
                    return ("Work assigned. Assignment ID: " + Id(id), new Dictionary<string, string?> { ["P29_ASSIGNMENT_ID"] = Id(id), ["P29_CURRENT_OWNER"] = to });
                }

                case "COMPLETE_TASK":
                {
                    var id = Long(request, "P29_ASSIGNMENT_ID") ?? throw new ApexProcessException("ORA-20044: Select an open assignment.");
                    await workflow.CompleteAssignmentAsync(id, Text(request, "P29_ASSIGN_NOTES"), ct);
                    return ("Task completed.", null);
                }

                default:
                {
                    var cur = await CurrentAsync(appId, ct);
                    return (string.Empty, new Dictionary<string, string?> { ["P29_CURRENT_STATUS"] = cur?.Status, ["P29_CURRENT_OWNER"] = cur?.Owner });
                }
            }
        }, ct);
    }

    private async Task<(string Status, string Owner)?> CurrentAsync(long? appId, CancellationToken ct)
    {
        var a = await db.Set<MffApplication>().AsNoTracking().Where(x => x.ApplicationId == appId)
            .Select(x => new { x.StatusCode, x.CurrentOwnerUser }).FirstOrDefaultAsync(ct);
        return a is null ? null : (a.StatusCode, a.CurrentOwnerUser ?? "—");
    }
}
