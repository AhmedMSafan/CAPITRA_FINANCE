using Capitra.Api.Security;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

/// <summary>Sign-ins and activity: every sign-in attempt, and per session the screening inquiries made in it. Read-only.</summary>
[ApiController]
[Route("api/scr/access-log")]
[HasPermission(Permissions.Scr.AccessLogView)]
public sealed class ScrAccessLogController(SignInLog log) : ControllerBase
{
    [HttpGet]
    public Task<SignInPage> List([FromQuery] SignInQuery query, CancellationToken ct) => log.ListAsync(query, ct);

    [HttpGet("{signInId:long}")]
    public Task<SessionDto> Session(long signInId, CancellationToken ct) => log.SessionAsync(signInId, ct);
}
