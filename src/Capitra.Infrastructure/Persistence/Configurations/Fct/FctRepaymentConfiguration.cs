using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctRepaymentConfiguration : IEntityTypeConfiguration<FctRepayment>
{
    public void Configure(EntityTypeBuilder<FctRepayment> builder)
    {
        builder.ToTable("FCT_REPAYMENTS");
        builder.HasIndex(x => new { x.DisbursementId, x.IsCurrentVersion, x.DueDate });
        builder.HasKey(x => x.ScheduleId);

        builder.Property(x => x.OpeningPrincipal).HasPrecision(20, 4);
        builder.Property(x => x.OutstandingPrincipal).HasPrecision(20, 4);
        builder.Property(x => x.OutstandingInterest).HasPrecision(20, 4);
        builder.Property(x => x.TotalInstallment).HasPrecision(20, 4);
        builder.Property(x => x.ClosingPrincipal).HasPrecision(20, 4);
        builder.Property(x => x.PaidPrincipal).HasPrecision(20, 4);
        builder.Property(x => x.PaidInterest).HasPrecision(20, 4);
        builder.Property(x => x.PaidFee).HasPrecision(20, 4);
        builder.Property(x => x.PaidPenalty).HasPrecision(20, 4);
        builder.Property(x => x.PrincipalDue).HasPrecision(20, 4);
        builder.Property(x => x.InterestDue).HasPrecision(20, 4);
        builder.Property(x => x.PenaltyDue).HasPrecision(20, 4);
        builder.Property(x => x.FeeDue).HasPrecision(20, 4);
        builder.Property(x => x.InterestRate).HasPrecision(9, 6);
        builder.Property(x => x.DayCountBasis).HasMaxLength(10);
        builder.Property(x => x.StatusCode).HasMaxLength(50);
        builder.Property(x => x.RecalcReason).HasMaxLength(500);
        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
