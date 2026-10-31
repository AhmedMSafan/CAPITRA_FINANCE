using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

public sealed record ProductDto(
    long ProductId, string ProductCode, string ProductNameEn, string? ProductNameAr, string BusinessLineCode, string ProductTypeCode,
    string? FactoringModeCode, bool? IsRecourse, string? DefaultCurrencyCode, DateOnly EffectiveFromDate, DateOnly? EffectiveToDate,
    bool IsActive, decimal MppPvRate, decimal MppLateFeeRate, decimal MppReservedAmountPct, decimal MppInvestorPaymentPct, int RowVersion);

public sealed record SaveProductRequest(
    string? ProductCode, string? ProductNameEn, string? ProductNameAr, string? BusinessLineCode, string? ProductTypeCode,
    string? FactoringModeCode, bool? IsRecourse, string? DefaultCurrencyCode, DateOnly? EffectiveFromDate, DateOnly? EffectiveToDate,
    bool IsActive, int? RowVersion);

public sealed record SaveMppParametersRequest(
    decimal? MppPvRate, decimal? MppLateFeeRate, decimal? MppReservedAmountPct, decimal? MppInvestorPaymentPct, int? RowVersion);

/// <summary>
/// ADM_PRODUCTS, the Product Catalog of Admin Setup (FINOVA_FINANCEING ProductsPage and ProductService), and the
/// Mortgage Portfolio Purchasing product page that keeps its four rates on the same row.
/// </summary>
public sealed class AdmProductService(ICapitraDbContext db)
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await db.Set<AdmProduct>().AsNoTracking().OrderBy(x => x.ProductNameEn).ToListAsync(cancellationToken)).Select(ToDto)];

    public async Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken cancellationToken)
    {
        var entity = new AdmProduct();
        await ApplyAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ProductDto> UpdateAsync(long id, SaveProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser();

        await ApplyAsync(entity, request, cancellationToken);
        await SaveAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser();
        if (await db.Set<AdmProductEventRule>().AnyAsync(x => x.ProductId == id, cancellationToken))
            throw new ConflictException("This product is referenced by other records and cannot be deleted. Deactivate it instead.");

        db.Remove(entity);
        await SaveAsync(cancellationToken);
    }

    /// <summary>The Mortgage Portfolio Purchasing product, or null when it has not been created in the catalog yet.</summary>
    public async Task<ProductDto?> GetMortgagePortfolioPurchasingAsync(CancellationToken cancellationToken)
    {
        var entity = await db.Set<AdmProduct>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductCode == AdmProduct.MortgagePortfolioPurchasingCode, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<ProductDto> SaveMortgagePortfolioPurchasingAsync(SaveMppParametersRequest r, CancellationToken cancellationToken)
    {
        var entity = await db.Set<AdmProduct>().FirstOrDefaultAsync(x => x.ProductCode == AdmProduct.MortgagePortfolioPurchasingCode, cancellationToken)
            ?? throw new NotFoundException($"Create product {AdmProduct.MortgagePortfolioPurchasingCode} in the product catalog first.");
        if (r.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser();

        var e = new SetupErrors();
        foreach (var (field, label, value) in new[]
                 {
                     (nameof(r.MppPvRate), "PV Rate", r.MppPvRate),
                     (nameof(r.MppLateFeeRate), "Late Fee Rate", r.MppLateFeeRate),
                     (nameof(r.MppReservedAmountPct), "Reserved Amount Percent", r.MppReservedAmountPct),
                     (nameof(r.MppInvestorPaymentPct), "Investor Payment Percent", r.MppInvestorPaymentPct),
                 })
        {
            if (value is null)
                e.Add(field, $"{label} is required.");
            e.Percent(field, label, value);
        }
        e.ThrowIfAny();

        entity.MppPvRate = r.MppPvRate!.Value;
        entity.MppLateFeeRate = r.MppLateFeeRate!.Value;
        entity.MppReservedAmountPct = r.MppReservedAmountPct!.Value;
        entity.MppInvestorPaymentPct = r.MppInvestorPaymentPct!.Value;
        await SaveAsync(cancellationToken);
        return ToDto(entity);
    }

    private async Task ApplyAsync(AdmProduct entity, SaveProductRequest r, CancellationToken cancellationToken)
    {
        var e = new SetupErrors();
        var code = e.Required(nameof(r.ProductCode), "Product code", r.ProductCode, 50, upper: true);
        var nameEn = e.Required(nameof(r.ProductNameEn), "Name (EN)", r.ProductNameEn, 200);
        var nameAr = e.Optional(nameof(r.ProductNameAr), "Name (AR)", r.ProductNameAr, 200);
        var line = e.Required(nameof(r.BusinessLineCode), "Business line", r.BusinessLineCode, 30, upper: true);
        var type = e.Required(nameof(r.ProductTypeCode), "Product type", r.ProductTypeCode, 30, upper: true);
        var mode = e.Optional(nameof(r.FactoringModeCode), "Factoring mode", r.FactoringModeCode, 20, upper: true);
        var currency = e.Optional(nameof(r.DefaultCurrencyCode), "Default currency", r.DefaultCurrencyCode, 3, upper: true);
        if (r.EffectiveFromDate is null)
            e.Add(nameof(r.EffectiveFromDate), "Effective From is required.");
        e.Dates(nameof(r.EffectiveToDate), r.EffectiveFromDate, r.EffectiveToDate);

        if (code.Length > 0 && await db.Set<AdmProduct>().AnyAsync(x => x.ProductCode == code && x.ProductId != entity.ProductId, cancellationToken))
            e.Add(nameof(r.ProductCode), $"Product code '{code}' already exists.");
        if (line.Length > 0 && !await db.Set<AdmLkpBusinessLine>().AnyAsync(x => x.Code == line, cancellationToken))
            e.Add(nameof(r.BusinessLineCode), "The selected business line does not exist.");
        if (currency is not null && !await db.Set<AdmLkpCurrency>().AnyAsync(x => x.CurrencyCode == currency, cancellationToken))
            e.Add(nameof(r.DefaultCurrencyCode), "The selected default currency does not exist.");
        e.ThrowIfAny();

        entity.ProductCode = code;
        entity.ProductNameEn = nameEn;
        entity.ProductNameAr = nameAr;
        entity.BusinessLineCode = line;
        entity.ProductTypeCode = type;
        entity.FactoringModeCode = mode;
        entity.IsRecourse = r.IsRecourse;
        entity.DefaultCurrencyCode = currency;
        entity.EffectiveFromDate = r.EffectiveFromDate!.Value;
        entity.EffectiveToDate = r.EffectiveToDate;
        entity.IsActive = r.IsActive;
    }

    private async Task<AdmProduct> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Set<AdmProduct>().FirstOrDefaultAsync(x => x.ProductId == id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser();
        }
    }

    private static ConflictException ChangedByAnotherUser() => new("This product was changed by another user. Reload and try again.");

    private static ProductDto ToDto(AdmProduct x) =>
        new(x.ProductId, x.ProductCode, x.ProductNameEn, x.ProductNameAr, x.BusinessLineCode, x.ProductTypeCode, x.FactoringModeCode, x.IsRecourse,
            x.DefaultCurrencyCode, x.EffectiveFromDate, x.EffectiveToDate, x.IsActive, x.MppPvRate, x.MppLateFeeRate, x.MppReservedAmountPct,
            x.MppInvestorPaymentPct, x.RowVersion);
}
