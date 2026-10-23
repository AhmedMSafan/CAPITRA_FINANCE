using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Lookups;

public sealed partial class AdmLookupService(ICapitraDbContext db) : IAdmLookupService
{
    private const int NameMaxLength = 200;
    private const int DescriptionMaxLength = 500;

    public IReadOnlyCollection<string> LookupNames => [.. LookupRegistry.All.Keys.Order()];

    public async Task<IReadOnlyList<LookupDto>> ListAsync(
        string lookupName, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = Resolve(lookupName).Query(db).AsNoTracking();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var rows = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(ToDto)];
    }

    public async Task<LookupDto> GetAsync(string lookupName, string code, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(Resolve(lookupName), code, tracking: false, cancellationToken);
        return ToDto(entity);
    }

    public async Task<LookupDto> CreateAsync(
        string lookupName, SaveLookupRequest request, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var code = NormalizeCode(request.Code);
        Validate(code, request);

        var exists = await definition.Query(db).AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
            throw new ConflictException($"{lookupName} '{code}' already exists.");

        var entity = definition.Create();
        entity.Code = code;
        Apply(entity, request);

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<LookupDto> UpdateAsync(
        string lookupName, string code, SaveLookupRequest request, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var normalized = NormalizeCode(code);
        Validate(normalized, request);

        if (!string.Equals(NormalizeCode(request.Code), normalized, StringComparison.Ordinal))
            throw BusinessValidationException.For(nameof(request.Code), "The code of an existing value cannot be changed.");

        if (request.RowVersion is null)
            throw BusinessValidationException.For(nameof(request.RowVersion), "RowVersion is required when updating.");

        var entity = await FindAsync(definition, normalized, tracking: true, cancellationToken);

        if (entity.RowVersion != request.RowVersion)
            throw new ConflictException($"{lookupName} '{normalized}' was changed by another user. Reload and try again.");

        Apply(entity, request);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"{lookupName} '{normalized}' was changed by another user. Reload and try again.");
        }

        return ToDto(entity);
    }

    private static LookupDefinition Resolve(string lookupName) =>
        LookupRegistry.All.TryGetValue(lookupName, out var definition)
            ? definition
            : throw new NotFoundException($"Lookup '{lookupName}' does not exist.");

    private async Task<LookupEntity> FindAsync(
        LookupDefinition definition, string code, bool tracking, CancellationToken cancellationToken)
    {
        var query = definition.Query(db);
        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            ?? throw new NotFoundException($"{definition.LookupName} '{code}' was not found.");
    }

    private static string NormalizeCode(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    private static void Validate(string code, SaveLookupRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!CodePattern().IsMatch(code))
            errors[nameof(request.Code)] = ["Code must be 1 to 30 characters: A-Z, 0-9 and underscore."];

        if (string.IsNullOrWhiteSpace(request.NameAr) || request.NameAr.Trim().Length > NameMaxLength)
            errors[nameof(request.NameAr)] = [$"Arabic name is required (max {NameMaxLength} characters)."];

        if (string.IsNullOrWhiteSpace(request.NameEn) || request.NameEn.Trim().Length > NameMaxLength)
            errors[nameof(request.NameEn)] = [$"English name is required (max {NameMaxLength} characters)."];

        if (request.Description?.Trim().Length > DescriptionMaxLength)
            errors[nameof(request.Description)] = [$"Description is limited to {DescriptionMaxLength} characters."];

        if (request.DisplayOrder < 0)
            errors[nameof(request.DisplayOrder)] = ["Display order cannot be negative."];

        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
    }

    private static void Apply(LookupEntity entity, SaveLookupRequest request)
    {
        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
    }

    private static LookupDto ToDto(LookupEntity x) =>
        new(x.Code, x.NameAr, x.NameEn, x.Description, x.DisplayOrder, x.IsActive, x.RowVersion);

    [GeneratedRegex("^[A-Z0-9_]{1,30}$")]
    private static partial Regex CodePattern();
}
