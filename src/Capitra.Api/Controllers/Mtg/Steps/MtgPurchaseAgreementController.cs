using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.PortfolioSteps.PurchaseAgreement;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg.Steps;

/// <summary>Portfolio workflow step "Portfolio Purchase Agreement" (APEX App 105 page 216).</summary>
[ApiController]
[Route("api/mtg/portfolios/{portfolioId:long}/purchase-agreement")]
[HasPermission(Permissions.Mtg.PortfolioView)]
public sealed class MtgPurchaseAgreementController(IMtgPurchaseAgreementService service) : ControllerBase
{
    /// <summary>The figures and annexes of the draft purchase agreement.</summary>
    [HttpGet]
    public Task<PurchaseAgreementDto> Get(long portfolioId, CancellationToken cancellationToken) =>
        service.GetAsync(portfolioId, cancellationToken);
}
