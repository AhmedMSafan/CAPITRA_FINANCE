using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndLoanConfiguration : IEntityTypeConfiguration<LndLoan>
{
    public void Configure(EntityTypeBuilder<LndLoan> builder)
    {
        builder.ToTable("LND_LOANS");
        builder.HasKey(x => x.LoanId);
        builder.HasIndex(x => x.LoanNumber).IsUnique();
        builder.HasIndex(x => new { x.ClientId, x.StatusCode });
        builder.HasIndex(x => new { x.StatusCode, x.LicenceCode });

        builder.Property(x => x.LoanId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LoanNumber).HasMaxLength(30);
        builder.Property(x => x.LicenceCode).HasMaxLength(10);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RepaymentFrequencyCode).HasMaxLength(20);
        builder.Property(x => x.InterestMethodCode).HasMaxLength(20);
        builder.Property(x => x.DayCountCode).HasMaxLength(20);
        builder.Property(x => x.LateFeeTypeCode).HasMaxLength(20);
        builder.Property(x => x.LateFeeBaseCode).HasMaxLength(30);
        builder.Property(x => x.PaymentOrder).HasMaxLength(200);
        builder.Property(x => x.PrepaymentRecalcCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmProduct>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LndProductVersion>().WithMany().HasForeignKey(x => x.ProductVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class LndInstallmentConfiguration : IEntityTypeConfiguration<LndInstallment>
{
    public void Configure(EntityTypeBuilder<LndInstallment> builder)
    {
        builder.ToTable("LND_INSTALLMENTS");
        builder.HasKey(x => x.InstallmentId);
        builder.HasIndex(x => new { x.LoanId, x.InstallmentNo }).IsUnique();
        builder.HasIndex(x => new { x.DueDate, x.StatusCode });

        builder.Property(x => x.InstallmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.HasOne<LndLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class LndLoanStatusHistConfiguration : IEntityTypeConfiguration<LndLoanStatusHist>
{
    public void Configure(EntityTypeBuilder<LndLoanStatusHist> builder)
    {
        builder.ToTable("LND_LOAN_STATUS_HIST");
        builder.HasKey(x => x.LoanStatusHistId);
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.LoanStatusHistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FromStatusCode).HasMaxLength(20);
        builder.Property(x => x.ToStatusCode).HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasOne<LndLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
