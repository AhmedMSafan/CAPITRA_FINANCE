using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Real estate developers (APEX App 105 pages 101/102).</summary>
[ApiController]
[Route("api/mtg/developers")]
[HasPermission(Permissions.Mtg.MasterDataView)]
public sealed class MtgDeveloperController(IMtgDeveloperService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<DeveloperDto>> List([FromQuery] bool activeOnly, CancellationToken cancellationToken) =>
        service.ListAsync(activeOnly, cancellationToken);

    [HttpGet("{developerId:long}")]
    public Task<DeveloperDto> Get(long developerId, CancellationToken cancellationToken) =>
        service.GetAsync(developerId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<ActionResult<DeveloperDto>> Create(SaveDeveloperRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { developerId = created.DeveloperId }, created);
    }

    [HttpPut("{developerId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public Task<DeveloperDto> Update(long developerId, SaveDeveloperRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(developerId, request, cancellationToken);

    [HttpDelete("{developerId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<IActionResult> Delete(long developerId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(developerId, rowVersion, cancellationToken);
        return NoContent();
    }
}
