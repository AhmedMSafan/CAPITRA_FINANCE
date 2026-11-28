using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpPortfolioPaymentMethodConfiguration : IEntityTypeConfiguration<MtgLkpPortfolioPaymentMethod>
{
    public void Configure(EntityTypeBuilder<MtgLkpPortfolioPaymentMethod> builder)
    {
        builder.ToTable("MTG_LKP_PORTFOLIO_PAYMENT_METHODS");
        builder.HasKey(x => x.PaymentMethodCode);

        builder.Property(x => x.PaymentMethodCode).HasMaxLength(30);
        builder.Property(x => x.PaymentMethodNameAr).HasMaxLength(100);
        builder.Property(x => x.PaymentMethodNameEn).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
