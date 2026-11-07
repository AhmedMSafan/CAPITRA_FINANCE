using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingProfileRuleConfiguration : IEntityTypeConfiguration<GlPostingProfileRule>
{
    public void Configure(EntityTypeBuilder<GlPostingProfileRule> builder)
    {
        builder.ToTable("GL_POSTING_PROFILE_RULES");
        builder.HasKey(x => x.ProfileRuleId);
        builder.HasIndex(x => new { x.PostingProfileId, x.RuleSeq }).IsUnique();
        builder.HasIndex(x => new { x.PostingProfileId, x.RuleScope, x.IsActive });

        builder.Property(x => x.ProfileRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleScope).HasMaxLength(30);
        builder.Property(x => x.ConditionAttribute).HasMaxLength(100);
        builder.Property(x => x.ConditionOperator).HasMaxLength(30);
        builder.Property(x => x.ConditionValue1).HasMaxLength(4000);
        builder.Property(x => x.ConditionValue2).HasMaxLength(4000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);

        builder.HasOne<GlPostingProfile>().WithMany().HasForeignKey(x => x.PostingProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
