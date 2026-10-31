using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCurrencyExposureSnapshotConfiguration : IEntityTypeConfiguration<GlCurrencyExposureSnapshot>
{
    public void Configure(EntityTypeBuilder<GlCurrencyExposureSnapshot> builder)
    {
        builder.ToTable("GL_CURRENCY_EXPOSURE_SNAPSHOTS");
        builder.HasKey(x => x.ExposureSnapshotId);
        builder.HasIndex(x => x.SnapshotCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.SnapshotDate, x.SnapshotStatus });

        builder.Property(x => x.ExposureSnapshotId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SnapshotCode).HasMaxLength(50);
        builder.Property(x => x.SnapshotStatus).HasMaxLength(30);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
