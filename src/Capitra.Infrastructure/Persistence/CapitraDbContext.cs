using Capitra.Application.Abstractions;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence.Conventions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Persistence;

public sealed class CapitraDbContext(DbContextOptions<CapitraDbContext> options)
    : IdentityDbContext<SecUser, SecRole, string, SecUserClaim, SecUserRole, SecUserLogin, SecRoleClaim, SecUserToken, SecUserPasskey>(options),
      ICapitraDbContext
{
    public const string MigrationsHistoryTable = "SYS_EF_MIGRATION_HISTORY";

    /// <summary>Pinned so the design-time model (migrations) and the runtime model are always the same.</summary>
    protected override Version SchemaVersion => IdentitySchemaVersions.Version3;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CapitraDbContext).Assembly);
        NamingStandard.Apply(builder);
    }
}
