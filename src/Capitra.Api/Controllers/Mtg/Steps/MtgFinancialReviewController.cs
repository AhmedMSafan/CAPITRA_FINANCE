using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.FinancialReview;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Financial Review" (APEX App 105 page 214).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/financial-review")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgFinancialReviewController(IMtgFinancialReviewService service) : ControllerBase
{
    [HttpGet]
    public Task<FinancialReviewDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Works the figures out for these choices without saving them.</summary>
    [HttpPost("calculate")]
    public Task<FinancialCalculationDto> Calculate(long portfolioId, FinancialReviewInputs inputs, CancellationToken cancellationToken) =>
        service.CalculateAsync(portfolioId, inputs, cancellationToken);

    /// <summary>Works the figures out again and saves them; the portfolio's purchase price follows.</summary>
    [HttpPut]
    [HasPermission(Permissions.Mtg.StepFinancialReview)]
    public Task<FinancialReviewDto> Save(long portfolioId, SaveFinancialReviewRequest request, CancellationToken cancellationToken) =>
        service.SaveAsync(portfolioId, request, cancellationToken);

    /// <summary>Accepts (ACCEPT) or rejects (REJECT) the step. A comment is required.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepFinancialReview)]
    public Task<FinancialReviewDto> Decide(long portfolioId, FinancialReviewDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
