using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseValidationRuleConfiguration : IEntityTypeConfiguration<GlCloseValidationRule>
{
    public void Configure(EntityTypeBuilder<GlCloseValidationRule> builder)
    {
        builder.ToTable("GL_CLOSE_VALIDATION_RULES");
        builder.HasKey(x => x.ValidationRuleId);
        builder.HasIndex(x => new { x.ValidationSetId, x.RuleCode }).IsUnique();
        builder.HasIndex(x => new { x.RuleCategory, x.SourceModule, x.IsActive });

        builder.Property(x => x.ValidationRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.RuleNameEn).HasMaxLength(200);
        builder.Property(x => x.RuleNameAr).HasMaxLength(200);
        builder.Property(x => x.RuleCategory).HasMaxLength(50);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.RuleType).HasMaxLength(30);
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.ExpectedResultType).HasMaxLength(30);
        builder.Property(x => x.ThresholdAmount).HasPrecision(18, 2);
        builder.Property(x => x.ThresholdPercent).HasPrecision(28, 10);
        builder.Property(x => x.ToleranceAmount).HasPrecision(18, 2);
        builder.Property(x => x.TolerancePercent).HasPrecision(28, 10);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlCloseValidationSet>().WithMany().HasForeignKey(x => x.ValidationSetId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
