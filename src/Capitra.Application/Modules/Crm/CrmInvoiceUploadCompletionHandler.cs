using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>
/// Complete of the Invoice Upload step of DISBURSEMENT_REQUEST_WF (business reference DISB-{id}): process, pay the charges, load the
/// cheques into the wallet and post all their GL under one transaction set (see <see cref="CrmDisbursementCompletionService"/>).
/// It runs in the completion's transaction, so a refusal or a GL error leaves the task open and only the draft saved.
/// </summary>
public sealed class CrmInvoiceUploadCompletionHandler(ICapitraDbContext db, CrmDisbursementCompletionService completion)
    : IWfeTaskCompletionHandler
{
    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, CrmDisbursementProcessService.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && string.Equals(context.StepCode, CrmDisbursementProcessService.InvoiceUploadStep, StringComparison.OrdinalIgnoreCase)
        && context.Action is "COMPLETE" or "APPROVE" or "ACCEPT";

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var reference = await db.Set<WfeWorkflowInstance>().AsNoTracking().Where(x => x.WorkflowInstanceId == context.WorkflowInstanceId)
            .Select(x => x.BusinessReference).FirstOrDefaultAsync(cancellationToken);
        if (reference is null || !reference.StartsWith("DISB-", StringComparison.Ordinal)
            || !long.TryParse(reference[5..], NumberStyles.None, CultureInfo.InvariantCulture, out var disbursementId))
            throw new BusinessRuleException("The workflow is not linked to a disbursement request (business reference DISB-{id}).");
        await completion.CompleteAsync(disbursementId, cancellationToken);
    }
}
