using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlExportAuditConfiguration : IEntityTypeConfiguration<GlOracleGlExportAudit>
{
    public void Configure(EntityTypeBuilder<GlOracleGlExportAudit> builder)
    {
        builder.ToTable("GL_ORACLE_GL_EXPORT_AUDITS");
        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AuditObject).HasMaxLength(50);
        builder.Property(x => x.ActionCode).HasMaxLength(50);
        builder.Property(x => x.OldStatus).HasMaxLength(50);
        builder.Property(x => x.NewStatus).HasMaxLength(50);
        builder.Property(x => x.ActionMessage).HasMaxLength(4000);
        builder.Property(x => x.ActionBy).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
