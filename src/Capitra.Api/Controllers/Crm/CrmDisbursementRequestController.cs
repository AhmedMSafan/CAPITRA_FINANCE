using Capitra.Api.Security;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Crm;

/// <summary>Customer Disbursement Request (APEX App 150722 pages 750 and 751). Reading needs CRM.VIEW; saving needs CRM.EDIT.</summary>
[ApiController]
[Route("api/crm/disbursement-requests")]
public sealed class CrmDisbursementRequestController(CrmDisbursementRequestService service) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmDisbursementRequestListDto>> List([FromQuery] long? clientId, CancellationToken cancellationToken) =>
        service.ListAsync(clientId, cancellationToken);

    [HttpGet("{disbursementId:long}")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmDisbursementRequestDto> Get(long disbursementId, CancellationToken cancellationToken) => service.GetAsync(disbursementId, cancellationToken);

    [HttpGet("clients")]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmOptionDto>> Clients(CancellationToken cancellationToken) => service.ClientsAsync(cancellationToken);

    [HttpGet("clients/{clientId:long}/limits")]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmOptionDto>> Limits(long clientId, CancellationToken cancellationToken) => service.LimitsAsync(clientId, cancellationToken);

    [HttpGet("currencies")]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmOptionDto>> Currencies(CancellationToken cancellationToken) => service.CurrenciesAsync(cancellationToken);

    /// <summary>The Application Summary of a limit's agreement; 204 when the limit has none.</summary>
    [HttpGet("limits/{limitId:long}/agreement-summary")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmAgreementSummaryDto?> LimitAgreementSummary(long limitId, CancellationToken cancellationToken) =>
        service.AgreementSummaryAsync(limitId, cancellationToken);

    [HttpGet("agreements/{agreementId:long}/summary")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmAgreementSummaryDto?> AgreementSummary(long agreementId, CancellationToken cancellationToken) =>
        service.SummaryAsync(agreementId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmDisbursementRequestSavedDto> Create(CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken) =>
        service.CreateAsync(dto, cancellationToken);

    [HttpPut("{disbursementId:long}")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmDisbursementRequestDto> Update(long disbursementId, CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken) =>
        service.UpdateAsync(disbursementId, dto, cancellationToken);

    [HttpDelete("{disbursementId:long}")]
    [HasPermission(Permissions.Crm.Edit)]
    public async Task<IActionResult> Delete(long disbursementId, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(disbursementId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{disbursementId:long}/start-workflow")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmDisbursementRequestSavedDto> StartWorkflow(long disbursementId, CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken) =>
        service.StartWorkflowAsync(disbursementId, dto, cancellationToken);
}
