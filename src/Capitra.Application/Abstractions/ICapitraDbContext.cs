using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Capitra.Application.Abstractions;

public interface ICapitraDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    EntityEntry Add(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
