using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Capitra.Application.Abstractions;

public interface ICapitraDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    EntityEntry Add(object entity);

    EntityEntry Remove(object entity);

    /// <summary>
    /// Saves pending changes. A save that breaks a unique key or a foreign key throws
    /// <see cref="Common.Exceptions.ConflictException"/> instead of a provider error.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
