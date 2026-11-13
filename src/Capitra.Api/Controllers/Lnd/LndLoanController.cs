using Capitra.Api.Security;
using Capitra.Application.Modules.Lnd;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Lnd;

/// <summary>Retail Lending > Loans. Reading needs LND.LOAN.VIEW; previewing, creating and cancelling a loan LND.LOAN.EDIT.</summary>
[ApiController]
[Route("api/lnd/loans")]
public sealed class LndLoanController(LndLoanService loans) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<IReadOnlyList<LndLoanListDto>> List(CancellationToken cancellationToken) => loans.ListAsync(cancellationToken);

    [HttpGet("{loanId:long}")]
    [HasPermission(Permissions.Lnd.LoanView)]
    public Task<LndLoanDto> Get(long loanId, CancellationToken cancellationToken) => loans.GetAsync(loanId, cancellationToken);

    [HttpGet("products")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndOptionDto>> Products(CancellationToken cancellationToken) => loans.ProductsAsync(cancellationToken);

    [HttpGet("clients")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<IReadOnlyList<LndOptionDto>> Clients([FromQuery] string? q, CancellationToken cancellationToken) => loans.ClientsAsync(q, cancellationToken);

    [HttpPost("preview")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public Task<LndLoanPreviewDto> Preview(LndLoanRequestDto dto, CancellationToken cancellationToken) => loans.PreviewAsync(dto, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndLoanDto> Create(LndLoanRequestDto dto, CancellationToken cancellationToken) =>
        await loans.GetAsync(await loans.CreateAsync(dto, cancellationToken), cancellationToken);

    [HttpPost("{loanId:long}/cancel")]
    [HasPermission(Permissions.Lnd.LoanEdit)]
    public async Task<LndLoanDto> Cancel(long loanId, LndLoanStatusChangeDto dto, CancellationToken cancellationToken)
    {
        await loans.CancelAsync(loanId, dto, cancellationToken);
        return await loans.GetAsync(loanId, cancellationToken);
    }
}
