using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Lookups;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>Generic CRUD for ADM_LKP_* tables. Lookup names come from LookupRegistry.</summary>
[ApiController]
[Route("api/adm/lookups")]
[HasPermission(Permissions.Adm.LookupView)]
public sealed class AdmLookupController(IAdmLookupService lookups) : ControllerBase
{
    [HttpGet]
    public IReadOnlyCollection<string> ListNames() => lookups.LookupNames;

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
}
