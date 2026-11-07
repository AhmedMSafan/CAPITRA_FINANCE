using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Settings;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>Administration > Module Settings. Reading needs ADM.SETTINGS.VIEW; asking for and deciding a change needs ADM.SETTINGS.EDIT.</summary>
[ApiController]
[Route("api/adm/module-settings")]
public sealed class AdmModuleSettingController(AdmModuleSettingService settings) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.SettingsView)]
    public Task<ModuleSettingsPageDto> Get(CancellationToken cancellationToken) => settings.GetAsync(cancellationToken);

    [HttpPost("{code}/request")]
    [HasPermission(Permissions.Adm.SettingsEdit)]
    public async Task<ModuleSettingsPageDto> RequestChange(string code, ModuleSettingRequestDto dto, CancellationToken cancellationToken)
    {
        await settings.RequestAsync(code, dto, cancellationToken);
        return await settings.GetAsync(cancellationToken);
    }

    [HttpPost("{code}/approve")]
    [HasPermission(Permissions.Adm.SettingsEdit)]
    public async Task<ModuleSettingsPageDto> Approve(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        await settings.ApproveAsync(code, dto, cancellationToken);
        return await settings.GetAsync(cancellationToken);
    }

    [HttpPost("{code}/reject")]
    [HasPermission(Permissions.Adm.SettingsEdit)]
    public async Task<ModuleSettingsPageDto> Reject(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        await settings.RejectAsync(code, dto, cancellationToken);
        return await settings.GetAsync(cancellationToken);
    }

    [HttpPost("{code}/cancel")]
    [HasPermission(Permissions.Adm.SettingsEdit)]
    public async Task<ModuleSettingsPageDto> Cancel(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        await settings.CancelAsync(code, dto, cancellationToken);
        return await settings.GetAsync(cancellationToken);
    }
}
