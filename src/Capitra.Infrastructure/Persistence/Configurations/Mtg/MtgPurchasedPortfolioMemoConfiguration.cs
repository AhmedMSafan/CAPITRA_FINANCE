using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioMemoConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioMemo>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioMemo> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_MEMOS");
        builder.HasKey(x => x.MemoId);
        builder.HasIndex(x => new { x.PortfolioId, x.VersionNo }).IsUnique();
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.MemoId).UseIdentityByDefaultColumn();
        builder.Property(x => x.MemoStatus).HasMaxLength(30);
        builder.Property(x => x.PreparedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
