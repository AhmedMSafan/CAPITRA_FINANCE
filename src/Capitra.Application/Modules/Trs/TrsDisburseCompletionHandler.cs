using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Trs;

/// <summary>
/// Complete of the Treasury Disburse step of DISBURSEMENT_REQUEST_WF (business reference DISB-{id}): the source of funds must be
/// executed first, so the bank drawdowns are recorded before the workflow ends.
/// </summary>
public sealed class TrsDisburseCompletionHandler(ICapitraDbContext db, TrsDisburseService disburse) : IWfeTaskCompletionHandler
{
    public const string TreasuryDisburseStep = "TREASURY_DISBURSE";

    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, CrmDisbursementProcessService.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && string.Equals(context.StepCode, TreasuryDisburseStep, StringComparison.OrdinalIgnoreCase)
        && context.Action is "COMPLETE" or "APPROVE" or "ACCEPT";

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var reference = await db.Set<WfeWorkflowInstance>().AsNoTracking().Where(x => x.WorkflowInstanceId == context.WorkflowInstanceId)
            .Select(x => x.BusinessReference).FirstOrDefaultAsync(cancellationToken);
        if (reference is null || !reference.StartsWith("DISB-", StringComparison.Ordinal)
            || !long.TryParse(reference[5..], NumberStyles.None, CultureInfo.InvariantCulture, out var disbursementId))
            throw new BusinessRuleException("The workflow is not linked to a disbursement request (business reference DISB-{id}).");
        if (!await disburse.IsExecutedAsync(disbursementId, cancellationToken))
            throw new BusinessRuleException("Execute the source of funds before completing this step.");
    }
}
