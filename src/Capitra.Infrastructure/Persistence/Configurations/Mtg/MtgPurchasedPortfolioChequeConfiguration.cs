using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioChequeConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioCheque>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioCheque> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_CHEQUES");
        builder.HasKey(x => x.ChequeId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.ChequeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChequeStatusCode).HasMaxLength(20);
        builder.Property(x => x.ReturnReason).HasMaxLength(1000);

        builder.HasOne<MtgPurchasedPortfolioAccount>().WithMany().HasForeignKey(x => x.PortfolioAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpChequeStatus>().WithMany().HasForeignKey(x => x.ChequeStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
