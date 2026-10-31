using Capitra.Api.Security;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Collection;

/// <summary>Collection and Repayments (APEX App 136). Reading needs COLLECTION.VIEW; saving needs COLLECTION.EDIT.</summary>
[ApiController]
[Route("api/collection")]
public sealed class CollectionRepaymentController(FctCollectionRepaymentService service) : ControllerBase
{
    [HttpGet("repayments")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctRepaymentListDto>> List(CancellationToken cancellationToken) => service.ListAsync(cancellationToken);

    [HttpGet("repayments/{repaymentTxnId:long}")]
    [HasPermission(Permissions.Collection.View)]
    public Task<FctRepaymentFormDto> Get(long repaymentTxnId, CancellationToken cancellationToken) => service.GetAsync(repaymentTxnId, cancellationToken);

    [HttpPut("repayments/{repaymentTxnId:long}")]
    [HasPermission(Permissions.Collection.Edit)]
    public Task<FctRepaymentFormDto> Update(long repaymentTxnId, FctRepaymentUpdateDto dto, CancellationToken cancellationToken) =>
        service.UpdateAsync(repaymentTxnId, dto, cancellationToken);

    [HttpGet("customers")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctOptionDto>> Customers(CancellationToken cancellationToken) => service.CustomersAsync(cancellationToken);

    [HttpPost("bank-transfers")]
    [HasPermission(Permissions.Collection.Edit)]
    public Task<FctRepaymentSavedDto> SaveBankTransfer(FctBankTransferDto dto, CancellationToken cancellationToken) =>
        service.SaveBankTransferAsync(dto, cancellationToken);

    [HttpGet("deposit-accounts")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctOptionDto>> DepositAccounts(CancellationToken cancellationToken) => service.DepositAccountsAsync(cancellationToken);

    [HttpGet("due-cheques")]
    [HasPermission(Permissions.Collection.View)]
    public Task<IReadOnlyList<FctDueChequeDto>> DueCheques(
        [FromQuery] long? depositBankAccountId, [FromQuery] DateOnly? dueFrom, [FromQuery] DateOnly? dueTo, CancellationToken cancellationToken) =>
        service.DueChequesAsync(depositBankAccountId, dueFrom, dueTo, cancellationToken);

    [HttpPost("due-cheques/results")]
    [HasPermission(Permissions.Collection.Edit)]
    public async Task<object> ApplyResults(IReadOnlyList<FctChequeResultDto> rows, CancellationToken cancellationToken) =>
        new { message = await service.ApplyChequeResultsAsync(rows, cancellationToken) };
}
