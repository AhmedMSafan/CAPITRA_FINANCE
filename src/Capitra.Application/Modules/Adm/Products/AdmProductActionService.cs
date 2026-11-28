using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

public sealed record ProductActionDto(
    string ActionCode, string ActionNameEn, string ActionNameAr, string Description, string ModuleCode, IReadOnlyList<string> AmountCodes,
    long? ProductActionId, long? ProductEventRuleId, string? RuleCode, string? RuleNameEn, bool IsActive, int? RowVersion, string UsedOn);

public sealed record ProductActionRuleOptionDto(long ProductEventRuleId, string RuleCode, string RuleNameEn, bool IsActive);

/// <summary>
/// A workflow step that posts a rule of the product: the step names the rule in the workflow (finova:ruleCode), so it is linked in the
/// Workflow Designer, not here. RuleFound is false when the product has no rule of that code.
/// </summary>
public sealed record ProductWorkflowStepDto(
    long WorkflowDefinitionId, string WorkflowCode, string WorkflowName, bool WorkflowActive, int StepNo, string StepCode, string StepName, string? Role,
    string RuleCode, string? RuleNameEn, bool RuleActive, string UsedOn);

public sealed record ProductActionsDto(IReadOnlyList<ProductActionDto> Actions, IReadOnlyList<ProductActionRuleOptionDto> Rules,
    IReadOnlyList<ProductWorkflowStepDto> Steps);

public sealed record SaveProductActionRequest(long? ProductEventRuleId, bool IsActive, int? RowVersion);

/// <summary>The Actions of a product in Admin Setup: the fixed actions of the product (<see cref="ProductActions"/>) and the rule each one posts.</summary>
public sealed class AdmProductActionService(ICapitraDbContext db)
{
    public async Task<ProductActionsDto> ListAsync(long productId, CancellationToken cancellationToken)
    {
        var product = await ProductAsync(productId, cancellationToken);
        var links = await (from a in db.Set<AdmProductAction>().AsNoTracking()
                           join r in db.Set<AdmProductEventRule>() on a.ProductEventRuleId equals r.ProductEventRuleId into rs
                           from r in rs.DefaultIfEmpty()
                           where a.ProductId == productId
                           select new { a, RuleCode = r == null ? null : r.RuleCode, RuleNameEn = r == null ? null : r.RuleNameEn })
            .ToListAsync(cancellationToken);
        var actions = ProductActions.All.Where(x => x.ProductCode == product.ProductCode).Select(d =>
        {
            var l = links.FirstOrDefault(x => x.a.ActionCode == d.ActionCode);
            return new ProductActionDto(d.ActionCode, d.ActionNameEn, d.ActionNameAr, d.Description, d.ModuleCode, d.AmountCodes,
                l?.a.ProductActionId, l?.a.ProductEventRuleId, l?.RuleCode, l?.RuleNameEn, l?.a.IsActive ?? false, l?.a.RowVersion, d.UsedOn);
        }).ToList();
        var rules = await (from r in db.Set<AdmProductEventRule>().AsNoTracking()
                           where r.ProductId == productId
                           orderby r.RuleNameEn
                           select new ProductActionRuleOptionDto(r.ProductEventRuleId, r.RuleCode, r.RuleNameEn, r.IsActive))
            .ToListAsync(cancellationToken);
        return new ProductActionsDto(actions, rules, await StepsAsync(productId, cancellationToken));
    }

