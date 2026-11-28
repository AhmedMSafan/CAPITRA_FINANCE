using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingLockConfiguration : IEntityTypeConfiguration<GlPostingLock>
{
    public void Configure(EntityTypeBuilder<GlPostingLock> builder)
    {
        builder.ToTable("GL_POSTING_LOCKS");
        builder.HasKey(x => x.PostingLockId);
        builder.HasIndex(x => new { x.LockType, x.LockKey, x.LockStatus }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.LockStatus });
        builder.HasIndex(x => new { x.QueueId, x.PostingRunId });

        builder.Property(x => x.PostingLockId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LockType).HasMaxLength(30);
        builder.Property(x => x.LockKey).HasMaxLength(200);
        builder.Property(x => x.LockStatus).HasMaxLength(30);
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.ReleasedBy).HasMaxLength(100);
        builder.Property(x => x.ReleaseReason).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingQueue>().WithMany().HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingRun>().WithMany().HasForeignKey(x => x.PostingRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
