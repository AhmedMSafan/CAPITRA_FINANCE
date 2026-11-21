using Capitra.Api.Security;
using Capitra.Application.Modules.Eod;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Eod;

public sealed record EodRunResponse(EodRunResultDto Result, EodPageDto Page);

/// <summary>
/// End of Day: the night batch of every application. Reading needs EOD.VIEW; running EOD.RUN; changing the jobs EOD.JOBS.EDIT.
/// </summary>
[ApiController]
[Route("api/eod")]
public sealed class EodController(EodService eod) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Eod.View)]
    public Task<EodPageDto> Get(CancellationToken ct) => eod.GetAsync(Has(Permissions.Eod.Run), ct);

    /// <summary>Runs the days in order from the next one not done up to the day given (today when none).</summary>
    [HttpPost("run")]
    [HasPermission(Permissions.Eod.Run)]
    public async Task<EodRunResponse> Run(EodRunRequest request, CancellationToken ct)
    {
        var result = await eod.RunAsync(request, ct);
        return new EodRunResponse(result, await eod.GetAsync(true, ct));
    }

    [HttpGet("jobs")]
    [HasPermission(Permissions.Eod.View)]
    public Task<IReadOnlyList<EodJobDto>> Jobs(CancellationToken ct) => eod.JobsAsync(ct);

    [HttpPut("jobs/{jobCode}")]
    [HasPermission(Permissions.Eod.JobsEdit)]
    public Task<EodJobDto> SaveJob(string jobCode, EodJobRequest request, CancellationToken ct) => eod.SaveJobAsync(jobCode, request, ct);

    private bool Has(string permission) => User.Claims.Any(c => c.Type == Permissions.ClaimType
        && (c.Value == Permissions.All || string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)));
}
