using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Modules.Trs;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Trs;

/// <summary>Treasury Disburse: choose, reserve and execute the source of funds of a disbursement, checked against limits and bank rules.</summary>
[ApiController]
[Route("api/trs/disbursements/{disbursementId:long}/funding")]
public sealed class TrsDisburseController(TrsDisburseService service, FctDisbursementSummaryService summary) : ControllerBase
{
    /// <summary>The invoice summary of the disbursement (the same one the disbursement request shows).</summary>
    [HttpGet("summary")]
    [HasPermission(Permissions.Trs.View)]
    public Task<FctDisbursementSummaryDto> Summary(long disbursementId, CancellationToken cancellationToken) =>
        summary.GetAsync(disbursementId, cancellationToken);

    [HttpGet]
    [HasPermission(Permissions.Trs.View)]
    public Task<TrsDisburseDto> Get(long disbursementId, CancellationToken cancellationToken) => service.GetAsync(disbursementId, cancellationToken);

    /// <summary>One party's invoices, bank accounts and amount to receive (by customer, or by tax number when not a customer yet).</summary>
    [HttpGet("party")]
    [HasPermission(Permissions.Trs.View)]
    public Task<TrsDisbursePartyDetailDto> Party(long disbursementId, [FromQuery] long? clientId, [FromQuery] string? taxNumber,
        CancellationToken cancellationToken) => service.PartyAsync(disbursementId, clientId, taxNumber, cancellationToken);

    [HttpPut]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<TrsDisburseDto> Save(long disbursementId, TrsDisburseFormDto dto, CancellationToken cancellationToken) =>
        service.SaveAsync(disbursementId, dto, cancellationToken);

    [HttpPost("approve-override")]
    [HasPermission(Permissions.Trs.Verify)]
    public Task<TrsDisburseDto> ApproveOverride(long disbursementId, CancellationToken cancellationToken) =>
        service.ApproveOverrideAsync(disbursementId, cancellationToken);

    [HttpPost("execute")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<TrsDisburseDto> Execute(long disbursementId, CancellationToken cancellationToken) => service.ExecuteAsync(disbursementId, cancellationToken);

    /// <summary>Reverse factoring: the bank confirmed the ticked supplier transfers; each posts its own GL entry.</summary>
    [HttpPost("transfers/confirm")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<TrsDisburseDto> ConfirmTransfers(long disbursementId, TrsConfirmTransfersDto dto, CancellationToken cancellationToken) =>
        service.ConfirmTransfersAsync(disbursementId, dto.TransferIds ?? [], dto.BankReference, cancellationToken);
}
