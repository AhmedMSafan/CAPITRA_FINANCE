using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctRepaymentAllocationConfiguration : IEntityTypeConfiguration<FctRepaymentAllocation>
{
    public void Configure(EntityTypeBuilder<FctRepaymentAllocation> builder)
    {
        builder.ToTable("FCT_REPAYMENT_ALLOCATIONS");
        builder.HasKey(x => x.AllocationId);
        builder.HasIndex(x => new { x.RepaymentTxnId, x.StatusCode, x.AllocationDate });
        builder.HasIndex(x => new { x.TargetType, x.TargetId, x.ComponentCode, x.StatusCode });

        builder.Property(x => x.AllocationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TargetType).HasMaxLength(20);
        builder.Property(x => x.ComponentCode).HasMaxLength(20);
        builder.Property(x => x.AllocationMode).HasMaxLength(15);
        builder.Property(x => x.RequestedValue).HasPrecision(20, 6);
        builder.Property(x => x.AllocationAmount).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.ReversalReason).HasMaxLength(1000);
        builder.Property(x => x.ReversedBy).HasMaxLength(100);

        builder.HasOne<FctRepaymentTransaction>().WithMany().HasForeignKey(x => x.RepaymentTxnId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctRepaymentSource>().WithMany().HasForeignKey(x => x.RepaymentSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
