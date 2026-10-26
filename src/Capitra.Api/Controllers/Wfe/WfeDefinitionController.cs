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

    [HttpGet("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeDefinitionDto> Get(long id, CancellationToken cancellationToken) => definitions.GetAsync(id, cancellationToken);

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
