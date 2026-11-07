using Capitra.Application.Abstractions;
using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Capitra.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Fills the audit columns on every save from the logged-in user and the server's UTC clock,
/// and bumps ROW_VERSION so concurrent edits are detected. Nothing here comes from the client.
/// </summary>
public sealed class AuditInterceptor(ICurrentUser currentUser, IClock clock) : SaveChangesInterceptor
{
    public const string SystemUser = "SYSTEM";

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
            return;

        var user = currentUser.UserName ?? SystemUser;
        var now = clock.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = user;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedBy = null;
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.RowVersion = 1;
                    break;

                case EntityState.Modified:
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedBy = user;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.RowVersion = entry.Property(x => x.RowVersion).OriginalValue + 1;
                    break;
            }
        }
    }
}
