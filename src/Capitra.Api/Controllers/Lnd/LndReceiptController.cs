using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Customer payments of a retail loan. Reading needs LND.LOAN.VIEW; previewing and receiving a payment LND.LOAN.RECEIVE.</summary>
[ApiController]
[Route("api/lnd/loans/{loanId:long}/receipts")]
public sealed class LndReceiptController(LndReceiptService receipts) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndReceiptsPageDto> Get(long loanId, CancellationToken cancellationToken) => Page(loanId, cancellationToken);

    [HttpGet("preview")]
    [HasPermission(Permissions.Lnd.LoanReceive)]
    public Task<LndReceiptPreviewDto> Preview(long loanId, [FromQuery] DateOnly? valueDate, [FromQuery] decimal? amount, CancellationToken cancellationToken) =>
        receipts.PreviewAsync(loanId, valueDate, amount, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Lnd.LoanReceive)]
    public async Task<LndReceiptsPageDto> Receive(long loanId, LndReceiptRequestDto dto, CancellationToken cancellationToken)
    {
        await receipts.ReceiveAsync(loanId, dto, cancellationToken);
        return await Page(loanId, cancellationToken);
    }

    private Task<LndReceiptsPageDto> Page(long loanId, CancellationToken cancellationToken) =>
        receipts.GetAsync(loanId, User.Claims.Any(c => c.Type == Permissions.ClaimType
            && (c.Value == Permissions.All || string.Equals(c.Value, Permissions.Lnd.LoanReceive, StringComparison.OrdinalIgnoreCase))), cancellationToken);
}
