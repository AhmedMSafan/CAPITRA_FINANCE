using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLoanConfiguration : IEntityTypeConfiguration<MffLoan>
{
    public void Configure(EntityTypeBuilder<MffLoan> builder)
    {
        builder.ToTable("MFF_LOANS");
        builder.HasKey(x => x.LoanId);
        builder.HasIndex(x => x.LoanAccountNo).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.ContractId);

        builder.Property(x => x.LoanId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LoanAccountNo).HasMaxLength(100);
        builder.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(x => x.AnnualRatePct).HasPrecision(9, 6);
        builder.Property(x => x.InitialInstallment).HasPrecision(18, 2);
        builder.Property(x => x.InstallmentFrequency).HasMaxLength(20);
        builder.Property(x => x.LoanStatus).HasMaxLength(30);
        builder.Property(x => x.OutstandingPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.CoreSystemRef).HasMaxLength(200);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffContract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
