using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseProfileConfiguration : IEntityTypeConfiguration<GlCloseProfile>
{
    public void Configure(EntityTypeBuilder<GlCloseProfile> builder)
    {
        builder.ToTable("GL_CLOSE_PROFILES");
        builder.HasKey(x => x.CloseProfileId);
        builder.HasIndex(x => x.CloseProfileCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.CloseType, x.IsActive });

        builder.Property(x => x.CloseProfileId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CloseProfileCode).HasMaxLength(50);
        builder.Property(x => x.CloseProfileNameEn).HasMaxLength(200);
        builder.Property(x => x.CloseProfileNameAr).HasMaxLength(200);
        builder.Property(x => x.CloseType).HasMaxLength(20);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(50);
        builder.Property(x => x.CloseFrequency).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
