using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>My Inbox: every signed-in user sees and works only the tasks assigned to them or their roles.</summary>
[ApiController]
[Route("api/wfe/inbox")]
public sealed class WfeInboxController(WfeInbox inbox) : ControllerBase
{
    [HttpGet("my")]
    public Task<IReadOnlyList<WfeInboxItemDto>> My(CancellationToken cancellationToken) => inbox.MyInboxAsync(cancellationToken);

    [HttpGet("all")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeInboxItemDto>> All(CancellationToken cancellationToken) => inbox.AllOpenAsync(cancellationToken);

    [HttpGet("{id:long}")]
    public Task<WfeInboxItemDto> Get(long id, CancellationToken cancellationToken) => inbox.GetAsync(id, cancellationToken);

    [HttpGet("{id:long}/data")]
    public Task<WfeInstanceDataDto> Data(long id, CancellationToken cancellationToken) => inbox.DataAsync(id, cancellationToken);

    /// <summary>The step's form, or form null when the step has none.</summary>
    [HttpGet("{id:long}/form")]
    public async Task<WfeTaskFormDto> Form(long id, CancellationToken cancellationToken) => new(await inbox.FormAsync(id, cancellationToken));

    [HttpPost("{id:long}/claim")]
    public Task<WfeInboxItemDto> Claim(long id, CancellationToken cancellationToken) => inbox.ClaimAsync(id, cancellationToken);

    [HttpPost("{id:long}/start")]
    public Task<WfeInboxItemDto> Start(long id, CancellationToken cancellationToken) => inbox.StartAsync(id, cancellationToken);

    [HttpPost("{id:long}/complete")]
    public Task<WfeInboxItemDto> Complete(long id, CompleteInboxItemRequest request, CancellationToken cancellationToken) =>
        inbox.CompleteAsync(id, request, cancellationToken);
}
