using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlFxRevaluationRuleConfiguration : IEntityTypeConfiguration<GlFxRevaluationRule>
{
    public void Configure(EntityTypeBuilder<GlFxRevaluationRule> builder)
    {
        builder.ToTable("GL_FX_REVALUATION_RULES");
        builder.HasKey(x => x.RevaluationRuleId);
        builder.HasIndex(x => new { x.LedgerId, x.RuleCode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.IsActive });

        builder.Property(x => x.RevaluationRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.RuleNameEn).HasMaxLength(240);
        builder.Property(x => x.RuleNameAr).HasMaxLength(240);
        builder.Property(x => x.RevaluationMethod).HasMaxLength(30);
        builder.Property(x => x.RevaluationScope).HasMaxLength(30);
        builder.Property(x => x.SourceBalanceType).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.GainAccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.LossAccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
