using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Write-off of a retail loan. Reading needs LND.LOAN.VIEW; asking LND.WRITE_OFF.REQUEST; deciding LND.WRITE_OFF.APPROVE.</summary>
[ApiController]
[Route("api/lnd/loans/{loanId:long}/write-offs")]
public sealed class LndWriteOffController(LndWriteOffService writeOffs) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndWriteOffsPageDto> Get(long loanId, CancellationToken cancellationToken) => Page(loanId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Lnd.WriteOffRequest)]
    public async Task<LndWriteOffsPageDto> Ask(long loanId, LndWriteOffRequestDto dto, CancellationToken cancellationToken)
    {
        await writeOffs.RequestAsync(loanId, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("{writeOffId:long}/withdraw")]
    [HasPermission(Permissions.Lnd.WriteOffRequest)]
    public async Task<LndWriteOffsPageDto> Withdraw(long loanId, long writeOffId, CancellationToken cancellationToken)
    {
        await writeOffs.WithdrawAsync(loanId, writeOffId, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("{writeOffId:long}/approve")]
    [HasPermission(Permissions.Lnd.WriteOffApprove)]
    public async Task<LndWriteOffsPageDto> Approve(long loanId, long writeOffId, LndWriteOffDecisionDto dto, CancellationToken cancellationToken)
    {
        await writeOffs.ApproveAsync(loanId, writeOffId, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    [HttpPost("{writeOffId:long}/reject")]
    [HasPermission(Permissions.Lnd.WriteOffApprove)]
    public async Task<LndWriteOffsPageDto> Reject(long loanId, long writeOffId, LndWriteOffDecisionDto dto, CancellationToken cancellationToken)
    {
        await writeOffs.RejectAsync(loanId, writeOffId, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    private Task<LndWriteOffsPageDto> Page(long loanId, CancellationToken cancellationToken) =>
        writeOffs.GetAsync(loanId, Has(Permissions.Lnd.WriteOffRequest), Has(Permissions.Lnd.WriteOffApprove), cancellationToken);

    private bool Has(string permission) => User.Claims.Any(c => c.Type == Permissions.ClaimType
        && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
