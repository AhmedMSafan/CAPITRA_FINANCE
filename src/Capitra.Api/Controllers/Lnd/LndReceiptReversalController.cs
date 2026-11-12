using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Reversal of a retail loan receipt, with replay of the later receipts. Needs LND.RECEIPT.REVERSE.</summary>
[ApiController]
[Route("api/lnd/loans/{loanId:long}/receipts/{txnId:long}")]
public sealed class LndReceiptReversalController(LndReceiptReversalService reversals, LndReceiptService receipts) : ControllerBase
{
    [HttpGet("reversal")]
    [HasPermission(Permissions.Lnd.ReceiptReverse)]
    public Task<LndReversalPreviewDto> Preview(long loanId, long txnId, CancellationToken cancellationToken) =>
        reversals.PreviewAsync(loanId, txnId, cancellationToken);

    [HttpPost("reverse")]
    [HasPermission(Permissions.Lnd.ReceiptReverse)]
    public async Task<LndReceiptsPageDto> Reverse(long loanId, long txnId, LndReceiptReversalRequestDto dto, CancellationToken cancellationToken)
    {
        await reversals.ReverseAsync(loanId, txnId, dto, cancellationToken);
        return await receipts.GetAsync(loanId, User.Claims.Any(c => c.Type == Permissions.ClaimType
            && (c.Value == Permissions.All || string.Equals(c.Value, Permissions.Lnd.LoanReceive, StringComparison.OrdinalIgnoreCase))), cancellationToken);
    }
}
