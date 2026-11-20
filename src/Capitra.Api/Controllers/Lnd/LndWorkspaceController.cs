using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>The Lending Workspace: the customers, and one customer's loans and payments. Needs LND.LOAN.VIEW.</summary>
[ApiController]
[Route("api/lnd/customers")]
public sealed class LndWorkspaceController(LndWorkspaceService workspace) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<IReadOnlyList<LndWsCustomerDto>> List(CancellationToken cancellationToken) => workspace.ListCustomersAsync(cancellationToken);

    [HttpGet("{clientId:long}")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndWorkspaceDto> Get(long clientId, CancellationToken cancellationToken) => workspace.GetAsync(clientId, cancellationToken);
}
