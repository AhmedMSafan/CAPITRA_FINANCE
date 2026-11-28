using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// One step of a consumer finance as the steps strip shows it: DONE (who, when, outcome, the GL tran sets it posted; opens view only),
/// OPEN (the step waiting now, done from the Inbox by its role) or LATER (not reached).
/// </summary>
public sealed record LndCfStepViewDto(string StepCode, string StepName, string? Role, string Status, string? Outcome, string? DoneBy, DateTime? DoneAt,
    IReadOnlyList<string> GlTranSetIds);

/// <summary>
/// The steps of a consumer finance in order (Ahmed, 28 Nov 2026), the factoring standard: completed steps open view only with their GL
/// Tran Set IDs; only the step waiting now is worked on, from the Inbox.
/// </summary>
public sealed class LndCfStepsService(ICapitraDbContext db, LndCfChargesPlanner planner)
{
    public const string Done = "DONE";
    public const string Open = "OPEN";
    public const string Later = "LATER";

    private static readonly string[] Order =
        [LndCfFinanceRules.Credit, LndCfFinanceRules.Charges, LndCfFinanceRules.Schedule, LndCfFinanceRules.Delivery, LndCfFinanceRules.SupplierPayment];

    public async Task<IReadOnlyList<LndCfStepViewDto>> StepsAsync(long loanId, CancellationToken ct)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct);
        return loan is null ? [] : await StepsAsync(loan, ct);
    }

    public async Task<IReadOnlyList<LndCfStepViewDto>> StepsAsync(LndLoan loan, CancellationToken ct)
    {
        if (loan.WorkflowInstanceId is not { } run) return [];
        var xml = await WfeVersions.XmlOfInstanceAsync(db, run, ct);
        var steps = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => s.WorkflowInstanceId == run && s.StepCode != null && s.StepCode != "" && s.ElementType != "startEvent" && s.ElementType != "endEvent")
            .OrderBy(s => s.StepInstanceId).Select(s => new { s.StepCode, s.StepStatus, s.CompletedAt, s.CompletedBy, s.Outcome }).ToListAsync(ct);
        var claimStep = await planner.ClaimStepAsync(loan, ct);
        var list = new List<LndCfStepViewDto>
        {
            new("CF_NEW", "New Finance", "SALES_OFFICER", Done, "SUBMIT", loan.CreatedBy, loan.CreatedAt, []),
        };
        var stopped = false;
        foreach (var code in Order.Where(c => xml is null || BpmnModel.StepByCode(xml, c) is not null || steps.Any(s => s.StepCode == c)))
        {
            var last = steps.LastOrDefault(s => string.Equals(s.StepCode, code, StringComparison.OrdinalIgnoreCase));
            var role = xml is null ? null : BpmnModel.StepByCode(xml, code)?.Assignee;
            if (last is null || stopped)
            {
                list.Add(new LndCfStepViewDto(code, LndCfFinanceRules.StepName(code), role, Later, null, null, null, []));
                continue;
            }
            var done = last.StepStatus == WfeRuntime.Completed;
            if (done && string.Equals(last.Outcome, "REJECT", StringComparison.OrdinalIgnoreCase)) stopped = true;
            list.Add(new LndCfStepViewDto(code, LndCfFinanceRules.StepName(code), role, done ? Done : Open, done ? last.Outcome : null,
                done ? last.CompletedBy : null, done ? last.CompletedAt : null, done ? GlOf(loan, code, claimStep) : []));
        }
        return list;
    }

    /// <summary>The GL tran sets a step posted on the finance (the claim at the step that creates it, the booking, the supplier payment).</summary>
    public static IReadOnlyList<string> GlOf(LndLoan loan, string step, string claimStep)
    {
        string? gl = step switch
        {
            _ when step == claimStep => loan.ClaimGlTranSetId,
            LndCfFinanceRules.Delivery => loan.DeliveryGlTranSetId,
            LndCfFinanceRules.SupplierPayment => loan.SupplierPaymentGlTranSetId,
            _ => null,
        };
        return gl is null ? [] : [gl];
    }
}
