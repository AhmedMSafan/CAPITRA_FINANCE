using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioSettlementConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioSettlement>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioSettlement> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_SETTLEMENTS");
        builder.HasKey(x => x.SettlementId);
        builder.HasIndex(x => x.PortfolioId).IsUnique();
        builder.HasIndex(x => new { x.SettlementStatusCode, x.AccountingStatusCode });

        builder.Property(x => x.SettlementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ApprovedPurchaseValue).HasPrecision(18, 2);
        builder.Property(x => x.PaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.AcquisitionCostAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.PaymentMethodCode).HasMaxLength(30);
        builder.Property(x => x.BankGlAccountCode).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);
        builder.Property(x => x.BankAccountNo).HasMaxLength(100);
        builder.Property(x => x.TransferReference).HasMaxLength(200);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.SellerPaymentReference).HasMaxLength(200);
        builder.Property(x => x.SettlementStatusCode).HasMaxLength(20);
        builder.Property(x => x.AccountingStatusCode).HasMaxLength(20);
        builder.Property(x => x.GlBatchReference).HasMaxLength(200);
        builder.Property(x => x.GlJournalReference).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.SettledBy).HasMaxLength(255);

        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpPortfolioPaymentMethod>().WithMany().HasForeignKey(x => x.PaymentMethodCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
