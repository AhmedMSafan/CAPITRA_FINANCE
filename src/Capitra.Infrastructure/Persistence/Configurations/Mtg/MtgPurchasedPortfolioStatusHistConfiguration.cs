using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioStatusHistConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioStatusHist>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioStatusHist> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_STATUS_HIST");
        builder.HasKey(x => x.StatusHistId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.StatusHistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FromStatus).HasMaxLength(30);
        builder.Property(x => x.ToStatus).HasMaxLength(30);
        builder.Property(x => x.ChangedBy).HasMaxLength(255);
        builder.Property(x => x.Comments).HasMaxLength(2000);

        builder.HasOne<MtgLkpPortfolioStatus>().WithMany().HasForeignKey(x => x.FromStatus).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpPortfolioStatus>().WithMany().HasForeignKey(x => x.ToStatus).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
