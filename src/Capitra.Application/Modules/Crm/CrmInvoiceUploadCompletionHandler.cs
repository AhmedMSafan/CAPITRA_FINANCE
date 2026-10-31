using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>
/// Complete of the Invoice Upload step of DISBURSEMENT_REQUEST_WF (business reference DISB-{id}): every charge must be paid,
/// then the charges' GL is posted and its transaction set ID kept on the request (see <see cref="CrmDisbursementChargePaymentService"/>).
/// It runs in the completion's transaction, so a refusal or a GL error leaves the task open and nothing posted.
/// </summary>
public sealed class CrmInvoiceUploadCompletionHandler(
    ICapitraDbContext db, CrmDisbursementChargePaymentService payments, ProductActionPosting posting, AdmProductEventRuleService rules)
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
        await payments.CompleteAsync(disbursementId, posting, rules, cancellationToken);
    }
}
