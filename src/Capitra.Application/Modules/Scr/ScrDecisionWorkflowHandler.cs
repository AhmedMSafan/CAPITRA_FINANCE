using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// The business effect of the SCR_DECISION_APPROVAL workflow's steps, run inside the task's completion:
/// <list type="bullet">
/// <item>SCR_REVIEW ACCEPT: the decision is AWAITING_APPROVAL, reviewed by this user. REJECT: RETURNED to the entry clerk.</item>
/// <item>SCR_APPROVAL ACCEPT: the decision is PUBLISHED as the next Final List version. REJECT: RETURNED.</item>
/// </list>
/// The reviewer cannot be the person who entered the decision, and the approver neither of them (<see cref="ScrDuties"/>, by person).
/// A failed check throws, so the task stays open.
/// </summary>
public sealed class ScrDecisionWorkflowHandler(ICapitraDbContext db, IClock clock, IScrUserProfiles profiles) : IWfeTaskCompletionHandler
{
    public const string ReviewStep = "SCR_REVIEW";
    public const string ApprovalStep = "SCR_APPROVAL";

    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, ScrStaffService.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && context.StepCode is ReviewStep or ApprovalStep;

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        await ApplyAsync(context, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var d = await db.Set<ScrDecision>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == context.WorkflowInstanceId, cancellationToken)
            ?? throw new BusinessRuleException("The workflow's decision was not found.");
        var profile = await profiles.ByUserNameAsync(context.User, cancellationToken);
        var person = ScrDuties.PersonKey(profile?.EmployeeNo, context.User);
        var accept = string.Equals(context.Action, "ACCEPT", StringComparison.OrdinalIgnoreCase);
        var now = clock.UtcNow;

        if (context.StepCode == ReviewStep)
        {
            Expect(d, ScrDecisionStatus.InReview);
            if (!accept)
            {
                Return(d, $"Returned at review by {context.User}.");
                return;
            }

            ScrDuties.CheckReview(d.EnteredByPerson, person);
            d.ReviewedBy = context.User;
            d.ReviewedByPerson = person;
            d.ReviewedAt = now;
            d.StatusCode = ScrDecisionStatus.AwaitingApproval;
            return;
        }

        Expect(d, ScrDecisionStatus.AwaitingApproval);
        if (!accept)
        {
            Return(d, $"Returned at approval by {context.User}.");
            return;
        }

        ScrDuties.CheckApproval(d.EnteredByPerson, d.ReviewedByPerson, person);
        var version = (await db.Set<ScrRegistryVersion>().MaxAsync(v => (int?)v.VersionNo, cancellationToken) ?? 0) + 1;
        d.ApprovedBy = context.User;
        d.ApprovedByPerson = person;
        d.StatusCode = ScrDecisionStatus.Published;
        d.PublishedAt = now;
        d.RegistryVersionNo = version;
        // the version number is unique: a second approval at the same moment fails here and its task stays open
        db.Add(new ScrRegistryVersion { VersionNo = version, DecisionId = d.DecisionId, PublishedAt = now, PublishedBy = context.User });
    }

    private static void Expect(ScrDecision d, string status)
    {
        if (d.StatusCode != status)
            throw new BusinessRuleException($"Decision {d.DecisionRef} is {d.StatusCode.ToLowerInvariant().Replace('_', ' ')}, not {status.ToLowerInvariant().Replace('_', ' ')}.");
    }

    private static void Return(ScrDecision d, string reason)
    {
        d.StatusCode = ScrDecisionStatus.Returned;
        d.ReturnReason = reason;
    }
}
