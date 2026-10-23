using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence.Conventions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Capitra.Infrastructure.Persistence;

public sealed class CapitraDbContext(DbContextOptions<CapitraDbContext> options)
    : IdentityDbContext<SecUser, SecRole, string, SecUserClaim, SecUserRole, SecUserLogin, SecRoleClaim, SecUserToken, SecUserPasskey>(options),
      ICapitraDbContext
{
    public const string MigrationsHistoryTable = "SYS_EF_MIGRATION_HISTORY";

    /// <summary>Pinned so the design-time model (migrations) and the runtime model are always the same.</summary>
    protected override Version SchemaVersion => IdentitySchemaVersions.Version3;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new ConflictException(
                "This record is used by other records, so it cannot be deleted. Make it inactive instead.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("A record with the same code already exists.", ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CapitraDbContext).Assembly);
        NamingStandard.Apply(builder);
    }
}
