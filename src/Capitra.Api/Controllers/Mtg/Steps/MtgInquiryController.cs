using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps;
using Capitra.Application.Modules.Mtg.PortfolioSteps.Inquiries;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Inquiries" (APEX App 105 page 210).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/inquiries")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgInquiryController(IMtgInquiryService service) : ControllerBase
{
    [HttpGet]
    public Task<InquiriesDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);

    [HttpPost("records")]
    [HasPermission(Permissions.Mtg.StepInquiries)]
    public Task<InquiryDto> Create(long portfolioId, SaveInquiryRequest request, CancellationToken cancellationToken) =>
        service.CreateAsync(portfolioId, request, cancellationToken);

    [HttpPut("records/{inquiryResultId:long}")]
    [HasPermission(Permissions.Mtg.StepInquiries)]
    public Task<InquiryDto> Update(long portfolioId, long inquiryResultId, SaveInquiryRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(portfolioId, inquiryResultId, request, cancellationToken);

    [HttpDelete("records/{inquiryResultId:long}")]
    [HasPermission(Permissions.Mtg.StepInquiries)]
    public async Task<IActionResult> Delete(long portfolioId, long inquiryResultId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(portfolioId, inquiryResultId, rowVersion, cancellationToken);
        return NoContent();
    }

    /// <summary>Keeps contracts in the deal or excludes them. An exclusion made here is final.</summary>
    [HttpPut("deal-decisions")]
    [HasPermission(Permissions.Mtg.StepInquiries)]
    public async Task<IActionResult> SaveDealDecisions(long portfolioId, SaveDealDecisionsRequest request, CancellationToken cancellationToken)
    {
        await service.SaveDealDecisionsAsync(portfolioId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>Completes (APPROVE) or rejects (REJECT) the step.</summary>
    [HttpPost("decision")]
    [HasPermission(Permissions.Mtg.StepInquiries)]
    public Task<InquiriesDto> Decide(long portfolioId, InquiryDecisionRequest request, CancellationToken cancellationToken) =>
        service.DecideAsync(portfolioId, request, cancellationToken);
}
