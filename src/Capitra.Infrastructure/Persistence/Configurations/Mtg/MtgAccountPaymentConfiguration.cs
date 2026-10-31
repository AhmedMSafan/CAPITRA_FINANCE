using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgAccountPaymentConfiguration : IEntityTypeConfiguration<MtgAccountPayment>
{
    public void Configure(EntityTypeBuilder<MtgAccountPayment> builder)
    {
        builder.ToTable("MTG_ACCOUNT_PAYMENTS");
        builder.HasKey(x => x.PaymentId);
        builder.HasIndex(x => x.PortfolioAccountId);

        builder.Property(x => x.PaymentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DueAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentStatusCode).HasMaxLength(20);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);

        builder.HasOne<MtgPurchasedPortfolioAccount>().WithMany().HasForeignKey(x => x.PortfolioAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpPaymentStatus>().WithMany().HasForeignKey(x => x.PaymentStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
