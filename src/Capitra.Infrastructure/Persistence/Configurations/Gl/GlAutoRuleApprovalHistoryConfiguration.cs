using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoRuleApprovalHistoryConfiguration : IEntityTypeConfiguration<GlAutoRuleApprovalHistory>
{
    public void Configure(EntityTypeBuilder<GlAutoRuleApprovalHistory> builder)
    {
        builder.ToTable("GL_AUTO_RULE_APPROVAL_HIST");
        builder.HasKey(x => x.ApprovalHistoryId);

        builder.Property(x => x.ApprovalHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ActionBy).HasMaxLength(255);
        builder.Property(x => x.FromStatus).HasMaxLength(30);
        builder.Property(x => x.ToStatus).HasMaxLength(30);
        builder.Property(x => x.Comments).HasMaxLength(2000);

        builder.HasOne<GlAutoAccountingRuleSet>().WithMany().HasForeignKey(x => x.RuleSetId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
