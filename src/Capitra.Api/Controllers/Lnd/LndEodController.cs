using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>
/// Retail Lending's end of day detail: its runs, the finances in non-accrual and those proposed for write-off. It runs as the job
/// LND_DAILY of the End of Day application (api/eod), not from here.
/// </summary>
[ApiController]
[Route("api/lnd/eod")]
public sealed class LndEodController(LndEodService eod) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndEodPageDto> Get(CancellationToken cancellationToken) => eod.GetAsync(false, cancellationToken);
}
