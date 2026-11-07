using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlExportProfileConfiguration : IEntityTypeConfiguration<GlOracleGlExportProfile>
{
    public void Configure(EntityTypeBuilder<GlOracleGlExportProfile> builder)
    {
        builder.ToTable("GL_ORACLE_GL_EXPORT_PROFILES");
        builder.HasKey(x => x.ExportProfileId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.ProfileCode }).IsUnique();

        builder.Property(x => x.ExportProfileId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProfileCode).HasMaxLength(50);
        builder.Property(x => x.ProfileNameEn).HasMaxLength(200);
        builder.Property(x => x.ProfileNameAr).HasMaxLength(200);
        builder.Property(x => x.ExportScope).HasMaxLength(30);
        builder.Property(x => x.ExportLevel).HasMaxLength(30);
        builder.Property(x => x.DefaultSourceCode).HasMaxLength(50);
        builder.Property(x => x.DefaultCategoryCode).HasMaxLength(50);
        builder.Property(x => x.DuplicateCheckLevel).HasMaxLength(30);
        builder.Property(x => x.ReversalHandling).HasMaxLength(30);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlLedgerMap>().WithMany().HasForeignKey(x => x.LedgerMapId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
