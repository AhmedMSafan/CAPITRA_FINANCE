using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLoanMonthlySnapshotConfiguration : IEntityTypeConfiguration<MffLoanMonthlySnapshot>
{
    public void Configure(EntityTypeBuilder<MffLoanMonthlySnapshot> builder)
    {
        builder.ToTable("MFF_LOAN_MONTHLY_SNAPSHOTS");
        builder.HasKey(x => x.SnapshotId);
        builder.HasIndex(x => new { x.SnapshotMonthDate, x.LoanId }).IsUnique();
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.SnapshotId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LoanStatus).HasMaxLength(30);
        builder.Property(x => x.OutstandingPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.OverdueAmount).HasPrecision(18, 2);
        builder.Property(x => x.SubsidyBalance).HasPrecision(18, 2);
        builder.Property(x => x.LastPaymentAmount).HasPrecision(18, 2);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
