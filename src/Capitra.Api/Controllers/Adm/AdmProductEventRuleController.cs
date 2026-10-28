using Capitra.Api.Security;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Adm;

/// <summary>Product event rules with their charges and GL lines (ADM_PRODUCT_EVENT_RULES). Reading needs ADM.PRODUCT.VIEW; changes need ADM.PRODUCT.EDIT.</summary>
[ApiController]
[Route("api/adm/product-event-rules")]
public sealed class AdmProductEventRuleController(AdmProductEventRuleService rules) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<ProductEventRuleDto>> List(CancellationToken cancellationToken) => rules.ListAsync(cancellationToken);

    [HttpGet("event-types")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<EventTypeOptionDto>> ListEventTypes(CancellationToken cancellationToken) => rules.ListEventTypesAsync(cancellationToken);

    [HttpGet("workflow-steps")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<WorkflowOptionDto>> ListWorkflowSteps(CancellationToken cancellationToken) => rules.ListWorkflowStepsAsync(cancellationToken);

    [HttpGet("gl-accounts")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<GlAccountOptionDto>> ListGlAccounts(CancellationToken cancellationToken) => rules.ListGlAccountsAsync(cancellationToken);

    [HttpGet("{id:long}")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<ProductEventRuleDto> Get(long id, CancellationToken cancellationToken) => rules.GetAsync(id, cancellationToken);

    [HttpPost]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductEventRuleDto> Create(SaveProductEventRuleRequest request, CancellationToken cancellationToken) =>
        rules.CreateAsync(request, cancellationToken);

    [HttpPut("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<ProductEventRuleDto> Update(long id, SaveProductEventRuleRequest request, CancellationToken cancellationToken) =>
        rules.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task Delete(long id, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        rules.DeleteAsync(id, rowVersion, cancellationToken);

    [HttpGet("{ruleId:long}/charges")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<RuleChargeDto>> ListCharges(long ruleId, CancellationToken cancellationToken) =>
        rules.ListChargesAsync(ruleId, cancellationToken);

    [HttpPost("{ruleId:long}/charges")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RuleChargeDto> CreateCharge(long ruleId, SaveRuleChargeRequest request, CancellationToken cancellationToken) =>
        rules.CreateChargeAsync(ruleId, request, cancellationToken);

    [HttpPut("{ruleId:long}/charges/{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RuleChargeDto> UpdateCharge(long ruleId, long id, SaveRuleChargeRequest request, CancellationToken cancellationToken) =>
        rules.UpdateChargeAsync(ruleId, id, request, cancellationToken);

    [HttpDelete("{ruleId:long}/charges/{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task DeleteCharge(long ruleId, long id, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        rules.DeleteChargeAsync(ruleId, id, rowVersion, cancellationToken);

    [HttpGet("{ruleId:long}/gl-lines")]
    [HasPermission(Permissions.Adm.ProductView)]
    public Task<IReadOnlyList<RuleGlLineDto>> ListGlLines(long ruleId, CancellationToken cancellationToken) =>
        rules.ListGlLinesAsync(ruleId, cancellationToken);

    [HttpPost("{ruleId:long}/gl-lines")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RuleGlLineDto> CreateGlLine(long ruleId, SaveRuleGlLineRequest request, CancellationToken cancellationToken) =>
        rules.CreateGlLineAsync(ruleId, request, cancellationToken);

    [HttpPut("{ruleId:long}/gl-lines/{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task<RuleGlLineDto> UpdateGlLine(long ruleId, long id, SaveRuleGlLineRequest request, CancellationToken cancellationToken) =>
        rules.UpdateGlLineAsync(ruleId, id, request, cancellationToken);

    [HttpDelete("{ruleId:long}/gl-lines/{id:long}")]
    [HasPermission(Permissions.Adm.ProductEdit)]
    public Task DeleteGlLine(long ruleId, long id, [FromQuery] int rowVersion, CancellationToken cancellationToken) =>
        rules.DeleteGlLineAsync(ruleId, id, rowVersion, cancellationToken);
}
