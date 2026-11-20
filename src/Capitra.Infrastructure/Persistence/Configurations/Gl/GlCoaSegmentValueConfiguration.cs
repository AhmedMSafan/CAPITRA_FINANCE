using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCoaSegmentValueConfiguration : IEntityTypeConfiguration<GlCoaSegmentValue>
{
    public void Configure(EntityTypeBuilder<GlCoaSegmentValue> builder)
    {
        builder.ToTable("GL_COA_SEGMENT_VALUES");
        builder.HasKey(x => x.SegmentValueId);
        builder.HasIndex(x => new { x.CoaSegmentId, x.ValueCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaSegmentId, x.IsEnabled, x.ValueCode });
        builder.HasIndex(x => x.ParentValueId);
        builder.HasIndex(x => x.ReferenceLegalEntityId);
        builder.HasIndex(x => x.ReferenceBranchId);
        builder.HasIndex(x => x.ReferenceCostCenterId);
        builder.HasIndex(x => x.ReferenceBusinessLineCode);

        builder.Property(x => x.SegmentValueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValueCode).HasMaxLength(30);
        builder.Property(x => x.ValueNameEn).HasMaxLength(240);
        builder.Property(x => x.ValueNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ReferenceBusinessLineCode).HasMaxLength(30);

        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ParentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegment>().WithMany().HasForeignKey(x => x.CoaSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.ReferenceBranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCostCenter>().WithMany().HasForeignKey(x => x.ReferenceCostCenterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.ReferenceLegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
