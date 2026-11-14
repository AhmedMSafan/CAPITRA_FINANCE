using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlSetupAuditConfiguration : IEntityTypeConfiguration<GlSetupAudit>
{
    public void Configure(EntityTypeBuilder<GlSetupAudit> builder)
    {
        builder.ToTable("GL_SETUP_AUDITS");
        builder.HasKey(x => x.AuditId);
        builder.HasIndex(x => new { x.EntityName, x.EntityKey });

        builder.Property(x => x.AuditId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EntityName).HasMaxLength(100);
        builder.Property(x => x.EntityKey).HasMaxLength(200);
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ChangeSummary).HasMaxLength(4000);
        builder.Property(x => x.SourceSessionId).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
