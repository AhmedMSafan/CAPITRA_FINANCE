using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.AmlReview;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "AML Review" (APEX App 105 page 209).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/aml-review")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgAmlReviewController(IMtgAmlReviewService service) : ControllerBase
{
    [HttpGet]
    public Task<AmlReviewDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Records one AML check of the developer or an investor.</summary>
    [HttpPost("results")]
    [HasPermission(Permissions.Mtg.StepAmlReview)]
    public Task<AmlResultDto> AddResult(long portfolioId, AddAmlResultRequest request, CancellationToken cancellationToken) =>
        service.AddResultAsync(portfolioId, request, cancellationToken);

    /// <summary>Keeps contracts in the deal (SELECTED) or leaves them out (EXCLUDED).</summary>
    [HttpPut("deal-decisions")]
    [HasPermission(Permissions.Mtg.StepAmlReview)]
    public async Task<IActionResult> SaveDealDecisions(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await service.SaveDealDecisionsAsync(portfolioId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>Accepts or rejects the review. A comment is required either way.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepAmlReview)]
    public Task<AmlReviewDto> Decide(long portfolioId, AmlDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
