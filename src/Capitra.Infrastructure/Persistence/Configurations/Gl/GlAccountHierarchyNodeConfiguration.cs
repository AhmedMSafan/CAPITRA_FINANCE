using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountHierarchyNodeConfiguration : IEntityTypeConfiguration<GlAccountHierarchyNode>
{
    public void Configure(EntityTypeBuilder<GlAccountHierarchyNode> builder)
    {
        builder.ToTable("GL_ACCOUNT_HIERARCHY_NODES");
        builder.HasKey(x => x.NodeId);
        builder.HasIndex(x => new { x.HierarchyId, x.NodeCode }).IsUnique();
        builder.HasIndex(x => new { x.HierarchyId, x.ParentNodeId, x.DisplaySeq });
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.SegmentValueId);

        builder.Property(x => x.NodeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.NodeCode).HasMaxLength(60);
        builder.Property(x => x.NodeNameEn).HasMaxLength(240);
        builder.Property(x => x.NodeNameAr).HasMaxLength(240);
        builder.Property(x => x.NodeType).HasMaxLength(20);
        builder.Property(x => x.RollupOperator).HasMaxLength(10);

        builder.HasOne<GlAccountHierarchy>().WithMany().HasForeignKey(x => x.HierarchyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountHierarchyNode>().WithMany().HasForeignKey(x => x.ParentNodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.SegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
