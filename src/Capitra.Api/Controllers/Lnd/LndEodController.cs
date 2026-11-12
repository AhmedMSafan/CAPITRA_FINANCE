using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Retail Lending end of day. Reading needs LND.LOAN.VIEW; running a day LND.EOD.RUN.</summary>
[ApiController]
[Route("api/lnd/eod")]
public sealed class LndEodController(LndEodService eod) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndEodPageDto> Get(CancellationToken cancellationToken) => Page(cancellationToken);

    [HttpPost("run")]
    [HasPermission(Permissions.Lnd.EodRun)]
    public async Task<LndEodPageDto> Run(LndEodRequestDto dto, CancellationToken cancellationToken)
    {
        await eod.RunAsync(dto, cancellationToken);
        return await Page(cancellationToken);
    }

    private Task<LndEodPageDto> Page(CancellationToken cancellationToken) =>
        eod.GetAsync(User.Claims.Any(c => c.Type == Permissions.ClaimType
            && (c.Value == Permissions.All || string.Equals(c.Value, Permissions.Lnd.EodRun, StringComparison.OrdinalIgnoreCase))), cancellationToken);
}
