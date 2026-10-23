using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Accounts of a portfolio (APEX App 105 page 205).</summary>
[ApiController]
[Route("api/mtg/portfolio-accounts")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgPortfolioAccountController(IMtgPortfolioAccountService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<PortfolioAccountDto>> List([FromQuery] long portfolioId, CancellationToken cancellationToken) =>
        service.ListAsync(portfolioId, cancellationToken);

    [HttpGet("{portfolioAccountId:long}")]
    public Task<PortfolioAccountDto> Get(long portfolioAccountId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioAccountId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<ActionResult<PortfolioAccountDto>> Create(SavePortfolioAccountRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { portfolioAccountId = created.PortfolioAccountId }, created);
    }

    [HttpPut("{portfolioAccountId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public Task<PortfolioAccountDto> Update(long portfolioAccountId, SavePortfolioAccountRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(portfolioAccountId, request, cancellationToken);

    [HttpDelete("{portfolioAccountId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<IActionResult> Delete(long portfolioAccountId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(portfolioAccountId, rowVersion, cancellationToken);
        return NoContent();
    }
}
