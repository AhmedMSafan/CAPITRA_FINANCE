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
public sealed class ScrStaffController(ScrStaffService staff) : ControllerBase
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

    [HttpPost("decisions/{decisionId:long}/cancel")]
    [HasPermission(Permissions.Scr.DecisionEdit)]
    public Task<ScrDecisionDto> Cancel(long decisionId, CancellationToken ct) => staff.CancelAsync(decisionId, ct);

    /// <summary>The Federation Final List now: every entry, active, suspended and lifted, with the version.</summary>
    [HttpGet("registry")]
    [HasPermission(Permissions.Scr.DecisionView)]
    public Task<ScrRegistryDto> Registry(CancellationToken ct) => staff.RegistryAsync(ct);
}
