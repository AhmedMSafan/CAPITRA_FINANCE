using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlClosePeriodLockConfiguration : IEntityTypeConfiguration<GlClosePeriodLock>
{
    public void Configure(EntityTypeBuilder<GlClosePeriodLock> builder)
    {
        builder.ToTable("GL_CLOSE_PERIOD_LOCKS");
        builder.HasKey(x => x.PeriodLockId);
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.LockLevel, x.LockStatus }).IsUnique();
        builder.HasIndex(x => new { x.LedgerPeriodId, x.LockStatus });

        builder.Property(x => x.PeriodLockId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LockLevel).HasMaxLength(30);
        builder.Property(x => x.LockStatus).HasMaxLength(30);
        builder.Property(x => x.LockReason).HasMaxLength(1000);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.UnlockedBy).HasMaxLength(100);
        builder.Property(x => x.UnlockReason).HasMaxLength(1000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
