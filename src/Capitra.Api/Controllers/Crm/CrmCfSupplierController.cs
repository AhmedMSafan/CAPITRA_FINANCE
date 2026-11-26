using Capitra.Api.Security;
using Capitra.Application.Modules.Crm.CfSuppliers;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Crm;

/// <summary>
/// Consumer Finance suppliers: Sales enters and submits applications (CRM.CF_SUPPLIER.EDIT); the CF_SUPPLIER_APPROVAL step roles read
/// them (CRM.CF_SUPPLIER.VIEW) and act on the step whose task they hold, from the CAPITRA inbox.
/// </summary>
[ApiController]
[Route("api/crm/cf-suppliers")]
public sealed class CrmCfSupplierController(CrmCfSupplierService service) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<IReadOnlyList<CrmCfSupplierListItemDto>> List([FromQuery] string? status, CancellationToken ct) => service.ListAsync(status, ct);

    [HttpGet("options")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<CrmCfSupplierOptionsDto> Options(CancellationToken ct) => service.OptionsAsync(ct);

    [HttpGet("clients")]
    [HasPermission(Permissions.Crm.CfSupplierEdit)]
    public Task<IReadOnlyList<CrmCfClientOptionDto>> Clients([FromQuery] string? search, CancellationToken ct) => service.ClientsAsync(search, ct);

    /// <summary>The customer's supplier application, for its Customer 360 tab: { applicationId } (null when it has none yet).</summary>
    [HttpGet("by-client/{clientId:long}")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public async Task<object> ByClient(long clientId, CancellationToken ct) => new { applicationId = await service.ApplicationOfClientAsync(clientId, ct) };

    [HttpGet("{applicationId:long}")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<CrmCfSupplierDto> Get(long applicationId, CancellationToken ct) => service.GetAsync(applicationId, ct);

    [HttpPost]
    [HasPermission(Permissions.Crm.CfSupplierEdit)]
    public Task<CrmCfSupplierDto> Create(CrmCfSupplierRequest request, CancellationToken ct) => service.CreateAsync(request, ct);

    [HttpPut("{applicationId:long}")]
    [HasPermission(Permissions.Crm.CfSupplierEdit)]
    public Task<CrmCfSupplierDto> Update(long applicationId, CrmCfSupplierRequest request, CancellationToken ct) => service.UpdateAsync(applicationId, request, ct);

    /// <summary>Changes an active supplier's terms without a new approval, with a reason (CRM.CF_SUPPLIER.AMEND).</summary>
    [HttpPost("{applicationId:long}/amend")]
    [HasPermission(Permissions.Crm.CfSupplierAmend)]
    public Task<CrmCfSupplierDto> Amend(long applicationId, CrmCfAmendRequest request, CancellationToken ct) => service.AmendAsync(applicationId, request, ct);

    [HttpPost("{applicationId:long}/submit")]
    [HasPermission(Permissions.Crm.CfSupplierEdit)]
    public Task<CrmCfSupplierDto> Submit(long applicationId, CancellationToken ct) => service.SubmitAsync(applicationId, ct);

    /// <summary>Every step of the application with its decision; completed steps are view-only.</summary>
    [HttpGet("{applicationId:long}/steps")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<IReadOnlyList<CrmCfStepViewDto>> Steps(long applicationId, CancellationToken ct) => service.StepsAsync(applicationId, ct);

    /// <summary>Site visit step: the visit date (the user's own task).</summary>
    [HttpPost("{applicationId:long}/steps/{inboxId:long}/site-visit")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<CrmCfSupplierDto> SiteVisit(long applicationId, long inboxId, CrmCfSiteVisitRequest request, CancellationToken ct) =>
        service.SaveSiteVisitAsync(applicationId, inboxId, request, ct);

    /// <summary>Contract step: the bank account the supplier is paid to (the user's own task).</summary>
    [HttpPost("{applicationId:long}/steps/{inboxId:long}/bank-account")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<CrmCfSupplierDto> BankAccount(long applicationId, long inboxId, [FromQuery] long? bankAccountId, CancellationToken ct) =>
        service.SaveBankAccountAsync(applicationId, inboxId, bankAccountId, ct);

    /// <summary>Completes the user's workflow task on the application (ACCEPT, or REJECT to return it) with its comment.</summary>
    [HttpPost("{applicationId:long}/steps/{inboxId:long}/complete")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<CrmCfSupplierDto> CompleteStep(long applicationId, long inboxId, CrmCfCompleteStepRequest request, CancellationToken ct) =>
        service.CompleteAsync(applicationId, inboxId, request, ct);

    /// <summary>The application's documents (one list, added at any time).</summary>
    [HttpGet("{applicationId:long}/documents")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task<IReadOnlyList<CrmCfDocumentDto>> Documents(long applicationId, CancellationToken ct) => service.DocumentsAsync(applicationId, ct);

    /// <summary>Adds a document to the application, at any time: before submit, at any step, or after activation.</summary>
    [HttpPost("{applicationId:long}/documents")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    [RequestSizeLimit(CrmCfSupplierService.MaxFileBytes + 1024 * 1024)]
    public async Task<CrmCfDocumentDto> AddDocument(long applicationId, IFormFile file, [FromForm] string? title, [FromForm] string? documentTypeCode, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        return await service.AddDocumentAsync(applicationId, documentTypeCode, title, file.FileName, file.ContentType, buffer.ToArray(), ct);
    }

    /// <summary>Removes a document the user added, while the supplier is not active (it is kept, marked inactive).</summary>
    [HttpPost("documents/{documentId:long}/remove")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public Task RemoveDocument(long documentId, CancellationToken ct) => service.RemoveDocumentAsync(documentId, ct);

    /// <summary>An application document, shown in the browser.</summary>
    [HttpGet("documents/{documentId:long}")]
    [HasPermission(Permissions.Crm.CfSupplierView)]
    public async Task<IActionResult> Document(long documentId, CancellationToken ct)
    {
        var f = await service.DocumentFileAsync(documentId, ct);
        // filename* (RFC 5987) keeps Arabic file names
        Response.Headers.ContentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = f.FileName }.ToString();
        Response.Headers["X-Content-SHA256"] = f.Sha256Hash;
        return File(f.Content, f.MimeType);
    }
}
