using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>
/// Paying a retail loan out. Reading needs LND.LOAN.VIEW; asking for and withdrawing a payout LND.LOAN.EDIT; paying or rejecting one
/// LND.LOAN.DISBURSE.
/// </summary>
[ApiController]
[Route("api/lnd")]
public sealed class LndDisbursementController(LndDisbursementService disbursements) : ControllerBase
{
    [HttpGet("loans/{loanId:long}/disbursements")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndDisbursementPageDto> Get(long loanId, CancellationToken cancellationToken) => Page(loanId, cancellationToken);

    [HttpPost("loans/{loanId:long}/disbursements")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndDisbursementPageDto> RequestPayout(long loanId, LndDisbursementRequestDto dto, CancellationToken cancellationToken)
    {
        await disbursements.RequestAsync(loanId, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("loans/{loanId:long}/disbursements/{id:long}/pay")]
    [HasPermission(Permissions.Lnd.LoanDisburse)]
    public async Task<LndDisbursementPageDto> Pay(long loanId, long id, LndDisbursementDecisionDto dto, CancellationToken cancellationToken)
    {
        await disbursements.PayAsync(id, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("loans/{loanId:long}/disbursements/{id:long}/reject")]
    [HasPermission(Permissions.Lnd.LoanDisburse)]
    public async Task<LndDisbursementPageDto> Reject(long loanId, long id, LndDisbursementDecisionDto dto, CancellationToken cancellationToken)
    {
        await disbursements.RejectAsync(id, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("loans/{loanId:long}/disbursements/{id:long}/withdraw")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndDisbursementPageDto> Withdraw(long loanId, long id, CancellationToken cancellationToken)
    {
        await disbursements.WithdrawAsync(id, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    private Task<LndDisbursementPageDto> Page(long loanId, CancellationToken cancellationToken) =>
        disbursements.GetAsync(loanId, Has(Permissions.Lnd.LoanEdit), Has(Permissions.Lnd.LoanDisburse), cancellationToken);

    private bool Has(string permission) => User.Claims.Any(c =>
        c.Type == Permissions.ClaimType && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
