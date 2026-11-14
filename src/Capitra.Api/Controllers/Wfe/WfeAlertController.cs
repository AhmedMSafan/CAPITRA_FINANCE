using Capitra.Api.Security;
using Capitra.Application.Modules.Wfe;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Wfe;

/// <summary>The bell in the page header: the signed-in user's task-deadline alerts (due soon, overdue, escalated).</summary>
[ApiController]
[Route("api/wfe/alerts")]
public sealed class WfeAlertController(WfeAlerts alerts, WfeSla sla) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<WfeAlertDto>> Mine(CancellationToken ct) => alerts.MineAsync(ct);

    [HttpPost("{alertId:long}/read")]
    public Task Read(long alertId, CancellationToken ct) => alerts.ReadAsync(alertId, ct);

    [HttpPost("read-all")]
    public Task ReadAll(CancellationToken ct) => alerts.ReadAsync(null, ct);

    /// <summary>Runs the deadline check now (it also runs every minute on its own).</summary>
    [HttpPost("check")]
    [HasPermission(Permissions.Wfe.DefinitionEdit)]
    public Task<WfeSlaRunResult> Check(CancellationToken ct) => sla.RunAsync(ct);
}