    /// <summary>
    /// The steps of the workflows that name a rule of this product (finova:ruleCode), in their order in each workflow, with the screen and
    /// button that post it.
    /// </summary>
    public async Task<IReadOnlyList<ProductWorkflowStepDto>> StepsAsync(long productId, CancellationToken cancellationToken)
    {
        var rules = await db.Set<AdmProductEventRule>().AsNoTracking().Where(r => r.ProductId == productId)
            .Select(r => new { r.RuleCode, r.RuleNameEn, r.IsActive }).ToListAsync(cancellationToken);
        var byCode = rules.GroupBy(r => r.RuleCode, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        if (byCode.Count == 0) return [];
        var workflows = await db.Set<WfeWorkflowDefinition>().AsNoTracking().Where(w => w.BpmnXml.Contains("ruleCode"))
            .OrderBy(w => w.WorkflowName).ToListAsync(cancellationToken);
        var result = new List<ProductWorkflowStepDto>();
        foreach (var w in workflows)
        {
            IReadOnlyList<BpmnElement> elements;
            try { elements = BpmnModel.Elements(w.BpmnXml); }
            catch (System.Xml.XmlException) { continue; }
            var steps = elements.Where(e => e.IsHumanTask || !string.IsNullOrWhiteSpace(e.StepCode)).ToList();
            for (var i = 0; i < steps.Count; i++)
            {
                var e = steps[i];
                var code = e.RuleCode?.Trim();
                if (string.IsNullOrEmpty(code) || !byCode.TryGetValue(code, out var rule)) continue;
                var stepCode = e.StepCode ?? e.Id;
                var name = e.Name ?? stepCode;
                result.Add(new ProductWorkflowStepDto(w.WorkflowDefinitionId, w.WorkflowCode, w.WorkflowName, w.IsActive, i + 1, stepCode, name,
                    e.Assignee, rule.RuleCode, rule.RuleNameEn, rule.IsActive, WorkflowStepUses.Of(w.WorkflowCode, stepCode, name)));
            }
        }
        return result;
    }

    /// <summary>Links the action to a rule of the same product (or clears the link), and turns it on or off.</summary>
    public async Task<ProductActionsDto> SaveAsync(long productId, string actionCode, SaveProductActionRequest request, CancellationToken cancellationToken)
    {
        var product = await ProductAsync(productId, cancellationToken);
        var def = ProductActions.Find(actionCode);
        if (def is null || def.ProductCode != product.ProductCode)
            throw new NotFoundException($"Product {product.ProductCode} has no action {actionCode}.");

        var e = new SetupErrors();
        if (request.ProductEventRuleId is { } ruleId
            && !await db.Set<AdmProductEventRule>().AnyAsync(r => r.ProductEventRuleId == ruleId && r.ProductId == productId, cancellationToken))
            e.Add(nameof(request.ProductEventRuleId), "The selected rule does not belong to this product.");
        if (request.IsActive && request.ProductEventRuleId is null)
            e.Add(nameof(request.ProductEventRuleId), "Choose the rule the action posts, or turn the action off.");
        e.ThrowIfAny();

        var entity = await db.Set<AdmProductAction>().FirstOrDefaultAsync(x => x.ActionCode == actionCode, cancellationToken);
        if (entity is null)
        {
            entity = new AdmProductAction { ProductId = productId, ActionCode = actionCode };
            db.Add(entity);
        }
        else if (request.RowVersion != entity.RowVersion)
        {
            throw new ConflictException("This action was changed by another user. Reload and try again.");
        }

        entity.ProductId = productId;
        entity.ProductEventRuleId = request.ProductEventRuleId;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return await ListAsync(productId, cancellationToken);
    }

    private async Task<AdmProduct> ProductAsync(long productId, CancellationToken cancellationToken) =>
        await db.Set<AdmProduct>().AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Product {productId} was not found.");
}

/// <summary>Where a workflow step's rule is posted from, for Admin Setup: the screen and button of the known steps, else the step's task.</summary>
public static class WorkflowStepUses
{
    private static readonly Dictionary<(string Workflow, string Step), string> Known = new()
    {
        [("CF_FINANCE", "CF_CHARGES")] = "Retail Lending › Finance page › Charges and claim step › Create claim button (the upfront charges' GL lines)",
        [("CF_FINANCE", "CF_DELIVERY")] = "Retail Lending › Finance page › Delivery and customer receipt step › Accept (books the finance)",
        [("CF_FINANCE", "CF_SUPPLIER_PAYMENT")] = "Retail Lending › Finance page › Supplier payment step › Accept (pays the supplier)",
    };

    public static string Of(string workflowCode, string stepCode, string stepName) =>
        Known.TryGetValue((workflowCode.ToUpperInvariant(), stepCode.ToUpperInvariant()), out var text) ? text
            : $"Inbox › task \"{stepName}\" › its page › Accept";
}
