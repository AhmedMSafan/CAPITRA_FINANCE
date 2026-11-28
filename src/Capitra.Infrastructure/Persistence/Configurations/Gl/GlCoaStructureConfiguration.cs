using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCoaStructureConfiguration : IEntityTypeConfiguration<GlCoaStructure>
{
    public void Configure(EntityTypeBuilder<GlCoaStructure> builder)
    {
        builder.ToTable("GL_COA_STRUCTURES");
        builder.HasKey(x => x.CoaId);
        builder.HasIndex(x => x.CoaCode).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.CoaCode });

        builder.Property(x => x.CoaId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CoaCode).HasMaxLength(30);
        builder.Property(x => x.CoaNameEn).HasMaxLength(240);
        builder.Property(x => x.CoaNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DelimiterChar).HasMaxLength(3);

        builder.ConfigureAudit();
    }
}
