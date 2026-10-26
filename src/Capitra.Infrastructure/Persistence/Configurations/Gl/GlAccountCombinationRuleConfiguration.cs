using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountCombinationRuleConfiguration : IEntityTypeConfiguration<GlAccountCombinationRule>
{
    public void Configure(EntityTypeBuilder<GlAccountCombinationRule> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATION_RULES");
        builder.HasKey(x => x.RuleId);
        builder.HasIndex(x => new { x.CoaId, x.RuleCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.IsActive, x.PriorityNo });
        builder.HasIndex(x => new { x.RuleType, x.RuleScope, x.ValidationLevel });

        builder.Property(x => x.RuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.RuleNameEn).HasMaxLength(240);
        builder.Property(x => x.RuleNameAr).HasMaxLength(240);
        builder.Property(x => x.RuleType).HasMaxLength(20);
        builder.Property(x => x.RuleScope).HasMaxLength(40);
        builder.Property(x => x.ValidationLevel).HasMaxLength(20);
        builder.Property(x => x.ErrorMessageEn).HasMaxLength(1000);
        builder.Property(x => x.ErrorMessageAr).HasMaxLength(1000);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
