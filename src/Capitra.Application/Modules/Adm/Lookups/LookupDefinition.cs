using Capitra.Application.Abstractions;
using Capitra.Domain.Common;

namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>
/// Binds a lookup name used in the API route (api/adm/lookups/{name}) to its entity, and describes its form
/// as the APEX page does: lengths, which standard columns it shows, and any extra fields.
/// </summary>
public sealed record LookupDefinition(
    string LookupName,
    string Module,
    string Title,
    string ApexPages,
    Type EntityType,
    Func<ICapitraDbContext, IQueryable<LookupEntity>> Query,
    Func<LookupEntity> Create)
{
    public int CodeMaxLength { get; init; } = LookupEntity.CodeMaxLength;

    public int NameMaxLength { get; init; } = 200;

    public bool ShowDisplayOrder { get; init; } = true;

    public string DisplayOrderLabel { get; init; } = "Display Sequence";

    /// <summary>When true the order must be 1 or more (approval step sequence).</summary>
    public bool DisplayOrderRequired { get; init; }

    public bool ShowDescription { get; init; }

    public IReadOnlyList<LookupField> Fields { get; init; } = [];

    public static LookupDefinition For<TEntity>(string lookupName, string module, string title, string apexPages)
        where TEntity : LookupEntity, new() =>
        new(lookupName, module, title, apexPages, typeof(TEntity), db => db.Set<TEntity>(), () => new TEntity());
}
