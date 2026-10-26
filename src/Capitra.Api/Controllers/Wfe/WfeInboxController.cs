using Capitra.Application.Modules.Wfe;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>My Inbox: every signed-in user sees and works only the tasks assigned to them or their roles.</summary>
[ApiController]
[Route("api/wfe/inbox")]
public sealed class WfeInboxController(WfeInbox inbox) : ControllerBase
{
    [HttpGet("my")]
    public Task<IReadOnlyList<WfeInboxItemDto>> My(CancellationToken cancellationToken) => inbox.MyInboxAsync(cancellationToken);

    [HttpGet("{id:long}")]
    public Task<WfeInboxItemDto> Get(long id, CancellationToken cancellationToken) => inbox.GetAsync(id, cancellationToken);

    [HttpPost("{id:long}/claim")]
    public Task<WfeInboxItemDto> Claim(long id, CancellationToken cancellationToken) => inbox.ClaimAsync(id, cancellationToken);

    [HttpPost("{id:long}/start")]
    public Task<WfeInboxItemDto> Start(long id, CancellationToken cancellationToken) => inbox.StartAsync(id, cancellationToken);

    [HttpPost("{id:long}/complete")]
    public Task<WfeInboxItemDto> Complete(long id, CompleteInboxItemRequest request, CancellationToken cancellationToken) =>
        inbox.CompleteAsync(id, request, cancellationToken);
}
