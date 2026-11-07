using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioSnapshotConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioSnapshot>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioSnapshot> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_SNAPSHOTS");
        builder.HasKey(x => x.SnapshotId);
        builder.HasIndex(x => new { x.AsOfDate, x.PortfolioId }).IsUnique();

        builder.Property(x => x.SnapshotId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.PurchaseValue).HasPrecision(18, 2);
        builder.Property(x => x.ChequeCoverage).HasPrecision(9, 4);
        builder.Property(x => x.DocumentCompletionPct).HasPrecision(9, 6);
        builder.Property(x => x.CurrentStatus).HasMaxLength(30);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
