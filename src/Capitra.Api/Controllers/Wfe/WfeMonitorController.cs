using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>The process monitor: requests and where they are, one request's process view, and the time spent per step and person.</summary>
[ApiController]
[Route("api/wfe/monitor")]
public sealed class WfeMonitorController(WfeMonitor monitor) : ControllerBase
{
    [HttpGet("instances")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<IReadOnlyList<WfeMonitorRowDto>> List([FromQuery] string? status, [FromQuery] long? workflowId, [FromQuery] string? search, CancellationToken ct) =>
        monitor.ListAsync(status, workflowId, search, ct);

    [HttpGet("instances/{id:long}")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeMonitorInstanceDto> Instance(long id, CancellationToken ct) => monitor.InstanceAsync(id, ct);

    [HttpGet("times")]
    [HasPermission(Permissions.Wfe.DefinitionView)]
    public Task<WfeTimeReportDto> Times([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] long? workflowId, CancellationToken ct) =>
        monitor.TimesAsync(from, to, workflowId, ct);
}
