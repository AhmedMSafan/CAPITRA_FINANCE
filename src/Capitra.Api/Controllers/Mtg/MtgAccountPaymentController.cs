using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Payment history of a portfolio account (APEX App 105 pages 206/207).</summary>
[ApiController]
[Route("api/mtg/account-payments")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgAccountPaymentController(IMtgAccountPaymentService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AccountPaymentDto>> List([FromQuery] long portfolioAccountId, CancellationToken cancellationToken) =>
        service.ListAsync(portfolioAccountId, cancellationToken);

    [HttpGet("{accountPaymentId:long}")]
    public Task<AccountPaymentDto> Get(long accountPaymentId, CancellationToken cancellationToken) =>
        service.GetAsync(accountPaymentId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<ActionResult<AccountPaymentDto>> Create(SaveAccountPaymentRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { accountPaymentId = created.AccountPaymentId }, created);
    }

    [HttpPut("{accountPaymentId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public Task<AccountPaymentDto> Update(long accountPaymentId, SaveAccountPaymentRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(accountPaymentId, request, cancellationToken);

    [HttpDelete("{accountPaymentId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<IActionResult> Delete(long accountPaymentId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(accountPaymentId, rowVersion, cancellationToken);
        return NoContent();
    }
}
