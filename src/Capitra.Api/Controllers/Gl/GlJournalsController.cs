using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>Journal entry (APEX App 120 page 50) and the journals list with its bulk actions (page 51).</summary>
[ApiController]
[Route("api/gl/journals")]
[HasPermission(Permissions.Gl.JournalView)]
public sealed class GlJournalsController(IGlJournalService service, IGlJournalQueryService queries) : ControllerBase
{
    /// <summary>Segment filters arrive as "3:1101;1:01": segment number and value.</summary>
    internal static Dictionary<int, string?>? ParseSegments(string? segments) =>
        string.IsNullOrWhiteSpace(segments)
            ? null
            : segments.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Split(':', 2))
                .Where(x => x.Length == 2 && int.TryParse(x[0], out _))
                .GroupBy(x => int.Parse(x[0]))
                .ToDictionary(g => g.Key, g => (string?)g.Last()[1]);

    [HttpGet]
    public Task<JournalSearchResultDto> Search(
        [FromQuery] string? stage, [FromQuery] string? q, [FromQuery] string? tran, [FromQuery] long? ledgerId,
        [FromQuery] string? source, [FromQuery] string? category, [FromQuery] string? period,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] decimal? minAmount, [FromQuery] decimal? maxAmount,
        [FromQuery] string? createdBy, [FromQuery] string? segments, CancellationToken cancellationToken) =>
        queries.SearchAsync(new JournalSearchRequest(stage, q, tran, ledgerId, source, category, period, from, to,
            minAmount, maxAmount, createdBy, ParseSegments(segments)), cancellationToken);

    [HttpGet("setup")]
    public Task<JournalEntrySetupDto> Setup(CancellationToken cancellationToken) => service.GetSetupAsync(cancellationToken);

    [HttpGet("{journalHeaderId:long}")]
    public Task<JournalDto> Get(long journalHeaderId, CancellationToken cancellationToken) => service.GetAsync(journalHeaderId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<SaveJournalResultDto> Create(SaveJournalRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(request, cancellationToken);

    [HttpPut("{journalHeaderId:long}")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<SaveJournalResultDto> Update(long journalHeaderId, SaveJournalRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(journalHeaderId, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/submit")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<JournalActionResultDto> Submit(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Submit, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/approve")]
    [HasPermission(Permissions.Gl.JournalApprove)]
    public Task<JournalActionResultDto> Approve(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Approve, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/reject")]
    [HasPermission(Permissions.Gl.JournalApprove)]
    public Task<JournalActionResultDto> Reject(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Reject, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/post")]
    [HasPermission(Permissions.Gl.JournalPost)]
    public Task<JournalActionResultDto> Post(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Post, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/cancel")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<JournalActionResultDto> Cancel(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Cancel, request, cancellationToken);

    [HttpPost("{journalHeaderId:long}/reverse")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<JournalActionResultDto> Reverse(long journalHeaderId, JournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActAsync(journalHeaderId, GlJournalService.Reverse, request, cancellationToken);

    [HttpPost("bulk/submit")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<BulkJournalResultDto> SubmitMany(BulkJournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActManyAsync(GlJournalService.Submit, request, cancellationToken);

    [HttpPost("bulk/approve")]
    [HasPermission(Permissions.Gl.JournalApprove)]
    public Task<BulkJournalResultDto> ApproveMany(BulkJournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActManyAsync(GlJournalService.Approve, request, cancellationToken);

    [HttpPost("bulk/reject")]
    [HasPermission(Permissions.Gl.JournalApprove)]
    public Task<BulkJournalResultDto> RejectMany(BulkJournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActManyAsync(GlJournalService.Reject, request, cancellationToken);

    [HttpPost("bulk/post")]
    [HasPermission(Permissions.Gl.JournalPost)]
    public Task<BulkJournalResultDto> PostMany(BulkJournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActManyAsync(GlJournalService.Post, request, cancellationToken);

    [HttpPost("bulk/cancel")]
    [HasPermission(Permissions.Gl.JournalEdit)]
    public Task<BulkJournalResultDto> CancelMany(BulkJournalActionRequest request, CancellationToken cancellationToken) =>
        service.ActManyAsync(GlJournalService.Cancel, request, cancellationToken);
}
