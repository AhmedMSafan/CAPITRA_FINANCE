using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Developer signatories (APEX App 105 pages 103/104).</summary>
[ApiController]
[Route("api/mtg/developer-signatories")]
[HasPermission(Permissions.Mtg.MasterDataView)]
public sealed class MtgDeveloperSignatoryController(IMtgDeveloperSignatoryService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<DeveloperSignatoryDto>> List([FromQuery] long? developerId, CancellationToken cancellationToken) =>
        service.ListAsync(developerId, cancellationToken);

    [HttpGet("{developerSignatoryId:long}")]
    public Task<DeveloperSignatoryDto> Get(long developerSignatoryId, CancellationToken cancellationToken) =>
        service.GetAsync(developerSignatoryId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<ActionResult<DeveloperSignatoryDto>> Create(SaveDeveloperSignatoryRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { developerSignatoryId = created.DeveloperSignatoryId }, created);
    }

    [HttpPut("{developerSignatoryId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public Task<DeveloperSignatoryDto> Update(long developerSignatoryId, SaveDeveloperSignatoryRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(developerSignatoryId, request, cancellationToken);

    [HttpDelete("{developerSignatoryId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<IActionResult> Delete(long developerSignatoryId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(developerSignatoryId, rowVersion, cancellationToken);
        return NoContent();
    }
}
