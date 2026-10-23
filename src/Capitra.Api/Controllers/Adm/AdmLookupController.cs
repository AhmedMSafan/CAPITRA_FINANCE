using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>
/// Generic CRUD for the lookup tables listed in LookupRegistry (APEX App 105 pages 3, 11 to 40).
/// Any signed-in user can read lookups, because every screen's drop-downs use them; changes need ADM.LOOKUP.EDIT.
/// </summary>
[ApiController]
[Route("api/adm/lookups")]
public sealed class AdmLookupController(IAdmLookupService lookups) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<LookupDefinitionDto> Definitions() => lookups.Definitions;

    [HttpGet("{lookupName}")]
    public Task<IReadOnlyList<LookupDto>> List(string lookupName, [FromQuery] bool includeInactive, CancellationToken cancellationToken) =>
        lookups.ListAsync(lookupName, includeInactive, cancellationToken);

    [HttpGet("{lookupName}/{code}")]
    public Task<LookupDto> Get(string lookupName, string code, CancellationToken cancellationToken) =>
        lookups.GetAsync(lookupName, code, cancellationToken);

    [HttpPost("{lookupName}")]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public async Task<ActionResult<LookupDto>> Create(string lookupName, SaveLookupRequest request, CancellationToken cancellationToken)
    {
        var created = await lookups.CreateAsync(lookupName, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { lookupName, code = created.Code }, created);
    }

    [HttpPut("{lookupName}/{code}")]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public Task<LookupDto> Update(string lookupName, string code, SaveLookupRequest request, CancellationToken cancellationToken) =>
        lookups.UpdateAsync(lookupName, code, request, cancellationToken);

    [HttpDelete("{lookupName}/{code}")]
    [HasPermission(Permissions.Adm.LookupEdit)]
    public async Task<IActionResult> Delete(string lookupName, string code, [FromQuery] int rowVersion, CancellationToken cancellationToken)
    {
        await lookups.DeleteAsync(lookupName, code, rowVersion, cancellationToken);
        return NoContent();
    }
}
