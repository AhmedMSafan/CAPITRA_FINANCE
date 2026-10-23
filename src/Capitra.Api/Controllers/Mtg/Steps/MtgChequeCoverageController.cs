using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ChequeCoverage;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Cheque Coverage Review" (APEX App 105 page 213).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/cheque-coverage")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgChequeCoverageController(IMtgChequeCoverageService service) : ControllerBase
{
    [HttpGet]
    public Task<ChequeCoverageDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    [HttpPost("cheques")]
    [HasPermission(Permissions.Mtg.StepChequeCoverage)]
    public Task<ChequeDto> Create(long portfolioId, SaveChequeRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(portfolioId, request, cancellationToken);

    [HttpPut("cheques/{chequeId:long}")]
    [HasPermission(Permissions.Mtg.StepChequeCoverage)]
    public Task<ChequeDto> Update(long portfolioId, long chequeId, SaveChequeRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(portfolioId, chequeId, request, cancellationToken);

    [HttpDelete("cheques/{chequeId:long}")]
    [HasPermission(Permissions.Mtg.StepChequeCoverage)]
    public async Task<IActionResult> Delete(long portfolioId, long chequeId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(portfolioId, chequeId, rowVersion, cancellationToken);
        return NoContent();
    }

    /// <summary>Accepts (ACCEPT) or rejects (REJECT) the step. A comment is required.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepChequeCoverage)]
    public Task<ChequeCoverageDto> Decide(long portfolioId, ChequeCoverageDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
