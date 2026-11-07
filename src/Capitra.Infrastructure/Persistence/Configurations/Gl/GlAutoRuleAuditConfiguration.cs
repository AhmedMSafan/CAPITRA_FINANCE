using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoRuleAuditConfiguration : IEntityTypeConfiguration<GlAutoRuleAudit>
{
    public void Configure(EntityTypeBuilder<GlAutoRuleAudit> builder)
    {
        builder.ToTable("GL_AUTO_RULE_AUDITS");
        builder.HasKey(x => x.RuleAuditId);
        builder.HasIndex(x => new { x.ObjectType, x.ObjectId, x.ChangedAt });

        builder.Property(x => x.RuleAuditId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ObjectType).HasMaxLength(40);
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ColumnName).HasMaxLength(128);
        builder.Property(x => x.ChangedBy).HasMaxLength(255);
        builder.Property(x => x.ChangeReason).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
