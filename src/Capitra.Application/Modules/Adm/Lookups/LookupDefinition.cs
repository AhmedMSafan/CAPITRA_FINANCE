using Capitra.Application.Abstractions;
using Capitra.Domain.Common;

namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>Binds a lookup name used in the API route to its entity and table.</summary>
public sealed record LookupDefinition(
    string LookupName,
    Type EntityType,
    Func<ICapitraDbContext, IQueryable<LookupEntity>> Query,
    Func<LookupEntity> Create)
{
    public static LookupDefinition For<TEntity>(string lookupName)
        where TEntity : LookupEntity, new() =>
        new(lookupName, typeof(TEntity), db => db.Set<TEntity>(), () => new TEntity());
}
