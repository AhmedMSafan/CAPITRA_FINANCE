using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Valuation;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Valuation" (APEX App 105 page 401).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/valuation")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgValuationController(IMtgValuationService service) : ControllerBase
{
    [HttpGet]
    public Task<ValuationsDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    [HttpPost("records")]
    [HasPermission(Permissions.Mtg.StepValuation)]
    public Task<ValuationDto> Create(long portfolioId, SaveValuationRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(portfolioId, request, cancellationToken);

    [HttpPut("records/{valuationId:long}")]
    [HasPermission(Permissions.Mtg.StepValuation)]
    public Task<ValuationDto> Update(long portfolioId, long valuationId, SaveValuationRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(portfolioId, valuationId, request, cancellationToken);

    [HttpDelete("records/{valuationId:long}")]
    [HasPermission(Permissions.Mtg.StepValuation)]
    public async Task<IActionResult> Delete(long portfolioId, long valuationId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(portfolioId, valuationId, rowVersion, cancellationToken);
        return NoContent();
    }

    /// <summary>Completes the step. There is no reject on this step.</summary>
    [HttpPost("complete")]
    [HasPermission(Permissions.Mtg.StepValuation)]
    public Task<ValuationsDto> Complete(long portfolioId, CompleteValuationRequest request, CancellationToken cancellationToken) =>
        service.CompleteAsync(portfolioId, request, cancellationToken);
}
