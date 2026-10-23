using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.CoverageReview;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Coverage Review" (APEX App 105 page 212).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/coverage-review")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgCoverageReviewController(IMtgCoverageReviewService service) : ControllerBase
{
    [HttpGet]
    public Task<CoverageReviewDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    /// <summary>Keeps every investor who meets the required coverage and excludes the rest.</summary>
    [HttpPost("apply-rule")]
    [HasPermission(Permissions.Mtg.StepCoverageReview)]
    public Task<ApplyCoverageRuleResult> ApplyRule(long portfolioId, CancellationToken cancellationToken) =>
        service.ApplyRuleAsync(portfolioId, cancellationToken);

    /// <summary>Keeps (SELECTED) or excludes (EXCLUDED) all of one investor's contracts.</summary>
    [HttpPut("investors/{investorId:long}/decision")]
    [HasPermission(Permissions.Mtg.StepCoverageReview)]
    public Task<CoverageReviewDto> SetInvestorDecision(
        long portfolioId, long investorId, CoverageInvestorDecisionRequest request, CancellationToken cancellationToken) =>
        service.SetInvestorDecisionAsync(portfolioId, investorId, request, cancellationToken);

    /// <summary>Accepts (ACCEPT) or rejects (REJECT) the step. A comment is required.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepCoverageReview)]
    public Task<CoverageReviewDto> Decide(long portfolioId, CoverageDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
