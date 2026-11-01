using System.Text.Json;
using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Crm;

/// <summary>The Invoice Upload form: the forms as a JSON payload, and the invoice and cheque XLSX/CSV files.</summary>
public sealed class CrmInvoiceUploadRequest
{
    public string Payload { get; set; } = string.Empty;

    public IFormFile? InvoiceFile { get; set; }

    public IFormFile? ChequeFile { get; set; }
}

/// <summary>
/// The Invoice Upload step of DISBURSEMENT_REQUEST_WF (APEX App 148 page 705, Invoice Entry and Verification). Reading needs CRM.VIEW;
/// saving and uploading need CRM.EDIT.
/// </summary>
[ApiController]
[Route("api/crm/disbursement-requests")]
public sealed class CrmInvoiceEntryController(
    CrmInvoiceEntryService service, CrmDisbursementProcessService process, CrmDisbursementChargePaymentService payments) : ControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    [HttpGet("{disbursementId:long}/invoice-entry")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmInvoiceEntryDto> Get(long disbursementId, CancellationToken cancellationToken) => service.GetAsync(disbursementId, cancellationToken);

    [HttpPut("{disbursementId:long}/invoice-entry")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmInvoiceEntryDto> Save(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken) =>
        service.SaveAsync(disbursementId, dto, cancellationToken);

    [HttpPost("{disbursementId:long}/invoice-entry/upload")]
    [HasPermission(Permissions.Crm.Edit)]
    [RequestSizeLimit(2 * MaxUploadBytes + 1024 * 1024)]
    public async Task<CrmInvoiceUploadResultDto> Upload(
        long disbursementId, [FromForm] CrmInvoiceUploadRequest request, [FromQuery] bool preview, CancellationToken cancellationToken)
    {
        var dto = JsonSerializer.Deserialize<CrmInvoiceEntrySaveDto>(request.Payload, JsonSerializerOptions.Web)
            ?? throw new BusinessRuleException("The page values are missing.");
        return await service.UploadAsync(disbursementId, dto, await FileOf(request.InvoiceFile, cancellationToken),
            await FileOf(request.ChequeFile, cancellationToken), preview, cancellationToken);
    }

    /// <summary>The charges preview while files are chosen and not saved yet: their rows count, nothing is saved.</summary>
    [HttpPost("{disbursementId:long}/invoice-entry/charges-preview-files")]
    [HasPermission(Permissions.Crm.View)]
    [RequestSizeLimit(2 * MaxUploadBytes + 1024 * 1024)]
    public async Task<CrmDisbursementResultDto> ChargesPreviewFiles(
        long disbursementId, [FromForm] CrmInvoiceUploadRequest request, CancellationToken cancellationToken)
    {
        var dto = JsonSerializer.Deserialize<CrmInvoiceEntrySaveDto>(request.Payload, JsonSerializerOptions.Web)
            ?? throw new BusinessRuleException("The page values are missing.");
        return await process.PreviewAsync(disbursementId, dto, cancellationToken, await FileOf(request.InvoiceFile, cancellationToken),
            await FileOf(request.ChequeFile, cancellationToken));
    }

    /// <summary>Process Disbursement Request: saves the page, then calculates the charges and the repayment schedule.</summary>
    [HttpPost("{disbursementId:long}/invoice-entry/process")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmDisbursementProcessedDto> Process(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken) =>
        process.ProcessAsync(disbursementId, dto, cancellationToken);

    /// <summary>The charges of the page values as they stand, calculated and not kept (nothing is saved).</summary>
    [HttpPost("{disbursementId:long}/invoice-entry/charges-preview")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmDisbursementResultDto> ChargesPreview(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken) =>
        process.PreviewAsync(disbursementId, dto, cancellationToken);

    /// <summary>The request's processed figures, charges and current repayment schedule.</summary>
    [HttpGet("{disbursementId:long}/invoice-entry/charge-payment")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmChargePaymentDto> ChargePayment(long disbursementId, CancellationToken cancellationToken) =>
        payments.GetAsync(disbursementId, cancellationToken);

    [HttpPost("{disbursementId:long}/invoice-entry/charge-payment/sources/{repaymentSourceId:long}")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmChargePaymentDto> PayCharges(long disbursementId, long repaymentSourceId, CancellationToken cancellationToken) =>
        payments.PayAsync(disbursementId, repaymentSourceId, cancellationToken);

    [HttpPost("{disbursementId:long}/invoice-entry/charge-payment/payments/{allocationId:long}/reverse")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmChargePaymentDto> ReversePayment(long disbursementId, long allocationId, CancellationToken cancellationToken) =>
        payments.ReverseAsync(disbursementId, allocationId, cancellationToken);

    [HttpGet("{disbursementId:long}/invoice-entry/result")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmDisbursementResultDto> Result(long disbursementId, CancellationToken cancellationToken) => process.ResultAsync(disbursementId, cancellationToken);

    [HttpGet("{disbursementId:long}/invoice-entry/summary")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmInvoiceSummaryDto> Summary(long disbursementId, long? clientId, long? limitId, CancellationToken cancellationToken) =>
        service.SummaryAsync(disbursementId, clientId, limitId, cancellationToken);

    [HttpGet("clients/{clientId:long}/invoice-entry-limits")]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmOptionDto>> Limits(long clientId, CancellationToken cancellationToken) => service.LimitsAsync(clientId, cancellationToken);

    [HttpGet("limits/{limitId:long}/advance-rate")]
    [HasPermission(Permissions.Crm.View)]
    public Task<decimal?> AdvanceRate(long limitId, CancellationToken cancellationToken) => service.LimitAdvanceRateAsync(limitId, cancellationToken);

    private static async Task<CrmUploadFile?> FileOf(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > MaxUploadBytes) throw new BusinessRuleException($"{file.FileName} is larger than 20 MB.");
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        return new CrmUploadFile(file.FileName, file.ContentType, buffer.ToArray());
    }
}
