using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgSecurityDepositConfiguration : IEntityTypeConfiguration<MtgSecurityDeposit>
{
    public void Configure(EntityTypeBuilder<MtgSecurityDeposit> builder)
    {
        builder.ToTable("MTG_SECURITY_DEPOSITS");
        builder.HasKey(x => x.DepositId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.DepositId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BankName).HasMaxLength(300);
        builder.Property(x => x.AccountReference).HasMaxLength(200);
        builder.Property(x => x.DepositAmount).HasPrecision(18, 2);
        builder.Property(x => x.DepositPct).HasPrecision(9, 6);

        builder.HasOne<MtgPurchasedPortfolioDocument>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
