using Capitra.Api.Security;
using Capitra.Application.Modules.Mtg.MasterData;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Mtg;

/// <summary>Projects (APEX App 105 pages 105/106).</summary>
[ApiController]
[Route("api/mtg/projects")]
[HasPermission(Permissions.Mtg.MasterDataView)]
public sealed class MtgProjectController(IMtgProjectService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ProjectDto>> List([FromQuery] long? developerId, [FromQuery] bool activeOnly, CancellationToken cancellationToken) =>
        service.ListAsync(developerId, activeOnly, cancellationToken);

    [HttpGet("{projectId:long}")]
    public Task<ProjectDto> Get(long projectId, CancellationToken cancellationToken) =>
        service.GetAsync(projectId, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<ActionResult<ProjectDto>> Create(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId = created.ProjectId }, created);
    }

    [HttpPut("{projectId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public Task<ProjectDto> Update(long projectId, SaveProjectRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(projectId, request, cancellationToken);

    [HttpDelete("{projectId:long}")]
    [HasPermission(Permissions.Mtg.MasterDataEdit)]
    public async Task<IActionResult> Delete(long projectId, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(projectId, rowVersion, cancellationToken);
        return NoContent();
    }
}
