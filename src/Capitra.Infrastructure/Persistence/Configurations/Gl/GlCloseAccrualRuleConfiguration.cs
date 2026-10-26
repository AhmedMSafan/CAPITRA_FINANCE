using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccrualRuleConfiguration : IEntityTypeConfiguration<GlCloseAccrualRule>
{
    public void Configure(EntityTypeBuilder<GlCloseAccrualRule> builder)
    {
        builder.ToTable("GL_CLOSE_ACCRUAL_RULES");
        builder.HasKey(x => x.AccrualRuleId);
        builder.HasIndex(x => x.AccrualRuleCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.SourceModule, x.IsActive });

        builder.Property(x => x.AccrualRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccrualRuleCode).HasMaxLength(50);
        builder.Property(x => x.AccrualRuleNameEn).HasMaxLength(200);
        builder.Property(x => x.AccrualRuleNameAr).HasMaxLength(200);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.AccrualCategoryCode).HasMaxLength(50);
        builder.Property(x => x.AccrualMethod).HasMaxLength(30);
        builder.Property(x => x.ReversalPolicy).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlCloseProfile>().WithMany().HasForeignKey(x => x.CloseProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
