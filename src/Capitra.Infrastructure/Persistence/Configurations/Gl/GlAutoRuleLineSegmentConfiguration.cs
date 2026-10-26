using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoRuleLineSegmentConfiguration : IEntityTypeConfiguration<GlAutoRuleLineSegment>
{
    public void Configure(EntityTypeBuilder<GlAutoRuleLineSegment> builder)
    {
        builder.ToTable("GL_AUTO_RULE_LINE_SEGMENTS");
        builder.HasKey(x => x.RuleLineSegmentId);
        builder.HasIndex(x => new { x.RuleLineId, x.CoaSegmentId }).IsUnique();

        builder.Property(x => x.RuleLineSegmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DerivationMethod).HasMaxLength(40);
        builder.Property(x => x.SourceValueCode).HasMaxLength(60);

        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.DefaultSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.FixedSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingRuleLine>().WithMany().HasForeignKey(x => x.RuleLineId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlAccountingAccountLookup>().WithMany().HasForeignKey(x => x.LookupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegment>().WithMany().HasForeignKey(x => x.CoaSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.SourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
