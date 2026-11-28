namespace Capitra.Application.Modules.Adm.Lookups;

public enum LookupFieldType
{
    Text,
    /// <summary>A whole number.</summary>
    Number,
    Boolean,

    /// <summary>A code from another lookup, named by <see cref="LookupField.LookupName"/>.</summary>
    Lookup,
}

/// <summary>
/// A column a lookup table has beyond the standard shape (for example MAX_FILE_SIZE_MB on document types).
/// <see cref="Name"/> is the API/JSON name; the entity property is the same name in PascalCase.
/// </summary>
public sealed record LookupField(
    string Name,
    string Label,
    LookupFieldType Type,
    bool Required = false,
    int? MaxLength = null,
    string? LookupName = null,
    int? Min = null,
    int? Max = null)
{
    public string PropertyName => char.ToUpperInvariant(Name[0]) + Name[1..];
}
