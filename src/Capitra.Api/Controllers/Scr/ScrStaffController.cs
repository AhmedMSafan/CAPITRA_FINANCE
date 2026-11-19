using Capitra.Api.Security;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

/// <summary>
/// Screening, federation side: letters (SCR.LETTER.EDIT), parties and decisions (SCR.DECISION.EDIT), reading all of it and the
/// Final List (SCR.DECISION.VIEW). Review and approval are workflow steps, done from the CAPITRA inbox.
/// </summary>
[ApiController]
[Route("api/scr")]
public sealed class ScrStaffController(ScrStaffService staff, ScrStepService steps) : ControllerBase
{
    [HttpGet("letters")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<IReadOnlyList<ScrLetterDto>> Letters(string? search, CancellationToken ct) => staff.LettersAsync(search, ct);

    [HttpPost("letters")]
    [HasPermission(Permissions.Scr.LetterEdit)]
    public Task<ScrLetterDto> CreateLetter(ScrLetterRequest request, CancellationToken ct) => staff.CreateLetterAsync(request, ct);

    [HttpPost("letters/{letterId:long}/files")]
    [HasPermission(Permissions.Scr.LetterEdit)]
    [RequestSizeLimit(ScrStaffService.MaxFileBytes + 1024 * 1024)]
    public async Task<ScrLetterDto> AddFile(long letterId, IFormFile file, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        return await staff.AddLetterFileAsync(letterId, file.FileName, file.ContentType, buffer.ToArray(), ct);
    }

    /// <summary>A letter's scan, shown in the browser (PDF or image).</summary>
    [HttpGet("letters/files/{letterFileId:long}")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public async Task<IActionResult> LetterFile(long letterFileId, CancellationToken ct)
    {
        var f = await staff.LetterFileAsync(letterFileId, ct);
        Response.Headers.ContentDisposition = $"inline; filename=\"{f.FileName.Replace("\"", string.Empty, StringComparison.Ordinal)}\"";
        Response.Headers["X-Content-SHA256"] = f.Sha256Hash;
        return File(f.FileBlob, f.MimeType);
    }

    [HttpGet("parties")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<IReadOnlyList<ScrPartyDto>> Parties(string? search, CancellationToken ct) => staff.PartiesAsync(search, ct);

    [HttpPost("parties")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrPartyDto> CreateParty(ScrPartyRequest request, CancellationToken ct) => staff.SavePartyAsync(null, request, ct);

    [HttpPut("parties/{partyId:long}")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrPartyDto> UpdateParty(long partyId, ScrPartyRequest request, CancellationToken ct) => staff.SavePartyAsync(partyId, request, ct);

    [HttpGet("decisions")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<IReadOnlyList<ScrDecisionDto>> Decisions(string? status, CancellationToken ct) => staff.DecisionsAsync(status, ct);

    [HttpGet("decisions/{decisionId:long}")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<ScrDecisionDto> Decision(long decisionId, CancellationToken ct) => staff.DecisionAsync(decisionId, ct);

    [HttpPost("decisions")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> CreateDecision(ScrDecisionRequest request, CancellationToken ct) => staff.SaveDecisionAsync(null, request, ct);

    [HttpPut("decisions/{decisionId:long}")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> UpdateDecision(long decisionId, ScrDecisionRequest request, CancellationToken ct) =>
        staff.SaveDecisionAsync(decisionId, request, ct);

    /// <summary>Sends the decision for review: starts the SCR_DECISION_APPROVAL workflow.</summary>
    [HttpPost("decisions/{decisionId:long}/submit")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> Submit(long decisionId, CancellationToken ct) => staff.SubmitAsync(decisionId, ct);

    [HttpPost("decisions/{decisionId:long}/run")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> Run(long decisionId, CancellationToken ct) => staff.SubmitAsync(decisionId, ct);

    /// <summary>Every step of the decision with its documents; completed steps are view-only.</summary>
    [HttpGet("decisions/{decisionId:long}/steps")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<IReadOnlyList<ScrStepViewDto>> Steps(long decisionId, CancellationToken ct) => steps.StepsAsync(decisionId, ct);

    /// <summary>Completes the user's workflow task on the decision (ACCEPT or REJECT) with its comment.</summary>
    [HttpPost("decisions/{decisionId:long}/steps/{inboxId:long}/complete")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public async Task<ScrDecisionDto> CompleteStep(long decisionId, long inboxId, ScrCompleteStepRequest request, CancellationToken ct)
    {
        await steps.CompleteAsync(decisionId, inboxId, request, ct);
        return await staff.DecisionAsync(decisionId, ct);
    }

    /// <summary>Adds a file at data entry (draft or returned decision).</summary>
    [HttpPost("decisions/{decisionId:long}/documents")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    [RequestSizeLimit(ScrStepService.MaxFileBytes + 1024 * 1024)]
    public async Task<ScrDocumentDto> AddDocument(long decisionId, IFormFile file, [FromForm] string? title, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        return await steps.AddDataEntryDocumentAsync(decisionId, title, file.FileName, file.ContentType, buffer.ToArray(), ct);
    }

    /// <summary>Adds a file at the workflow step of the user's task.</summary>
    [HttpPost("decisions/{decisionId:long}/steps/{inboxId:long}/documents")]
    [HasPermission(Permissions.Scr.DecisionView)]
    [RequestSizeLimit(ScrStepService.MaxFileBytes + 1024 * 1024)]
    public async Task<ScrDocumentDto> AddStepDocument(long decisionId, long inboxId, IFormFile file, [FromForm] string? title, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        return await steps.AddStepDocumentAsync(decisionId, inboxId, title, file.FileName, file.ContentType, buffer.ToArray(), ct);
    }

    /// <summary>Removes a file the user added while its step is open (it is kept, marked inactive).</summary>
    [HttpPost("decisions/documents/{documentId:long}/remove")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task RemoveDocument(long documentId, CancellationToken ct) => steps.RemoveDocumentAsync(documentId, ct);

    /// <summary>A decision document, shown in the browser.</summary>
    [HttpGet("decisions/documents/{documentId:long}")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public async Task<IActionResult> Document(long documentId, CancellationToken ct)
    {
        var f = await steps.DocumentFileAsync(documentId, ct);
        // filename* (RFC 5987) keeps Arabic file names
        Response.Headers.ContentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = f.FileName }.ToString();
        Response.Headers["X-Content-SHA256"] = f.Sha256Hash;
        return File(f.Content, f.MimeType);
    }

    [HttpPost("decisions/{decisionId:long}/cancel")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> Cancel(long decisionId, CancellationToken ct) => staff.CancelAsync(decisionId, ct);

    /// <summary>The Federation Final List now: every entry, active, suspended and lifted, with the version.</summary>
    [HttpGet("registry")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<ScrRegistryDto> Registry(CancellationToken ct) => staff.RegistryAsync(ct);
}
