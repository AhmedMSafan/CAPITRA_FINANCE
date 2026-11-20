using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCoaSegmentConfiguration : IEntityTypeConfiguration<GlCoaSegment>
{
    public void Configure(EntityTypeBuilder<GlCoaSegment> builder)
    {
        builder.ToTable("GL_COA_SEGMENTS");
        builder.HasKey(x => x.CoaSegmentId);
        builder.HasIndex(x => new { x.CoaId, x.SegmentNum }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.SegmentCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.SegmentType });
        builder.HasIndex(x => new { x.IsEnabled, x.DisplaySeq });

        builder.Property(x => x.CoaSegmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SegmentCode).HasMaxLength(30);
        builder.Property(x => x.SegmentNameEn).HasMaxLength(150);
        builder.Property(x => x.SegmentNameAr).HasMaxLength(150);
        builder.Property(x => x.SegmentType).HasMaxLength(30);
        builder.Property(x => x.ValueFormat).HasMaxLength(20);
        builder.Property(x => x.PadChar).HasMaxLength(1);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DefaultValueCode).HasMaxLength(120);
        builder.Property(x => x.DefaultMode).HasMaxLength(10);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
