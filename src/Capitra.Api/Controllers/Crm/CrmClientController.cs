using Capitra.Api.Security;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Crm;

/// <summary>The CRM clients (FINOVA_FINANCEING api/clients). Reading needs CRM.VIEW; create and update need CRM.EDIT.</summary>
[ApiController]
[Route("api/crm/clients")]
public sealed class CrmClientController(CrmClientService clients) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmClientListDto>> List(CancellationToken cancellationToken) => clients.ListAsync(cancellationToken);

    [HttpGet("options")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmClientOptionsDto> Options(CancellationToken cancellationToken) => clients.OptionsAsync(cancellationToken);

    /// <summary>The customers the signed-in user opened last in Customer 360.</summary>
    [HttpGet("recent")]
    [HasPermission(Permissions.Crm.View)]
    public Task<IReadOnlyList<CrmRecentClientDto>> Recent(CancellationToken cancellationToken) => clients.RecentAsync(cancellationToken);

    /// <summary>Customer 360 calls this when it opens a customer, for "Recently opened".</summary>
    [HttpPost("{clientId:long}/opened")]
    [HasPermission(Permissions.Crm.View)]
    public Task Opened(long clientId, CancellationToken cancellationToken) => clients.OpenedAsync(clientId, cancellationToken);

    [HttpGet("{clientId:long}")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmClientFormDto> Get(long clientId, CancellationToken cancellationToken) => clients.GetAsync(clientId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmClientFormDto> Create(CrmClientFormDto dto, CancellationToken cancellationToken) => clients.CreateAsync(dto, cancellationToken);

    [HttpPut("{clientId:long}")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<CrmClientFormDto> Update(long clientId, CrmClientFormDto dto, CancellationToken cancellationToken) =>
        clients.UpdateAsync(clientId, dto, cancellationToken);
}
