using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLoanRestructuringConfiguration : IEntityTypeConfiguration<MffLoanRestructuring>
{
    public void Configure(EntityTypeBuilder<MffLoanRestructuring> builder)
    {
        builder.ToTable("MFF_LOAN_RESTRUCTURINGS");
        builder.HasKey(x => x.RestructuringId);
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.RestructuringId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReasonAr).HasMaxLength(1000);
        builder.Property(x => x.OldRatePct).HasPrecision(9, 6);
        builder.Property(x => x.NewRatePct).HasPrecision(9, 6);
        builder.Property(x => x.OldInstallment).HasPrecision(18, 2);
        builder.Property(x => x.NewInstallment).HasPrecision(18, 2);
        builder.Property(x => x.CapitalizedArrears).HasPrecision(18, 2);
        builder.Property(x => x.LoanRestructuringStatus).HasMaxLength(30);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.NotesAr).HasMaxLength(2000);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
