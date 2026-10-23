using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Portfolio cases (APEX App 105 pages 201 board, 202 form, 203 overview).</summary>
[ApiController]
[Route("api/mtg/portfolios")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgPortfolioController(IMtgPortfolioService service, IMtgPortfolioQueryService queries) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<PortfolioCaseDto>> List([FromQuery] long? developerId, CancellationToken cancellationToken) =>
        queries.ListCasesAsync(developerId, cancellationToken);

    [HttpGet("{portfolioId:long}")]
    public Task<PortfolioDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    [HttpGet("{portfolioId:long}/workspace")]
    public Task<PortfolioWorkspaceDto> Workspace(long portfolioId, CancellationToken cancellationToken) =>
        queries.GetWorkspaceAsync(portfolioId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<ActionResult<PortfolioDto>> Create(SavePortfolioRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { portfolioId = created.PortfolioId }, created);
    }

    [HttpPut("{portfolioId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public Task<PortfolioDto> Update(long portfolioId, SavePortfolioRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(portfolioId, request, cancellationToken);

    [HttpPut("{portfolioId:long}/financial-terms")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public Task<PortfolioDto> SaveFinancialTerms(long portfolioId, SaveFinancialTermsRequest request, CancellationToken cancellationToken) =>
        service.SaveFinancialTermsAsync(portfolioId, request, cancellationToken);

    [HttpDelete("{portfolioId:long}")]
    [HasPermission(Permissions.Mtg.PortfolioEdit)]
    public async Task<IActionResult> Delete(long portfolioId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(portfolioId, rowVersion, cancellationToken);
        return NoContent();
    }
}
