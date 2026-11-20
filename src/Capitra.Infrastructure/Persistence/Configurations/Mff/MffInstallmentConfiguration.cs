using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffInstallmentConfiguration : IEntityTypeConfiguration<MffInstallment>
{
    public void Configure(EntityTypeBuilder<MffInstallment> builder)
    {
        builder.ToTable("MFF_INSTALLMENTS");
        builder.HasKey(x => x.InstallmentId);
        builder.HasIndex(x => new { x.LoanId, x.InstallmentNo }).IsUnique();
        builder.HasIndex(x => x.LoanId);
        builder.HasIndex(x => x.PaymentStatusCode);

        builder.Property(x => x.InstallmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OpeningPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.PrincipalDue).HasPrecision(18, 2);
        builder.Property(x => x.InterestDue).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceDue).HasPrecision(18, 2);
        builder.Property(x => x.FeesDue).HasPrecision(18, 2);
        builder.Property(x => x.SubsidyDue).HasPrecision(18, 2);
        builder.Property(x => x.BorrowerDue).HasPrecision(18, 2);
        builder.Property(x => x.TotalDue).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentStatusCode).HasMaxLength(50);
        builder.Property(x => x.ClosingPrincipal).HasPrecision(18, 2);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpPaymentStatus>().WithMany().HasForeignKey(x => x.PaymentStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
