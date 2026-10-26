using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.ChartOfAccounts;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>The chart of accounts tree and its accounts (APEX App 120 pages 2/3).</summary>
[ApiController]
[Route("api/gl/chart-of-accounts")]
[HasPermission(Permissions.Gl.SetupView)]
public sealed class GlChartOfAccountsController(IGlChartOfAccountsService service) : ControllerBase
{
    [HttpGet]
    public Task<ChartOfAccountsDto> Get([FromQuery] long? coaId, CancellationToken cancellationToken) =>
        service.GetAsync(coaId, cancellationToken);

    [HttpPost("accounts")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlAccountDto> Create(SaveGlAccountRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(request, cancellationToken);

    [HttpPut("accounts/{accountId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlAccountDto> Update(long accountId, SaveGlAccountRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(accountId, request, cancellationToken);
}
