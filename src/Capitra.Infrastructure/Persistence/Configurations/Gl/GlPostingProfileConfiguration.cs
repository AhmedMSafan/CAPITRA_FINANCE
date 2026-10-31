using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingProfileConfiguration : IEntityTypeConfiguration<GlPostingProfile>
{
    public void Configure(EntityTypeBuilder<GlPostingProfile> builder)
    {
        builder.ToTable("GL_POSTING_PROFILES");
        builder.HasKey(x => x.PostingProfileId);
        builder.HasIndex(x => x.ProfileCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.SourceCode, x.CategoryCode, x.IsActive });
        builder.HasIndex(x => new { x.PostingMode, x.IsActive, x.Priority });

        builder.Property(x => x.PostingProfileId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProfileCode).HasMaxLength(50);
        builder.Property(x => x.ProfileNameEn).HasMaxLength(200);
        builder.Property(x => x.ProfileNameAr).HasMaxLength(200);
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.CategoryCode).HasMaxLength(50);
        builder.Property(x => x.PostingMode).HasMaxLength(30);
        builder.Property(x => x.PostingLevel).HasMaxLength(30);
        builder.Property(x => x.BalanceUpdateMode).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
