using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.BoardReview;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Board Review" (APEX App 105 page 215).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/board-review")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgBoardReviewController(IMtgBoardReviewService service) : ControllerBase
{
    /// <summary>The board report: what every earlier step found, the price, and the board's decision once recorded.</summary>
    [HttpGet]
    public Task<BoardReviewDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Records the board resolution as an approval (ACCEPT) or a rejection (REJECT).</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepBoardReview)]
    public Task<BoardReviewDto> Decide(long portfolioId, BoardDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
