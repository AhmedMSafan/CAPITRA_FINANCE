using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlExternalGlSystemConfiguration : IEntityTypeConfiguration<GlExternalGlSystem>
{
    public void Configure(EntityTypeBuilder<GlExternalGlSystem> builder)
    {
        builder.ToTable("GL_EXTERNAL_GL_SYSTEMS");
        builder.HasKey(x => x.ExternalSystemId);
        builder.HasIndex(x => x.SystemCode).IsUnique();

        builder.Property(x => x.ExternalSystemId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SystemCode).HasMaxLength(30);
        builder.Property(x => x.SystemNameEn).HasMaxLength(200);
        builder.Property(x => x.SystemNameAr).HasMaxLength(200);
        builder.Property(x => x.OracleProductType).HasMaxLength(30);
        builder.Property(x => x.ExportMethod).HasMaxLength(30);
        builder.Property(x => x.EndpointCode).HasMaxLength(100);
        builder.Property(x => x.CredentialAlias).HasMaxLength(200);
        builder.Property(x => x.DefaultInterfaceMode).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
