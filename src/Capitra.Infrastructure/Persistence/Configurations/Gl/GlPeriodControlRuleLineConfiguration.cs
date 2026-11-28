using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodControlRuleLineConfiguration : IEntityTypeConfiguration<GlPeriodControlRuleLine>
{
    public void Configure(EntityTypeBuilder<GlPeriodControlRuleLine> builder)
    {
        builder.ToTable("GL_PERIOD_CONTROL_RULE_LINES");
        builder.HasKey(x => x.PeriodRuleLineId);
        builder.HasIndex(x => new { x.PeriodRuleId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.PeriodRuleId, x.IsActive, x.LineNo });

        builder.Property(x => x.PeriodRuleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ConditionType).HasMaxLength(30);
        builder.Property(x => x.ConditionCode).HasMaxLength(60);
        builder.Property(x => x.OperatorCode).HasMaxLength(30);
        builder.Property(x => x.ConditionValue).HasMaxLength(4000);

        builder.HasOne<GlPeriodControlRule>().WithMany().HasForeignKey(x => x.PeriodRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
