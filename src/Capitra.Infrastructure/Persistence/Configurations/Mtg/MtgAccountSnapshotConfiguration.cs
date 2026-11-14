using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgAccountSnapshotConfiguration : IEntityTypeConfiguration<MtgAccountSnapshot>
{
    public void Configure(EntityTypeBuilder<MtgAccountSnapshot> builder)
    {
        builder.ToTable("MTG_ACCOUNT_SNAPSHOTS");
        builder.HasKey(x => x.SnapshotId);
        builder.HasIndex(x => new { x.AsOfDate, x.PortfolioAccountId }).IsUnique();

        builder.Property(x => x.SnapshotId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.ArrearsAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentStatus).HasMaxLength(20);

        builder.HasOne<MtgPurchasedPortfolioAccount>().WithMany().HasForeignKey(x => x.PortfolioAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
