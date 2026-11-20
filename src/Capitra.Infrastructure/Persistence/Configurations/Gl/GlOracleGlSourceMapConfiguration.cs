using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlSourceMapConfiguration : IEntityTypeConfiguration<GlOracleGlSourceMap>
{
    public void Configure(EntityTypeBuilder<GlOracleGlSourceMap> builder)
    {
        builder.ToTable("GL_ORACLE_GL_SOURCE_MAPS");
        builder.HasKey(x => x.SourceMapId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.FinovaSourceCode }).IsUnique();

        builder.Property(x => x.SourceMapId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FinovaSourceCode).HasMaxLength(50);
        builder.Property(x => x.OracleJeSourceName).HasMaxLength(100);
        builder.Property(x => x.OracleSourceKey).HasMaxLength(100);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
