using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingValidationRuleConfiguration : IEntityTypeConfiguration<GlPostingValidationRule>
{
    public void Configure(EntityTypeBuilder<GlPostingValidationRule> builder)
    {
        builder.ToTable("GL_POSTING_VALIDATION_RULES");
        builder.HasKey(x => x.PostingValidationRuleId);
        builder.HasIndex(x => new { x.ValidationSetId, x.RuleCode }).IsUnique();
        builder.HasIndex(x => new { x.ValidationSetId, x.ValidationStage, x.IsActive, x.RuleSeq });

        builder.Property(x => x.PostingValidationRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.RuleName).HasMaxLength(200);
        builder.Property(x => x.ValidationStage).HasMaxLength(30);
        builder.Property(x => x.SeverityCode).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);

        builder.HasOne<GlPostingValidationSet>().WithMany().HasForeignKey(x => x.ValidationSetId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
