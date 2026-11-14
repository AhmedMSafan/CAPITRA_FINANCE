using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioTranSetCounterConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioTranSetCounter>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioTranSetCounter> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_TRAN_SET_COUNTERS");
        builder.HasKey(x => new { x.PortfolioNo, x.YearNo });
        builder.HasIndex(x => new { x.PortfolioNo, x.YearNo }).IsUnique();

        builder.Property(x => x.PortfolioNo).HasMaxLength(30);

        builder.ConfigureAudit();
    }
}
