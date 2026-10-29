using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Fct;

/// <summary>The Factoring customers and the Factoring Customer Workspace. Reading needs FCT.VIEW.</summary>
[ApiController]
[Route("api/fct/customers")]
public sealed class FctWorkspaceController(FctWorkspaceService workspace) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Fct.View)]
    public Task<IReadOnlyList<FctCustomerDto>> List(CancellationToken cancellationToken) => workspace.ListCustomersAsync(cancellationToken);

    [HttpGet("{clientId:long}")]
    [HasPermission(Permissions.Fct.View)]
    public Task<FctCustomerDetailsDto> Get(long clientId, CancellationToken cancellationToken) => workspace.GetCustomerAsync(clientId, cancellationToken);

    [HttpGet("{clientId:long}/applications")]
    [HasPermission(Permissions.Fct.View)]
    public Task<IReadOnlyList<FctApplicationDto>> Applications(long clientId, CancellationToken cancellationToken) =>
        workspace.ListApplicationsAsync(clientId, cancellationToken);

    [HttpGet("{clientId:long}/agreements")]
    [HasPermission(Permissions.Fct.View)]
    public Task<IReadOnlyList<FctAgreementDto>> Agreements(long clientId, CancellationToken cancellationToken) =>
        workspace.ListAgreementsAsync(clientId, cancellationToken);

    [HttpGet("{clientId:long}/credit-limits")]
    [HasPermission(Permissions.Fct.View)]
    public Task<IReadOnlyList<FctCreditLimitDto>> CreditLimits(long clientId, CancellationToken cancellationToken) =>
        workspace.ListCreditLimitsAsync(clientId, cancellationToken);
}
