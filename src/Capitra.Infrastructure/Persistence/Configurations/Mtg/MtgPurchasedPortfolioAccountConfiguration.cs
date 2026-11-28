using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioAccountConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioAccount>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioAccount> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_ACCOUNTS");
        builder.HasKey(x => x.PortfolioAccountId);
        builder.HasIndex(x => new { x.PortfolioId, x.InvestorId, x.UnitId }).IsUnique();
        builder.HasIndex(x => x.InvestorId);
        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => new { x.PortfolioId, x.SellerContractNo });

        builder.Property(x => x.PortfolioAccountId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ContractValue).HasPrecision(18, 2);
        builder.Property(x => x.PaidValue).HasPrecision(18, 2);
        builder.Property(x => x.OutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.InstallmentAmount).HasPrecision(18, 2);
        builder.Property(x => x.InstallmentFrequency).HasMaxLength(30);
        builder.Property(x => x.ArrearsAmount).HasPrecision(18, 2);
        builder.Property(x => x.AccountStatus).HasMaxLength(30);
        builder.Property(x => x.SellerContractNo).HasMaxLength(100);
        builder.Property(x => x.DealDecisionCode).HasMaxLength(30);

        builder.HasOne<MtgInvestor>().WithMany().HasForeignKey(x => x.InvestorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProjectUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
