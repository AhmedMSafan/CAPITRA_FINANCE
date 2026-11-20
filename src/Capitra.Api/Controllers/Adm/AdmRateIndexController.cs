using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Rates;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>Admin Setup rate maintenance: rate indexes and their rate periods. Reading needs ADM.PRODUCT.VIEW, changes ADM.PRODUCT.EDIT.</summary>
[ApiController]
[Route("api/adm/rate-indexes")]
public sealed class AdmRateIndexController(AdmRateIndexService rates) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<RateIndexDto>> List(CancellationToken cancellationToken) => rates.ListAsync(cancellationToken);

    [HttpGet("currencies")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<RateCurrencyDto>> Currencies(CancellationToken cancellationToken) => rates.CurrenciesAsync(cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RateIndexDto> Create(SaveRateIndexRequest request, CancellationToken cancellationToken) => rates.CreateAsync(request, cancellationToken);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RateIndexDto> Update(long id, SaveRateIndexRequest request, CancellationToken cancellationToken) =>
        rates.UpdateAsync(id, request, cancellationToken);

    [HttpGet("{id:long}/periods")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<RatePeriodDto>> Periods(long id, CancellationToken cancellationToken) => rates.PeriodsAsync(id, cancellationToken);

    [HttpPost("{id:long}/periods")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<IReadOnlyList<RatePeriodDto>> AddPeriod(long id, SaveRatePeriodRequest request, CancellationToken cancellationToken) =>
        rates.AddPeriodAsync(id, request, cancellationToken);

    [HttpPut("{id:long}/periods/{valueId:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<IReadOnlyList<RatePeriodDto>> UpdatePeriod(long id, long valueId, SaveRatePeriodRequest request, CancellationToken cancellationToken) =>
        rates.UpdatePeriodAsync(id, valueId, request, cancellationToken);

    [HttpDelete("{id:long}/periods/{valueId:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<IReadOnlyList<RatePeriodDto>> DeletePeriod(long id, long valueId, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        rates.DeletePeriodAsync(id, valueId, rowVersion, cancellationToken);
}
