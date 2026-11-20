using Capitra.Api.Security;
using Capitra.Application.Security;
using Capitra.Infrastructure.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Cmc;

/// <summary>
/// Communication Center: the outgoing mail servers. One is active and sends every e-mail of the system (account activation,
/// password reset). Passwords go in, never out.
/// </summary>
[ApiController]
[Route("api/cmc/mail-servers")]
[HasPermission(Permissions.Cmc.MailServerEdit)]
public sealed class CmcMailServersController(CmcMailServerService servers) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CmcMailServerDto>> List(CancellationToken ct) => servers.ListAsync(ct);

    /// <summary>Which server the system uses now: the Communication Center's active one, the application settings', or none.</summary>
    [HttpGet("status")]
    public Task<CmcMailStatus> Status(CancellationToken ct) => servers.StatusAsync(ct);

    [HttpPost]
    public Task<CmcMailServerDto> Create(CmcMailServerRequest request, CancellationToken ct) => servers.SaveAsync(null, request, ct);

    [HttpPut("{mailServerId:long}")]
    public Task<CmcMailServerDto> Update(long mailServerId, CmcMailServerRequest request, CancellationToken ct) => servers.SaveAsync(mailServerId, request, ct);

    [HttpPost("{mailServerId:long}/test")]
    public Task<CmcTestResult> SendTest(long mailServerId, CmcTestRequest request, CancellationToken ct) =>
        servers.TestAsync(mailServerId, request, User.Identity?.Name, ct);

    [HttpPost("{mailServerId:long}/activate")]
    public Task<CmcMailServerDto> Activate(long mailServerId, CancellationToken ct) => servers.ActivateAsync(mailServerId, ct);

    [HttpPost("{mailServerId:long}/deactivate")]
    public Task<CmcMailServerDto> Deactivate(long mailServerId, CancellationToken ct) => servers.DeactivateAsync(mailServerId, ct);
}

/// <summary>Communication Center: every e-mail the system sent or tried to send, newest first.</summary>
[ApiController]
[Route("api/cmc/email-log")]
[HasPermission(Permissions.Cmc.EmailLogView)]
public sealed class CmcEmailLogController(CmcMailServerService servers) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CmcEmailLogDto>> List([FromQuery] CmcEmailLogQuery query, CancellationToken ct) => servers.LogAsync(query, ct);
}
