using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Fct;

/// <summary>FINOVA's UploadFactoringAgreementDocumentRequest.</summary>
public sealed class FctUploadDocumentRequest
{
    public IFormFile? File { get; set; }

    public long DocTypeId { get; set; }

    public long StatusId { get; set; }
}

/// <summary>
/// The documents of a factoring agreement, uploaded from any factoring workflow step page (FINOVA
/// FactoringAgreementController documents endpoints). Reading needs FCT.VIEW, uploading FCT.EDIT.
/// </summary>
[ApiController]
[Route("api/fct")]
public sealed class FctDocumentController(FctDocumentService documents) : ControllerBase
{
    [HttpGet("document-lookups")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctDocumentLookupsDto> Lookups(CancellationToken cancellationToken) => documents.LookupsAsync(cancellationToken);

    [HttpGet("agreements/{agreementId:long}/documents")]
    [HasPermission(Permissions.Fct.View)]
    public Task<IReadOnlyList<FctAgreementDocumentDto>> List(long agreementId, CancellationToken cancellationToken) =>
        documents.ListAsync(agreementId, cancellationToken);

    [HttpPost("agreements/{agreementId:long}/documents")]
    [HasPermission(Permissions.Fct.Edit)]
    [RequestSizeLimit(FctDocumentService.MaxFileBytes + 1024 * 1024)]
    public async Task<FctAgreementDocumentDto> Upload(long agreementId, [FromForm] FctUploadDocumentRequest request, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        if (request.File is not null)
            await request.File.CopyToAsync(buffer, cancellationToken);
        return await documents.UploadAsync(
            agreementId, request.File?.FileName ?? string.Empty, request.File?.ContentType, buffer.ToArray(), request.DocTypeId, request.StatusId,
            cancellationToken);
    }

    [HttpGet("documents/{documentId:long}/content")]
    [HasPermission(Permissions.Fct.View)]
    public async Task<IActionResult> Content(long documentId, CancellationToken cancellationToken)
    {
        var file = await documents.ContentAsync(documentId, cancellationToken);
        return File(file.Content, file.MimeType);
    }
}
