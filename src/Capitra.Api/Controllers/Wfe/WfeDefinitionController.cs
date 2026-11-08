using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>Workflow definitions for the BPMN designer.</summary>
[ApiController]
[Route("api/wfe/definitions")]
public sealed class WfeDefinitionController(WfeDefinitions definitions) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeDefinitionDto>> List([FromQuery] string? search, CancellationToken cancellationToken) =>
        definitions.ListAsync(search, cancellationToken);

    [HttpGet("running")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeRunningInstanceDto>> Running(CancellationToken cancellationToken) => definitions.RunningAsync(cancellationToken);

    [HttpGet("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeDefinitionDto> Get(long id, CancellationToken cancellationToken) => definitions.GetAsync(id, cancellationToken);

    [HttpGet("{id:long}/versions")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeVersionDto>> Versions(long id, CancellationToken cancellationToken) => definitions.VersionsAsync(id, cancellationToken);

    [HttpGet("{id:long}/versions/{versionId:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public async Task<WfeVersionXmlDto> VersionXml(long id, long versionId, CancellationToken cancellationToken) =>
        new(await definitions.VersionXmlAsync(id, versionId, cancellationToken));

    [HttpPost]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeDefinitionDto> Create(SaveWfeDefinitionRequest request, CancellationToken cancellationToken) =>
        definitions.SaveAsync(null, request, cancellationToken);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeDefinitionDto> Update(long id, SaveWfeDefinitionRequest request, CancellationToken cancellationToken) =>
        definitions.SaveAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await definitions.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
