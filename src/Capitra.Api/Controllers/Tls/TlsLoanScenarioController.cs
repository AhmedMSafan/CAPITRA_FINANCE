using Capitra.Api.Security;
using Capitra.Application.Modules.Tls;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Tls;

/// <summary>Tools > Loan Calculator. Needs TLS.CALCULATOR.USE; a user sees only the scenarios they created.</summary>
[ApiController]
[Route("api/tls/loan-scenarios")]
[HasPermission(Permissions.Tls.Calculator)]
public sealed class TlsLoanScenarioController(TlsLoanScenarioService scenarios) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<TlsScenarioListItem>> List(CancellationToken cancellationToken) => scenarios.ListAsync(cancellationToken);

    [HttpGet("{id:long}")]
    public Task<TlsScenarioResult> Get(long id, CancellationToken cancellationToken) => scenarios.GetAsync(id, cancellationToken);

    /// <summary>The active rate indexes with the rate valid on the date (today when none is given).</summary>
    [HttpGet("rates")]
    public Task<IReadOnlyList<TlsRateOptionDto>> Rates([FromQuery] DateOnly? date, CancellationToken cancellationToken) =>
        scenarios.RatesAsync(date, cancellationToken);

    [HttpPost("preview")]
    public TlsScenarioResult Preview(TlsScenarioRequest request) => scenarios.Preview(request);

    [HttpPost("save")]
    public Task<TlsScenarioResult> Save(TlsScenarioRequest request, CancellationToken cancellationToken) => scenarios.SaveAsync(request, cancellationToken);
}
