using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountCombinationSegmentConfiguration : IEntityTypeConfiguration<GlAccountCombinationSegment>
{
    public void Configure(EntityTypeBuilder<GlAccountCombinationSegment> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATION_SEGMENTS");
        builder.HasKey(x => x.CombinationSegmentId);
        builder.HasIndex(x => new { x.AccountCombinationId, x.SegmentNum }).IsUnique();
        builder.HasIndex(x => new { x.AccountCombinationId, x.CoaSegmentId }).IsUnique();
        builder.HasIndex(x => new { x.CoaSegmentId, x.SegmentValueId });
        builder.HasIndex(x => x.SegmentValueCode);

        builder.Property(x => x.CombinationSegmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SegmentValueCode).HasMaxLength(30);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegment>().WithMany().HasForeignKey(x => x.CoaSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.SegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
