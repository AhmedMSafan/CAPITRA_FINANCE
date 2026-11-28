using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodControlRuleConfiguration : IEntityTypeConfiguration<GlPeriodControlRule>
{
    public void Configure(EntityTypeBuilder<GlPeriodControlRule> builder)
    {
        builder.ToTable("GL_PERIOD_CONTROL_RULES");
        builder.HasKey(x => x.PeriodRuleId);
        builder.HasIndex(x => x.RuleCode).IsUnique();
        builder.HasIndex(x => new { x.RuleType, x.IsActive, x.DisplaySeq });
        builder.HasIndex(x => new { x.LedgerId, x.SourceCode, x.BusinessLineCode });

        builder.Property(x => x.PeriodRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(60);
        builder.Property(x => x.RuleNameEn).HasMaxLength(200);
        builder.Property(x => x.RuleNameAr).HasMaxLength(200);
        builder.Property(x => x.RuleType).HasMaxLength(30);
        builder.Property(x => x.SeverityCode).HasMaxLength(30);
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.MessageEn).HasMaxLength(1000);
        builder.Property(x => x.MessageAr).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
