using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpPaymentStatusConfiguration : IEntityTypeConfiguration<AdmLkpPaymentStatus>
{
    public void Configure(EntityTypeBuilder<AdmLkpPaymentStatus> builder)
    {
        builder.ToTable("ADM_LKP_PAYMENT_STATUSES");
        builder.HasKey(x => x.PaymentStatusCode);

        builder.Property(x => x.PaymentStatusCode).HasMaxLength(20);
        builder.Property(x => x.PaymentStatusAr).HasMaxLength(100);
        builder.Property(x => x.PaymentStatusEn).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
