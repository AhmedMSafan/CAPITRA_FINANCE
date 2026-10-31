using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>The product catalog of Admin Setup (ADM_PRODUCTS). Reading needs ADM.PRODUCT.VIEW; changes need ADM.PRODUCT.EDIT.</summary>
[ApiController]
[Route("api/adm/products")]
public sealed class AdmProductController(AdmProductService products, AdmProductActionService actions) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<ProductDto>> List(CancellationToken cancellationToken) => products.ListAsync(cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductDto> Create(SaveProductRequest request, CancellationToken cancellationToken) => products.CreateAsync(request, cancellationToken);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductDto> Update(long id, SaveProductRequest request, CancellationToken cancellationToken) =>
        products.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task Delete(long id, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        products.DeleteAsync(id, rowVersion, cancellationToken);

    /// <summary>The Mortgage Portfolio Purchasing product; 204 when it is not in the catalog yet.</summary>
    [HttpGet("mortgage-portfolio-purchasing")]
    [HasPermission(Permissions.Adm.ProductView)]
    public async Task<ActionResult<ProductDto>> GetMortgagePortfolioPurchasing(CancellationToken cancellationToken) =>
        await products.GetMortgagePortfolioPurchasingAsync(cancellationToken) is { } product ? product : NoContent();

    [HttpPut("mortgage-portfolio-purchasing")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductDto> SaveMortgagePortfolioPurchasing(SaveMppParametersRequest request, CancellationToken cancellationToken) =>
        products.SaveMortgagePortfolioPurchasingAsync(request, cancellationToken);

    /// <summary>The product's fixed actions (buttons outside the workflow) and the rule each one posts.</summary>
    [HttpGet("{id:long}/actions")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<ProductActionsDto> Actions(long id, CancellationToken cancellationToken) => actions.ListAsync(id, cancellationToken);

    [HttpPut("{id:long}/actions/{actionCode}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductActionsDto> SaveAction(long id, string actionCode, SaveProductActionRequest request, CancellationToken cancellationToken) =>
        actions.SaveAsync(id, actionCode, request, cancellationToken);
}
