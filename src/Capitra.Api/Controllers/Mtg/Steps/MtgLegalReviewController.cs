using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.LegalReview;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Legal Review" (APEX App 105 page 211).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/legal-review")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgLegalReviewController(IMtgLegalReviewService service) : ControllerBase
{
    [HttpGet]
    public Task<LegalReviewDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Saves the checklist, findings and sign-off.</summary>
    [HttpPut]
    [HasPermission(Permissions.Mtg.StepLegalReview)]
    public Task<LegalReviewDto> Save(long portfolioId, SaveLegalReviewRequest request, CancellationToken cancellationToken) =>
        service.SaveAsync(portfolioId, request, cancellationToken);

    /// <summary>Keeps contracts in the deal or excludes them.</summary>
    [HttpPut("deal-decisions")]
    [HasPermission(Permissions.Mtg.StepLegalReview)]
    public async Task<IActionResult> SaveDealDecisions(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await service.SaveDealDecisionsAsync(portfolioId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>Accepts (ACCEPT) or rejects (REJECT) the step.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepLegalReview)]
    public Task<LegalReviewDto> Decide(long portfolioId, LegalDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
