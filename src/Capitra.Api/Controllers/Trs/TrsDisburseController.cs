using Capitra.Api.Security;
using Capitra.Application.Modules.Trs;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Trs;

/// <summary>Treasury Disburse: choose, reserve and execute the source of funds of a disbursement, checked against limits and bank rules.</summary>
[ApiController]
[Route("api/trs/disbursements/{disbursementId:long}/funding")]
public sealed class TrsDisburseController(TrsDisburseService service) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Trs.View)]
    public Task<TrsDisburseDto> Get(long disbursementId, CancellationToken cancellationToken) => service.GetAsync(disbursementId, cancellationToken);

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
}
