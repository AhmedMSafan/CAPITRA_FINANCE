using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Lookups;

public sealed partial class AdmLookupService(ICapitraDbContext db) : IAdmLookupService
{
    private const int DescriptionMaxLength = 500;

    public IReadOnlyList<LookupDefinitionDto> Definitions { get; } =
        [.. LookupRegistry.All.Values.Select(ToDefinitionDto)];

    public async Task<IReadOnlyList<LookupDto>> ListAsync(
        string lookupName, bool includeInactive, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var query = definition.Query(db).AsNoTracking();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var rows = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(x => ToDto(definition, x))];
    }

    public async Task<LookupDto> GetAsync(string lookupName, string code, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var entity = await FindAsync(definition, NormalizeCode(code), tracking: false, cancellationToken);
        return ToDto(definition, entity);
    }

    public async Task<LookupDto> CreateAsync(
        string lookupName, SaveLookupRequest request, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var code = NormalizeCode(request.Code);
        var errors = Validate(definition, code, request);
        var extras = await ReadExtrasAsync(definition, request, existing: null, errors, cancellationToken);
        ThrowIfAny(errors);

        var exists = await definition.Query(db).AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
            throw new ConflictException($"{definition.Title}: '{code}' already exists.");

        var entity = definition.Create();
        entity.Code = code;
        Apply(definition, entity, request, extras);

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(definition, entity);
    }

    public async Task<LookupDto> UpdateAsync(
        string lookupName, string code, SaveLookupRequest request, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var normalized = NormalizeCode(code);
        var errors = Validate(definition, normalized, request);

        if (!string.Equals(NormalizeCode(request.Code), normalized, StringComparison.Ordinal))
            errors[nameof(request.Code)] = ["The code of an existing value cannot be changed."];

        if (request.RowVersion is null)
            errors[nameof(request.RowVersion)] = ["RowVersion is required when updating."];

        var entity = await FindAsync(definition, normalized, tracking: true, cancellationToken);
        var extras = await ReadExtrasAsync(definition, request, entity, errors, cancellationToken);
        ThrowIfAny(errors);

        if (entity.RowVersion != request.RowVersion)
            throw ChangedByAnotherUser(definition, normalized);

        Apply(definition, entity, request, extras);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser(definition, normalized);
        }

        return ToDto(definition, entity);
    }

    public async Task DeleteAsync(string lookupName, string code, int rowVersion, CancellationToken cancellationToken)
    {
        var definition = Resolve(lookupName);
        var normalized = NormalizeCode(code);
        var entity = await FindAsync(definition, normalized, tracking: true, cancellationToken);

        if (entity.RowVersion != rowVersion)
            throw ChangedByAnotherUser(definition, normalized);

        db.Remove(entity);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser(definition, normalized);
        }
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
            ?? throw new NotFoundException($"{definition.Title}: '{code}' was not found.");
    }

    private static ConflictException ChangedByAnotherUser(LookupDefinition definition, string code) =>
        new($"{definition.Title}: '{code}' was changed by another user. Reload and try again.");

    private static string NormalizeCode(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    private static Dictionary<string, string[]> Validate(LookupDefinition definition, string code, SaveLookupRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var nameMax = definition.NameMaxLength;

        if (code.Length > definition.CodeMaxLength || !CodePattern().IsMatch(code))
            errors[nameof(request.Code)] = [$"Code must be 1 to {definition.CodeMaxLength} characters: A-Z, 0-9 and underscore."];

        if (string.IsNullOrWhiteSpace(request.NameAr) || request.NameAr.Trim().Length > nameMax)
            errors[nameof(request.NameAr)] = [$"Arabic name is required (max {nameMax} characters)."];

        if (string.IsNullOrWhiteSpace(request.NameEn) || request.NameEn.Trim().Length > nameMax)
            errors[nameof(request.NameEn)] = [$"English name is required (max {nameMax} characters)."];

        if (request.Description?.Trim().Length > DescriptionMaxLength)
            errors[nameof(request.Description)] = [$"Description is limited to {DescriptionMaxLength} characters."];

        if (request.DisplayOrder < 0 || (definition.DisplayOrderRequired && request.DisplayOrder < 1))
            errors[nameof(request.DisplayOrder)] = [definition.DisplayOrderRequired
                ? $"{definition.DisplayOrderLabel} is required and must be 1 or more."
                : $"{definition.DisplayOrderLabel} cannot be negative."];

        return errors;
    }

    /// <summary>Reads and validates the extra fields of this lookup from the request.</summary>
    private async Task<Dictionary<string, object?>> ReadExtrasAsync(
        LookupDefinition definition,
        SaveLookupRequest request,
        LookupEntity? existing,
        Dictionary<string, string[]> errors,
        CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, object?>();

        foreach (var field in definition.Fields)
        {
            var json = request.Extras is not null && request.Extras.TryGetValue(field.Name, out var element)
                ? element
                : default;
            var isEmpty = json.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ||
                          (json.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(json.GetString()));

            if (isEmpty)
            {
                if (field.Required)
                    errors[field.Name] = [$"{field.Label} is required."];
                values[field.Name] = field.Type == LookupFieldType.Boolean ? false : null;
                continue;
            }

            switch (field.Type)
            {
                case LookupFieldType.Text:
                    var text = json.ValueKind == JsonValueKind.String ? json.GetString()!.Trim() : null;
                    if (text is null || (field.MaxLength is { } max && text.Length > max))
                        errors[field.Name] = [$"{field.Label} must be text of at most {field.MaxLength} characters."];
                    values[field.Name] = text;
                    break;

                case LookupFieldType.Number:
                    if (json.ValueKind != JsonValueKind.Number || !json.TryGetInt32(out var number) ||
                        number < (field.Min ?? int.MinValue) || number > (field.Max ?? int.MaxValue))
                    {
                        errors[field.Name] = [$"{field.Label} must be a whole number between {field.Min} and {field.Max}."];
                        break;
                    }
                    values[field.Name] = number;
                    break;

                case LookupFieldType.Boolean:
                    if (json.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    {
                        errors[field.Name] = [$"{field.Label} must be true or false."];
                        break;
                    }
                    values[field.Name] = json.GetBoolean();
                    break;

                case LookupFieldType.Lookup:
                    var code = json.ValueKind == JsonValueKind.String ? NormalizeCode(json.GetString()) : string.Empty;
                    var unchanged = existing is not null && Equals(Property(definition, field).GetValue(existing), code);
                    var target = LookupRegistry.All[field.LookupName!];
                    if (!unchanged && !await target.Query(db).AnyAsync(x => x.Code == code && x.IsActive, cancellationToken))
                        errors[field.Name] = [$"{field.Label} '{code}' is not an active {target.Title} value."];
                    values[field.Name] = code;
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported lookup field type {field.Type}.");
            }
        }

        return values;
    }

    private static void ThrowIfAny(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);
    }

    private static void Apply(
        LookupDefinition definition, LookupEntity entity, SaveLookupRequest request, Dictionary<string, object?> extras)
    {
        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.Description = definition.ShowDescription && !string.IsNullOrWhiteSpace(request.Description)
            ? request.Description.Trim()
            : null;
        entity.DisplayOrder = definition.ShowDisplayOrder ? request.DisplayOrder : 0;
        entity.IsActive = request.IsActive;

        foreach (var field in definition.Fields)
            Property(definition, field).SetValue(entity, extras[field.Name]);
    }

    private static PropertyInfo Property(LookupDefinition definition, LookupField field) =>
        definition.EntityType.GetProperty(field.PropertyName)
        ?? throw new InvalidOperationException($"{definition.EntityType.Name} has no property {field.PropertyName}.");

    private static LookupDto ToDto(LookupDefinition definition, LookupEntity x) =>
        new(x.Code, x.NameAr, x.NameEn, x.Description, x.DisplayOrder, x.IsActive, x.RowVersion,
            definition.Fields.ToDictionary(f => f.Name, f => Property(definition, f).GetValue(x)));

    private static LookupDefinitionDto ToDefinitionDto(LookupDefinition d) =>
        new(d.LookupName, d.Module, d.Title, d.ApexPages, d.CodeMaxLength, d.NameMaxLength, d.ShowDisplayOrder,
            d.DisplayOrderLabel, d.DisplayOrderRequired, d.ShowDescription,
            [.. d.Fields.Select(f => new LookupFieldDto(
                f.Name, f.Label, f.Type.ToString().ToUpperInvariant(), f.Required, f.MaxLength, f.LookupName, f.Min, f.Max))]);

    [GeneratedRegex("^[A-Z0-9_]+$")]
    private static partial Regex CodePattern();
}
