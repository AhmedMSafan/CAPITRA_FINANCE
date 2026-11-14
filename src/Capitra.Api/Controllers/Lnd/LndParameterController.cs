using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>
/// Retail Lending > Parameters. Reading needs LND.PARAMETERS.VIEW, asking for or withdrawing a value LND.PARAMETERS.EDIT, and approving
/// or rejecting one LND.PARAMETERS.APPROVE.
/// </summary>
[ApiController]
[Route("api/lnd/parameters")]
public sealed class LndParameterController(LndParameterService parameters) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.ParametersView)]
    public Task<LndParametersPageDto> Get(CancellationToken cancellationToken) => Page(cancellationToken);

    [HttpPost("{code}/{scope}/request")]
    [HasPermission(Permissions.Lnd.ParametersEdit)]
    public async Task<LndParametersPageDto> RequestValue(string code, string scope, LndParameterRequestDto dto, CancellationToken cancellationToken)
    {
        await parameters.RequestAsync(code, scope, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("values/{id:long}/approve")]
    [HasPermission(Permissions.Lnd.ParametersApprove)]
    public async Task<LndParametersPageDto> Approve(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        await parameters.ApproveAsync(id, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("values/{id:long}/reject")]
    [HasPermission(Permissions.Lnd.ParametersApprove)]
    public async Task<LndParametersPageDto> Reject(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        await parameters.RejectAsync(id, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    [HttpPost("values/{id:long}/withdraw")]
    [HasPermission(Permissions.Lnd.ParametersEdit)]
    public async Task<LndParametersPageDto> Withdraw(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        await parameters.WithdrawAsync(id, dto, cancellationToken);
        return await Page(cancellationToken);
    }

    private Task<LndParametersPageDto> Page(CancellationToken cancellationToken) =>
        parameters.GetAsync(Has(Permissions.Lnd.ParametersEdit), Has(Permissions.Lnd.ParametersApprove), cancellationToken);

    private bool Has(string permission) => User.Claims.Any(c =>
        c.Type == Permissions.ClaimType && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
