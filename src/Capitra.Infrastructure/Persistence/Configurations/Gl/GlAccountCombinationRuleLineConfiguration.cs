using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountCombinationRuleLineConfiguration : IEntityTypeConfiguration<GlAccountCombinationRuleLine>
{
    public void Configure(EntityTypeBuilder<GlAccountCombinationRuleLine> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATION_RULE_LINES");
        builder.HasKey(x => x.RuleLineId);
        builder.HasIndex(x => new { x.RuleId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.SegmentCode, x.SegmentType, x.AttributeName });

        builder.Property(x => x.RuleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SegmentCode).HasMaxLength(30);
        builder.Property(x => x.SegmentType).HasMaxLength(30);
        builder.Property(x => x.AttributeName).HasMaxLength(50);
        builder.Property(x => x.OperatorCode).HasMaxLength(20);
        builder.Property(x => x.ValueFrom).HasMaxLength(240);
        builder.Property(x => x.ValueTo).HasMaxLength(240);
        builder.Property(x => x.ValueList).HasMaxLength(4000);
        builder.Property(x => x.IncludeExcludeFlag).HasMaxLength(10);

        builder.HasOne<GlAccountCombinationRule>().WithMany().HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
