using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Currencies;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>The currency list of Admin Setup (ADM_LKP_CURRENCIES). Reading is open to signed-in users; changes need ADM.LOOKUP.EDIT.</summary>
[ApiController]
[Route("api/adm/currencies")]
public sealed class AdmCurrencyController(AdmCurrencyService currencies) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CurrencyDto>> List(CancellationToken cancellationToken) => currencies.ListAsync(cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public Task<CurrencyDto> Create(SaveCurrencyRequest request, CancellationToken cancellationToken) =>
        currencies.CreateAsync(request, cancellationToken);

    [HttpPut("{code}")]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public Task<CurrencyDto> Update(string code, SaveCurrencyRequest request, CancellationToken cancellationToken) =>
        currencies.UpdateAsync(code, request, cancellationToken);

    [HttpDelete("{code}")]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public Task Delete(string code, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        currencies.DeleteAsync(code, rowVersion, cancellationToken);
}
