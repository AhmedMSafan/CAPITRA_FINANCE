using Capitra.Api.Security;
using Capitra.Application.Modules.Trs;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Trs;

/// <summary>Treasury Bank Limits: the facility, tranche and sublimit tree with live usage, and the bank rules (verified by a second user).</summary>
[ApiController]
[Route("api/trs/limits")]
public sealed class TrsLimitsController(TrsLimitsService service) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Trs.View)]
    public Task<TrsLimitsDto> Get(CancellationToken cancellationToken) => service.GetAsync(cancellationToken);

    [HttpGet("options")]
    [HasPermission(Permissions.Trs.View)]
    public Task<TrsLimitsOptionsDto> Options(CancellationToken cancellationToken) => service.OptionsAsync(cancellationToken);

    [HttpPost("facilities")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> AddFacility(TrsFacilityFormDto dto, CancellationToken cancellationToken) => service.SaveFacilityAsync(null, dto, cancellationToken);

    [HttpPut("facilities/{facilityId:long}")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> UpdateFacility(long facilityId, TrsFacilityFormDto dto, CancellationToken cancellationToken) =>
        service.SaveFacilityAsync(facilityId, dto, cancellationToken);

    [HttpPost("facilities/{facilityId:long}/tranches")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> AddTranche(long facilityId, TrsTrancheFormDto dto, CancellationToken cancellationToken) =>
        service.SaveTrancheAsync(facilityId, null, dto, cancellationToken);

    [HttpPut("facilities/{facilityId:long}/tranches/{trancheId:long}")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> UpdateTranche(long facilityId, long trancheId, TrsTrancheFormDto dto, CancellationToken cancellationToken) =>
        service.SaveTrancheAsync(facilityId, trancheId, dto, cancellationToken);

    [HttpPost("tranches/{trancheId:long}/sublimits")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> AddSublimit(long trancheId, TrsSublimitFormDto dto, CancellationToken cancellationToken) =>
        service.SaveSublimitAsync(trancheId, null, dto, cancellationToken);

    [HttpPut("tranches/{trancheId:long}/sublimits/{sublimitId:long}")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> UpdateSublimit(long trancheId, long sublimitId, TrsSublimitFormDto dto, CancellationToken cancellationToken) =>
        service.SaveSublimitAsync(trancheId, sublimitId, dto, cancellationToken);

    [HttpPost("rules")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> AddRule(TrsBankRuleFormDto dto, CancellationToken cancellationToken) => service.SaveRuleAsync(null, dto, cancellationToken);

    [HttpPut("rules/{ruleId:long}")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task<long> UpdateRule(long ruleId, TrsBankRuleFormDto dto, CancellationToken cancellationToken) => service.SaveRuleAsync(ruleId, dto, cancellationToken);

    [HttpPost("rules/{ruleId:long}/verify")]
    [HasPermission(Permissions.Trs.Verify)]
    public Task VerifyRule(long ruleId, CancellationToken cancellationToken) => service.VerifyRuleAsync(ruleId, cancellationToken);

    [HttpPost("rules/{ruleId:long}/deactivate")]
    [HasPermission(Permissions.Trs.Edit)]
    public Task DeactivateRule(long ruleId, TrsNotesDto dto, CancellationToken cancellationToken) => service.DeactivateRuleAsync(ruleId, dto, cancellationToken);
}
