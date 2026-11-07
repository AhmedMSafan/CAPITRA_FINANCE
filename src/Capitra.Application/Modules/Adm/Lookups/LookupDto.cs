namespace Capitra.Application.Modules.Adm.Lookups;

public sealed record LookupDto(
    string Code,
    string NameAr,
    string NameEn,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    int RowVersion,
    IReadOnlyDictionary<string, object?> Extras);

/// <summary>What the screen needs to draw a lookup's list and form.</summary>
public sealed record LookupDefinitionDto(
    string Name,
    string Module,
    string Title,
    string ApexPages,
    int CodeMaxLength,
    int NameMaxLength,
    bool ShowDisplayOrder,
    string DisplayOrderLabel,
    bool DisplayOrderRequired,
    bool ShowDescription,
    IReadOnlyList<LookupFieldDto> Fields);

public sealed record LookupFieldDto(
    string Name,
    string Label,
    string Type,
    bool Required,
    int? MaxLength,
    string? LookupName,
    int? Min,
    int? Max);
