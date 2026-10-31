using Capitra.Api.Security;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers;

/// <summary>
/// Runs the on-demand processes of the copied APEX pages (what apex.server.process called in APEX).
/// Each process checks its own permission, because one route serves every page.
/// </summary>
[ApiController]
[Authorize]
[Route("api/apex/{app:int}/{page:int}/{process}")]
public sealed class ApexController(ApexProcessRegistry registry, IAuthorizationService authorization) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Run(int app, int page, string process, ApexRequest request, CancellationToken cancellationToken)
    {
        var handler = registry.Find(app, page, process);
        if (handler is null)
            return NotFound();

        var allowed = await authorization.AuthorizeAsync(User, PermissionPolicyProvider.PolicyPrefix + handler.Permission);
        // on a workflow step page, the step's permission does as well
        if (!allowed.Succeeded && StepPermissions.For(app, page, handler.Permission) is { } step)
            allowed = await authorization.AuthorizeAsync(User, PermissionPolicyProvider.PolicyPrefix + step);
        if (!allowed.Succeeded)
            return Forbid();

        return Ok(await handler.RunAsync(request, cancellationToken));
    }
}
