using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>Workflow forms: laid out by workflow administrators, filled in by every signed-in user on their tasks and start forms.</summary>
[ApiController]
[Route("api/wfe/forms")]
public sealed class WfeFormController(WfeForms forms) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeFormDto>> List(CancellationToken ct) => forms.ListAsync(ct);

    [HttpGet("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeFormDto> Get(long id, CancellationToken ct) => forms.GetAsync(id, ct);

    [HttpGet("workflow/{definitionId:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeFormDto>> OfWorkflow(long definitionId, CancellationToken ct) => forms.OfWorkflowAsync(definitionId, ct);

    [HttpGet("workflow/{definitionId:long}/fields")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeDataField>> Fields(long definitionId, CancellationToken ct) => forms.FieldsAsync(definitionId, ct);

    [HttpPost]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeFormDto> Create(SaveWfeFormRequest request, CancellationToken ct) => forms.SaveAsync(null, request, ct);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeFormDto> Update(long id, SaveWfeFormRequest request, CancellationToken ct) => forms.SaveAsync(id, request, ct);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await forms.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>The released workflows a user can start from a form.</summary>
    [HttpGet("start")]
    public Task<IReadOnlyList<WfeStartFormItemDto>> StartList(CancellationToken ct) => forms.StartListAsync(ct);

    [HttpGet("start/{workflowCode}")]
    public Task<WfeFormViewDto> StartForm(string workflowCode, CancellationToken ct) => forms.StartFormAsync(workflowCode, ct);

    [HttpPost("start/{workflowCode}")]
    public Task<WfeStartedDto> Start(string workflowCode, WfeStartFromFormRequest request, CancellationToken ct) =>
        forms.StartAsync(workflowCode, request, ct);
}
