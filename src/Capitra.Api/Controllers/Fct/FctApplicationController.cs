using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Fct;

/// <summary>The Factoring Calculator and the applications (FCT_OFFERS). Calculating needs FCT.VIEW; changes need FCT.EDIT.</summary>
[ApiController]
[Route("api/fct")]
public sealed class FctApplicationController(FctApplicationService applications) : ControllerBase
{
    [HttpPost("calculator/calculate")]
    [HasPermission(Permissions.Fct.View)]
    public FctCalculationResult Calculate(FctCalculationRequest request) => applications.Calculate(request);

    [HttpPost("applications/from-calculator")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctCreateOfferResult> CreateFromCalculator(FctCreateOfferRequest request, CancellationToken cancellationToken) =>
        applications.CreateFromCalculatorAsync(request, cancellationToken);

    [HttpPut("applications/{offerId:long}/status")]
    [HasPermission(Permissions.Fct.Edit)]
    public async Task<IActionResult> SetStatus(long offerId, FctOfferStatusRequest request, CancellationToken cancellationToken)
    {
        await applications.SetStatusAsync(offerId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("applications/{offerId:long}/start-workflow")]
    [HasPermission(Permissions.Fct.Edit)]
    public Task<FctStartWorkflowResult> StartWorkflow(long offerId, CancellationToken cancellationToken) =>
        applications.StartWorkflowAsync(offerId, cancellationToken);
}
