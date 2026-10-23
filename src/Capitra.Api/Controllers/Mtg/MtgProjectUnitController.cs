using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Project units (APEX App 105 pages 107/108).</summary>
[ApiController]
[Route("api/mtg/project-units")]
[HasPermission(Permissions.Mtg.MasterDataView)]
public sealed class MtgProjectUnitController(IMtgProjectUnitService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ProjectUnitDto>> List([FromQuery] long? projectId, CancellationToken cancellationToken) =>
        service.ListAsync(projectId, cancellationToken);

    [HttpGet("{projectUnitId:long}")]
    public Task<ProjectUnitDto> Get(long projectUnitId, CancellationToken cancellationToken) =>
        service.GetAsync(projectUnitId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<ActionResult<ProjectUnitDto>> Create(SaveProjectUnitRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectUnitId = created.ProjectUnitId }, created);
    }

    [HttpPut("{projectUnitId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public Task<ProjectUnitDto> Update(long projectUnitId, SaveProjectUnitRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(projectUnitId, request, cancellationToken);

    [HttpDelete("{projectUnitId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<IActionResult> Delete(long projectUnitId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(projectUnitId, rowVersion, cancellationToken);
        return NoContent();
    }
}
