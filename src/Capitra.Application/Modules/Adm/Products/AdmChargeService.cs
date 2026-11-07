using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

public sealed record ChargeDto(
    long ChargeId, string ChargeCode, string ChargeNameEn, string? ChargeNameAr, string ChargeTypeCode, string FinancialNatureCode,
    bool IsRecurring, string CalculationTypeCode, decimal? DefaultRate, decimal? DefaultAmount, string? CurrencyCode, string? Description,
    DateOnly? EffectiveFromDate, DateOnly? EffectiveToDate, bool IsActive, int RowVersion);

public sealed record SaveChargeRequest(
    string? ChargeCode, string? ChargeNameEn, string? ChargeNameAr, string? ChargeTypeCode, string? FinancialNatureCode,
    bool IsRecurring, string? CalculationTypeCode, decimal? DefaultRate, decimal? DefaultAmount, string? CurrencyCode, string? Description,
    DateOnly? EffectiveFromDate, DateOnly? EffectiveToDate, bool IsActive, int? RowVersion);

/// <summary>ADM_CHARGES, the Charges screen of Admin Setup (FINOVA_FINANCEING ChargeComponentsPage and ChargeComponentService).</summary>
public sealed class AdmChargeService(ICapitraDbContext db)
{
    public static readonly string[] FinancialNatures = ["REVENUE", "EXPENSE", "OTHER"];

    public static readonly string[] CalculationTypes = ["FIXED", "PERCENTAGE", "FORMULA"];

    public async Task<IReadOnlyList<ChargeDto>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await db.Set<AdmCharge>().AsNoTracking().OrderBy(x => x.ChargeNameEn).ToListAsync(cancellationToken)).Select(ToDto)];

    public async Task<ChargeDto> CreateAsync(SaveChargeRequest request, CancellationToken cancellationToken)
    {
        var entity = new AdmCharge();
        await ApplyAsync(entity, request, null, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ChargeDto> UpdateAsync(long id, SaveChargeRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser();

        await ApplyAsync(entity, request, entity, cancellationToken);
        await SaveAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser();
        if (await db.Set<AdmProductEventRuleCharge>().AnyAsync(x => x.ChargeId == id, cancellationToken))
            throw new ConflictException("This charge is used by a product event rule. Deactivate it instead.");

        db.Remove(entity);
        await SaveAsync(cancellationToken);
    }

    private async Task ApplyAsync(AdmCharge entity, SaveChargeRequest r, AdmCharge? existing, CancellationToken cancellationToken)
    {
        var e = new SetupErrors();
        var code = e.Required(nameof(r.ChargeCode), "Charge code", r.ChargeCode, 50, upper: true);
        var nameEn = e.Required(nameof(r.ChargeNameEn), "Name (EN)", r.ChargeNameEn, 150);
        var nameAr = e.Optional(nameof(r.ChargeNameAr), "Name (AR)", r.ChargeNameAr, 150);
        var type = e.Required(nameof(r.ChargeTypeCode), "Charge type", r.ChargeTypeCode, 30, upper: true);
        var nature = e.OneOf(nameof(r.FinancialNatureCode), "Financial nature", r.FinancialNatureCode, FinancialNatures);
        var calc = e.OneOf(nameof(r.CalculationTypeCode), "Calculation type", r.CalculationTypeCode, CalculationTypes);
        var currency = e.Optional(nameof(r.CurrencyCode), "Currency", r.CurrencyCode, 3, upper: true);
        var description = e.Optional(nameof(r.Description), "Description", r.Description, 500);
        e.NotNegative(nameof(r.DefaultRate), "Default rate", r.DefaultRate);
        e.NotNegative(nameof(r.DefaultAmount), "Default amount", r.DefaultAmount);
        e.Percent(nameof(r.DefaultRate), "Default rate", r.DefaultRate);
        e.Dates(nameof(r.EffectiveToDate), r.EffectiveFromDate, r.EffectiveToDate);

        // FINOVA_FINANCEING: a fixed charge keeps an amount (with its currency), a percentage keeps a rate, a formula keeps neither.
        decimal? rate = null, amount = null;
        if (calc == "FIXED")
        {
            amount = r.DefaultAmount;
            if (amount.HasValue && currency is null)
                e.Add(nameof(r.CurrencyCode), "Currency is required when a default amount is given.");
        }
        else
        {
            currency = null;
            if (calc == "PERCENTAGE")
                rate = r.DefaultRate;
        }

        if (code.Length > 0 && await db.Set<AdmCharge>().AnyAsync(x => x.ChargeCode == code && x.ChargeId != entity.ChargeId, cancellationToken))
            e.Add(nameof(r.ChargeCode), $"Charge code '{code}' already exists.");
        // a new charge, or a changed type, needs an active type; an existing charge may keep a type made inactive since
        if (type.Length > 0 && (existing is null || existing.ChargeTypeCode != type)
            && !await db.Set<AdmLkpChargeType>().AnyAsync(x => x.Code == type && x.IsActive, cancellationToken))
            e.Add(nameof(r.ChargeTypeCode), "Select a valid active charge type.");
        if (currency is not null && !await db.Set<AdmLkpCurrency>().AnyAsync(x => x.CurrencyCode == currency, cancellationToken))
            e.Add(nameof(r.CurrencyCode), "The selected currency does not exist.");
        e.ThrowIfAny();

        entity.ChargeCode = code;
        entity.ChargeNameEn = nameEn;
        entity.ChargeNameAr = nameAr;
        entity.ChargeTypeCode = type;
        entity.FinancialNatureCode = nature;
        entity.IsRecurring = r.IsRecurring;
        entity.CalculationTypeCode = calc;
        entity.DefaultRate = rate;
        entity.DefaultAmount = amount;
        entity.CurrencyCode = currency;
        entity.Description = description;
        entity.EffectiveFromDate = r.EffectiveFromDate;
        entity.EffectiveToDate = r.EffectiveToDate;
        entity.IsActive = r.IsActive;
    }

    private async Task<AdmCharge> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Set<AdmCharge>().FirstOrDefaultAsync(x => x.ChargeId == id, cancellationToken)
            ?? throw new NotFoundException($"Charge {id} was not found.");

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

    private static ConflictException ChangedByAnotherUser() => new("This charge was changed by another user. Reload and try again.");

    private static ChargeDto ToDto(AdmCharge x) =>
        new(x.ChargeId, x.ChargeCode, x.ChargeNameEn, x.ChargeNameAr, x.ChargeTypeCode, x.FinancialNatureCode, x.IsRecurring, x.CalculationTypeCode,
            x.DefaultRate, x.DefaultAmount, x.CurrencyCode, x.Description, x.EffectiveFromDate, x.EffectiveToDate, x.IsActive, x.RowVersion);
}
