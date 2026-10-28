using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>The charges of Admin Setup (ADM_CHARGES). Reading needs ADM.PRODUCT.VIEW; changes need ADM.PRODUCT.EDIT.</summary>
[ApiController]
[Route("api/adm/charges")]
public sealed class AdmChargeController(AdmChargeService charges) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<ChargeDto>> List(CancellationToken cancellationToken) => charges.ListAsync(cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ChargeDto> Create(SaveChargeRequest request, CancellationToken cancellationToken) => charges.CreateAsync(request, cancellationToken);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ChargeDto> Update(long id, SaveChargeRequest request, CancellationToken cancellationToken) =>
        charges.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task Delete(long id, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        charges.DeleteAsync(id, rowVersion, cancellationToken);
}
