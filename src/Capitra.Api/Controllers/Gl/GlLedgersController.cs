using Capitra.Api.Security;
using Capitra.Application.Modules.Gl.Ledgers;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Gl;

/// <summary>Ledgers of each legal entity. APEX App 120 set these up by script; here they have a screen.</summary>
[ApiController]
[Route("api/gl/ledgers")]
[HasPermission(Permissions.Gl.SetupView)]
public sealed class GlLedgersController(IGlLedgerService service) : ControllerBase
{
    [HttpGet]
    public Task<LedgersPageDto> Get(CancellationToken cancellationToken) => service.GetAsync(cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlLedgerDto> Create(SaveGlLedgerRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(request, cancellationToken);

    [HttpPut("{ledgerId:long}")]
    [HasPermission(Permissions.Gl.SetupEdit)]
    public Task<GlLedgerDto> Update(long ledgerId, SaveGlLedgerRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(ledgerId, request, cancellationToken);
}
