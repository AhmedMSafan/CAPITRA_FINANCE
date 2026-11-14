using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>Data sources for workflow forms: the built-in ones and the lists workflow administrators keep.</summary>
[ApiController]
[Route("api/wfe/data-sources")]
public sealed class WfeDataSourceController(WfeDataSources sources) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeDataSourceDto>> List(CancellationToken ct) => sources.ListAsync(ct);

    [HttpGet("{code}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeDataSourceDto> Get(string code, CancellationToken ct) => sources.GetAsync(code, ct);

    [HttpGet("{code}/options")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeOptionDto>> Options(string code, CancellationToken ct) => sources.OptionsAsync(code, ct);

    [HttpPost]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeDataSourceDto> Create(SaveWfeDataSourceRequest request, CancellationToken ct) => sources.SaveAsync(null, request, ct);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeDataSourceDto> Update(long id, SaveWfeDataSourceRequest request, CancellationToken ct) => sources.SaveAsync(id, request, ct);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await sources.DeleteAsync(id, ct);
        return NoContent();
    }
}
