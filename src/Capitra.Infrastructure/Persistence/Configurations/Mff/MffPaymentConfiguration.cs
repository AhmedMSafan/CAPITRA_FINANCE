using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffPaymentConfiguration : IEntityTypeConfiguration<MffPayment>
{
    public void Configure(EntityTypeBuilder<MffPayment> builder)
    {
        builder.ToTable("MFF_PAYMENTS");
        builder.HasKey(x => x.PaymentId);
        builder.HasIndex(x => x.InstallmentId);
        builder.HasIndex(x => x.LoanId);
        builder.HasIndex(x => x.StatusCode);

        builder.Property(x => x.PaymentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentType).HasMaxLength(30);
        builder.Property(x => x.PaymentChannel).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.AllocatedPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.AllocatedInterest).HasPrecision(18, 2);
        builder.Property(x => x.AllocatedFees).HasPrecision(18, 2);
        builder.Property(x => x.AllocatedInsurance).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(50);

        builder.HasOne<MffInstallment>().WithMany().HasForeignKey(x => x.InstallmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpPaymentStatus>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
