using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>Out of office: every signed-in user sets their own; workflow administrators can set and see everyone's.</summary>
[ApiController]
[Route("api/wfe/out-of-office")]
public sealed class WfeOutOfOfficeController(WfeOutOfOfficeService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<WfeOutOfOfficeDto>> Mine(CancellationToken ct) => service.ListAsync(false, ct);

    [HttpGet("all")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<IReadOnlyList<WfeOutOfOfficeDto>> All(CancellationToken ct) => service.ListAsync(true, ct);

    [HttpGet("covering")]
    public Task<IReadOnlyList<WfeCoverDto>> Covering(CancellationToken ct) => service.CoveringAsync(ct);

    [HttpGet("users")]
    public Task<IReadOnlyList<WfeUserOptionDto>> Users(CancellationToken ct) => service.UsersAsync(ct);

    [HttpGet("workflows")]
    public Task<IReadOnlyList<WfeWorkflowOptionDto>> Workflows(CancellationToken ct) => service.WorkflowsAsync(ct);

    [HttpPost]
    public Task<WfeOutOfOfficeDto> Create(SaveWfeOutOfOfficeRequest request, CancellationToken ct) => service.SaveAsync(null, request, false, ct);

    [HttpPut("{id:long}")]
    public Task<WfeOutOfOfficeDto> Update(long id, SaveWfeOutOfOfficeRequest request, CancellationToken ct) => service.SaveAsync(id, request, false, ct);

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, false, ct);
        return NoContent();
    }

    [HttpPost("admin")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeOutOfOfficeDto> AdminCreate(SaveWfeOutOfOfficeRequest request, CancellationToken ct) => service.SaveAsync(null, request, true, ct);

    [HttpPut("admin/{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeOutOfOfficeDto> AdminUpdate(long id, SaveWfeOutOfOfficeRequest request, CancellationToken ct) => service.SaveAsync(id, request, true, ct);

    [HttpDelete("admin/{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public async Task<IActionResult> AdminDelete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, true, ct);
        return NoContent();
    }
}
