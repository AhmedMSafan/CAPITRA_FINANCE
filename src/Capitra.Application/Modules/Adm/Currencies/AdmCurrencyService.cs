using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Currencies;

public sealed record CurrencyDto(string CurrencyCode, string CurrencyNameEn, string CurrencyNameAr, int DecimalPlaces, bool IsActive, int RowVersion);

public sealed record SaveCurrencyRequest(string? CurrencyCode, string? CurrencyNameEn, string? CurrencyNameAr, int? DecimalPlaces, bool IsActive, int? RowVersion);

/// <summary>ADM_LKP_CURRENCIES, the list APEX App 105 pages 37/38 maintain, for the Admin Setup currency list.</summary>
public sealed class AdmCurrencyService(ICapitraDbContext db)
{
    public async Task<IReadOnlyList<CurrencyDto>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await db.Set<AdmLkpCurrency>().AsNoTracking().OrderBy(x => x.CurrencyCode).ToListAsync(cancellationToken)).Select(ToDto)];

    public async Task<CurrencyDto> CreateAsync(SaveCurrencyRequest request, CancellationToken cancellationToken)
    {
        var code = Normalize(request.CurrencyCode);
        Validate(code, request);
        if (await db.Set<AdmLkpCurrency>().AnyAsync(x => x.CurrencyCode == code, cancellationToken))
            throw new ConflictException($"Currency '{code}' already exists.");

        var entity = new AdmLkpCurrency { CurrencyCode = code };
        Apply(entity, request);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<CurrencyDto> UpdateAsync(string code, SaveCurrencyRequest request, CancellationToken cancellationToken)
    {
        code = Normalize(code);
        Validate(code, request);
        var entity = await FindAsync(code, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser(code);

        Apply(entity, request);
        await SaveAsync(code, cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(string code, int rowVersion, CancellationToken cancellationToken)
    {
        code = Normalize(code);
        var entity = await FindAsync(code, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser(code);

        db.Remove(entity);
        await SaveAsync(code, cancellationToken);
    }

    private async Task<AdmLkpCurrency> FindAsync(string code, CancellationToken cancellationToken) =>
        await db.Set<AdmLkpCurrency>().FirstOrDefaultAsync(x => x.CurrencyCode == code, cancellationToken)
            ?? throw new NotFoundException($"Currency '{code}' was not found.");

    private async Task SaveAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser(code);
        }
    }

    private static void Validate(string code, SaveCurrencyRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (code.Length != 3)
            errors[nameof(request.CurrencyCode)] = ["Code must be the three-letter ISO code."];
        if (string.IsNullOrWhiteSpace(request.CurrencyNameEn) || request.CurrencyNameEn.Trim().Length > 100)
            errors[nameof(request.CurrencyNameEn)] = ["Name (EN) is required, up to 100 characters."];
        if (string.IsNullOrWhiteSpace(request.CurrencyNameAr) || request.CurrencyNameAr.Trim().Length > 100)
            errors[nameof(request.CurrencyNameAr)] = ["Name (AR) is required, up to 100 characters."];
        if (request.DecimalPlaces is null or < 0 or > 6)
            errors[nameof(request.DecimalPlaces)] = ["Decimal places must be between 0 and 6."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
    }

    private static void Apply(AdmLkpCurrency entity, SaveCurrencyRequest request)
    {
        entity.CurrencyNameEn = request.CurrencyNameEn!.Trim();
        entity.CurrencyNameAr = request.CurrencyNameAr!.Trim();
        entity.DecimalPlaces = request.DecimalPlaces!.Value;
        entity.IsActive = request.IsActive;
    }

    private static string Normalize(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    private static ConflictException ChangedByAnotherUser(string code) =>
        new($"Currency '{code}' was changed by another user. Reload and try again.");

    private static CurrencyDto ToDto(AdmLkpCurrency x) =>
        new(x.CurrencyCode, x.CurrencyNameEn, x.CurrencyNameAr, x.DecimalPlaces, x.IsActive, x.RowVersion);
}
