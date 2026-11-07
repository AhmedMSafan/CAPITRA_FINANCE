using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseProfileModuleConfiguration : IEntityTypeConfiguration<GlCloseProfileModule>
{
    public void Configure(EntityTypeBuilder<GlCloseProfileModule> builder)
    {
        builder.ToTable("GL_CLOSE_PROFILE_MODULES");
        builder.HasKey(x => x.ProfileModuleId);
        builder.HasIndex(x => new { x.CloseProfileId, x.SourceModule }).IsUnique();
        builder.HasIndex(x => new { x.SourceModule, x.IsActive });

        builder.Property(x => x.ProfileModuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceModuleNameEn).HasMaxLength(200);
        builder.Property(x => x.SourceModuleNameAr).HasMaxLength(200);
        builder.Property(x => x.ExpectedStatusCode).HasMaxLength(50);

        builder.HasOne<GlCloseProfile>().WithMany().HasForeignKey(x => x.CloseProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
