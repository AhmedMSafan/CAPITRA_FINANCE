using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

public sealed record ProductActionDto(
    string ActionCode, string ActionNameEn, string ActionNameAr, string Description, string ModuleCode, IReadOnlyList<string> AmountCodes,
    long? ProductActionId, long? ProductEventRuleId, string? RuleCode, string? RuleNameEn, string? EventTypeCode, bool IsActive, int? RowVersion);

public sealed record ProductActionRuleOptionDto(long ProductEventRuleId, string RuleCode, string RuleNameEn, string EventTypeCode, bool IsActive);

public sealed record ProductActionsDto(IReadOnlyList<ProductActionDto> Actions, IReadOnlyList<ProductActionRuleOptionDto> Rules);

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
                           join t in db.Set<GlAccountingEventType>() on r.EventTypeId equals t.EventTypeId into ts
                           from t in ts.DefaultIfEmpty()
                           where a.ProductId == productId
                           select new { a, RuleCode = r == null ? null : r.RuleCode, RuleNameEn = r == null ? null : r.RuleNameEn, EventTypeCode = t == null ? null : t.EventTypeCode })
            .ToListAsync(cancellationToken);
        var actions = ProductActions.All.Where(x => x.ProductCode == product.ProductCode).Select(d =>
        {
            var l = links.FirstOrDefault(x => x.a.ActionCode == d.ActionCode);
            return new ProductActionDto(d.ActionCode, d.ActionNameEn, d.ActionNameAr, d.Description, d.ModuleCode, d.AmountCodes,
                l?.a.ProductActionId, l?.a.ProductEventRuleId, l?.RuleCode, l?.RuleNameEn, l?.EventTypeCode, l?.a.IsActive ?? false, l?.a.RowVersion);
        }).ToList();
        var rules = await (from r in db.Set<AdmProductEventRule>().AsNoTracking()
                           join t in db.Set<GlAccountingEventType>() on r.EventTypeId equals t.EventTypeId
                           where r.ProductId == productId
                           orderby r.RuleNameEn
                           select new ProductActionRuleOptionDto(r.ProductEventRuleId, r.RuleCode, r.RuleNameEn, t.EventTypeCode, r.IsActive))
            .ToListAsync(cancellationToken);
        return new ProductActionsDto(actions, rules);
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
